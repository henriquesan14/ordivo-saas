# Ordivo

Monólito modular para gestão de clientes e ordens de serviço em ASP.NET Core e .NET 10.

## Arquitetura

- `Ordivo.SharedKernel`: blocos comuns (`Result`, `Error`, `Entity`, `AggregateRoot`, eventos de domínio, `ICommand` e `IQuery`).
- `Ordivo.Domain`: agregados, entidades, regras e eventos do domínio; não depende de aplicação ou infraestrutura.
- `Ordivo.Application`: somente commands, queries, handlers CQRS, DTOs, extensions e abstrações; não utiliza classes `Service`.
- `Ordivo.Infrastructure`: Entity Framework Core, PostgreSQL, mapeamentos e repositórios.
- `Ordivo.Api`: composição, transporte HTTP e conversão de `Result` em respostas HTTP.

As dependências apontam para dentro: `Api -> Application -> Domain -> SharedKernel`. A infraestrutura implementa abstrações definidas pela aplicação.

## Vertical slices

Cada operação possui sua própria pasta. Exemplos:

```text
Application/
  Customers/
    CreateCustomer/
      CreateCustomer.cs
    GetCustomer/
      GetCustomer.cs
    ListCustomers/
      ListCustomers.cs
```

Os agregados são convertidos com extensions `ToDto()` e `ToListDto()`, mantendo o mapeamento fora dos handlers e dos objetos de domínio.

## Executar e testar

```powershell
dotnet run --project src/Ordivo.Api
dotnet test Ordivo.slnx
```

## Banco de dados

Configure `ConnectionStrings__OrdivoDatabase` e execute:

```powershell
dotnet ef database update --project src/Ordivo.Infrastructure --startup-project src/Ordivo.Api
```

Em desenvolvimento, a configuração padrão espera PostgreSQL local com banco `ordivo`, usuário `postgres` e senha `postgres`.

## Auditoria e autenticação

O interceptor do EF Core preenche `CreatedAt`, `CreatedByName`, `UpdatedAt` e `UpdatedByName`. Alterações em owned entities também marcam o agregado como atualizado. Clientes, ordens, usuários e tenants implementam `IAuditableEntity` por meio de `AggregateRoot`.

Os endpoints em `/api/customers` e `/api/service-orders` exigem autenticação. O JWT é transportado no cookie `ordivo.access_token`, configurado com `HttpOnly`, `SameSite=Strict` e `Secure=true` em produção. Configure em produção:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Key
```

`Jwt__Key` deve possuir pelo menos 32 caracteres e ser armazenada em um secret manager. A chave presente em `appsettings.Development.json` é somente para desenvolvimento local. O endpoint `/health` permanece anônimo.

## Identidade

- `POST /api/auth/register`: cria o tenant e seu primeiro `Owner`, envia a verificação e retorna `202`; nenhum cookie de autenticação é emitido antes da confirmação do email.
- `POST /api/auth/verify-email` e `/resend-verification`: confirmam ou reenviam a verificação de email.
- `POST /api/auth/forgot-password` e `/reset-password`: recuperam a senha sem revelar se o email existe.
- `POST /api/auth/invitations/accept`: define a senha e ativa um usuário convidado.
- `POST /api/auth/login`: valida email, senha e email verificado, e grava os cookies de autenticação.
- `POST /api/auth/refresh`: rotaciona o refresh token e renova o access token.
- `POST /api/auth/logout`: revoga a sessão no banco e remove os cookies.
- `GET /api/auth/sessions`: lista as sessões do usuário autenticado sem expor tokens ou hashes.
- `DELETE /api/auth/sessions/{id}`: revoga uma sessão pertencente ao usuário autenticado.
- `IGenerateToken`: abstração da Application implementada pela infraestrutura JWT.
- `IPasswordHasher`: abstração da Application implementada com `PasswordHasher` do ASP.NET Core.
- `IUserContext`: disponibiliza `UserId`, email, papel e estado de autenticação a partir das claims da requisição.

O frontend não recebe o token no JSON nem precisa manipulá-lo. O navegador envia o cookie `HttpOnly` automaticamente nas próximas requisições. Requisições cross-origin devem incluir credenciais (`credentials: "include"` no `fetch`).

O access token expira em 15 minutos e fica no cookie `ordivo.access_token`. O refresh token expira em 30 dias e fica no cookie `ordivo.refresh_token`. Ambos são `HttpOnly`; em produção também são `Secure`. Refresh tokens são aleatórios, rotacionados a cada uso e persistidos somente como hash SHA-256 na tabela `auth_sessions`. A reutilização de um token rotacionado revoga toda a família. Troca ou recuperação de senha e desativação do usuário revogam todas as sessões ativas. O PlatformAdmin renova sua sessão em `POST /api/platform/auth/refresh`.

## Segurança HTTP

Antes de qualquer `POST`, `PUT`, `PATCH` ou `DELETE`, o frontend deve obter um token em `GET /api/auth/csrf` e enviar o valor retornado no header `X-CSRF-TOKEN`. O cookie antiforgery é mantido pelo navegador e não é acessível ao JavaScript.

```javascript
const csrfResponse = await fetch(`${apiUrl}/api/auth/csrf`, {
  credentials: "include"
});
const { token } = await csrfResponse.json();

await fetch(`${apiUrl}/api/platform/auth/login`, {
  method: "POST",
  credentials: "include",
  headers: {
    "Content-Type": "application/json",
    "X-CSRF-TOKEN": token
  },
  body: JSON.stringify({ email, password })
});
```

O CORS aceita somente origens configuradas em `Cors__AllowedOrigins__N` e permite credenciais; nunca combine cookies com `AllowAnyOrigin`. O Compose usa `CORS_ORIGIN=http://localhost:4200` por padrão.

O rate limiting usa uma janela fixa de 60 segundos: 120 requisições globais, 5 tentativas nos endpoints de autenticação e 20 renovações de sessão, particionadas por usuário autenticado ou endereço IP. Respostas bloqueadas usam HTTP `429` e incluem o header `Retry-After`.

O registro recebe `tenantName`, `name`, `email` e `password`. Ele cria o tenant e seu primeiro usuário `Owner` na mesma unidade de trabalho.

## Clientes

- `POST /api/customers`: cria um cliente.
- `GET /api/customers/{id}`: consulta um cliente.
- `PUT /api/customers/{id}`: atualiza nome, documento, telefone e email.
- `PATCH /api/customers/{id}/status`: ativa ou desativa sem excluir o histórico.
- `GET /api/customers`: retorna `PagedResult<CustomerDto>`.

A listagem aceita `name`, `document`, `email`, `phone`, `includeInactive`, `page`, `pageSize`, `sortBy` e `descending`. O tamanho máximo de página é 100; clientes inativos são omitidos por padrão. Os campos de ordenação são `name`, `document`, `email`, `phone`, `createdAt` e `updatedAt`.

## Multi-tenancy

- O JWT contém as claims `sub` e `tenant_id`.
- `IUserContext` disponibiliza `UserId` e `TenantId` para commands e queries.
- Usuários, clientes e ordens de serviço possuem `TenantId` obrigatório.
- Clientes e ordens usam filtros globais do EF Core pelo tenant autenticado.
- O documento do cliente é único dentro do tenant, não globalmente.
- O nome do tenant pode se repetir; cada tenant possui um `Slug` estável e único, gerado automaticamente.
- Requisições sem uma claim `tenant_id` válida não acessam dados tenant-scoped.

## Usuários do tenant

O módulo `/api/users` é isolado pelo `TenantId` autenticado:

- `GET /api/users`: lista os usuários do tenant.
- `GET /api/users/{id}`: consulta um usuário do tenant.
- `POST /api/users`: cria usuário; exige `Owner` ou `Admin`.
- `POST /api/users/invitations`: cria um usuário inativo e envia um convite; exige `Owner` ou `Admin`.
- `PATCH /api/users/{id}/role`: altera papel; exige `Owner`.
- `PATCH /api/users/{id}/status`: ativa ou desativa; exige `Owner` ou `Admin`.
- `PUT /api/users/me/password`: troca a própria senha após validar a senha atual.

Um `Admin` não pode administrar um `Owner`, o próprio usuário não pode se desativar e o último Owner ativo não pode ser rebaixado nem desativado. Senhas são persistidas somente como hash.

O módulo de tenants oferece vertical slices sem services:

- `GET /api/tenant`: retorna o tenant da claim `tenant_id`.
- `PUT /api/tenant`: altera o nome do tenant autenticado.

`Tenant`, `User`, `Customer` e `ServiceOrder` usam filtros globais. Login e verificação de email ignoram o filtro explicitamente porque acontecem antes da emissão do token.

## Docker

Copie as variáveis de exemplo, ajuste senhas/chaves e inicie:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

A API ficará em `http://localhost:8080` e o PostgreSQL em `localhost:5432`. O volume `postgres-data` preserva os dados e `data-protection-keys` preserva as chaves criptográficas usadas pelo antiforgery entre reinicializações.

Configure `SMTP_HOST`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_EMAIL` e `FRONTEND_BASE_URL` para entrega real de emails. Sem `SMTP_HOST`, os links de verificação, recuperação e convite são registrados no log da API para desenvolvimento local.

Em desenvolvimento, a documentação interativa Scalar fica em `http://localhost:8080/scalar/v1` e o documento OpenAPI em `http://localhost:8080/openapi/v1.json`.

No Compose, `Database__ApplyMigrations=true` aplica as migrations antes de iniciar o pipeline HTTP. Em produção, mantenha essa opção desabilitada e execute migrations em uma etapa única do deploy para evitar concorrência entre réplicas.

## Administração global

O administrador da plataforma usa `PlatformUser`, separado dos usuários de tenant. Ele não possui `TenantId` e recebe a claim `platform_role`.

Configure o primeiro administrador por secret/variável de ambiente:

```text
PlatformAdmin__Name
PlatformAdmin__Email
PlatformAdmin__Password
```

A senha inicial precisa ter pelo menos 12 caracteres. O seed é idempotente e não substitui credenciais existentes.

Em desenvolvimento, `appsettings.Development.json` contém uma credencial conhecida apenas para facilitar o primeiro acesso. O seed entrega a senha ao `PasswordHasher` e persiste somente seu hash. Em produção, sobrescreva `PlatformAdmin__Password` por secret manager e nunca versione a senha real.

- `POST /api/platform/auth/login`: login de usuário da plataforma.
- `GET /api/platform/tenants`: lista tenants usando a policy `PlatformAdmin`.
- `POST /api/platform/tenants`: cria um tenant e seu primeiro Owner.
- `GET /api/platform/tenants/{id}`: consulta um tenant por identificador.
- `GET /api/platform/tenants/by-slug/{slug}`: consulta um tenant pelo slug.
- `PUT /api/platform/tenants/{id}`: atualiza o nome do tenant.
- `PATCH /api/platform/tenants/{id}/status`: ativa ou suspende o tenant.

Ao suspender um tenant, todas as suas sessões ativas são revogadas. Login, refresh token, tokens JWT já emitidos e fluxos de identidade são bloqueados enquanto o tenant permanecer inativo. A reativação permite novos logins, mas não restaura sessões revogadas.

Não existe registro público de administrador global. O bypass de filtros ocorre somente no `IPlatformTenantRepository`; repositórios normais continuam tenant-scoped.
