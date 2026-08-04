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

Os endpoints em `/api/customers` e `/api/service-orders` exigem um token JWT Bearer válido. Configure em produção:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Key
```

`Jwt__Key` deve possuir pelo menos 32 caracteres e ser armazenada em um secret manager. A chave presente em `appsettings.Development.json` é somente para desenvolvimento local. O endpoint `/health` permanece anônimo.

## Identidade

- `POST /api/auth/register`: cria o primeiro usuário com papel `Owner`, armazena somente o hash da senha e retorna um JWT.
- `POST /api/auth/login`: valida email e senha e retorna um novo JWT.
- `IGenerateToken`: abstração da Application implementada pela infraestrutura JWT.
- `IPasswordHasher`: abstração da Application implementada com `PasswordHasher` do ASP.NET Core.
- `IUserContext`: disponibiliza `UserId`, email, papel e estado de autenticação a partir das claims da requisição.

Os demais endpoints exigem o header `Authorization: Bearer <token>`.

O registro recebe `tenantName`, `name`, `email` e `password`. Ele cria o tenant e seu primeiro usuário `Owner` na mesma unidade de trabalho.

## Multi-tenancy

- O JWT contém as claims `sub` e `tenant_id`.
- `IUserContext` disponibiliza `UserId` e `TenantId` para commands e queries.
- Usuários, clientes e ordens de serviço possuem `TenantId` obrigatório.
- Clientes e ordens usam filtros globais do EF Core pelo tenant autenticado.
- O documento do cliente é único dentro do tenant, não globalmente.
- Requisições sem uma claim `tenant_id` válida não acessam dados tenant-scoped.

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

A API ficará em `http://localhost:8080` e o PostgreSQL em `localhost:5432`. O volume `postgres-data` preserva os dados entre reinicializações.

No Compose, `Database__ApplyMigrations=true` aplica as migrations antes de iniciar o pipeline HTTP. Em produção, mantenha essa opção desabilitada e execute migrations em uma etapa única do deploy para evitar concorrência entre réplicas.
