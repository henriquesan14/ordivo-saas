import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { finalize } from 'rxjs';
import { Api, errorMessage } from '../core/api';
import { Auth } from '../core/auth';
import { Plan, PlatformTenant } from '../core/models';

@Component({
  selector: 'app-platform-login',
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<main class="platform-auth">
    <section>
      <a class="platform-brand" routerLink="/platform/login"
        ><span>O</span>
        <div>Ordivo<small>PLATFORM</small></div></a
      >
      <div class="platform-copy">
        <p class="eyebrow">ADMINISTRAÇÃO GLOBAL</p>
        <h1>Controle a plataforma<br />de um só lugar.</h1>
        <p>Tenants, planos, assinaturas e saúde da operação.</p>
      </div>
      <footer>Acesso restrito e auditado</footer>
    </section>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <div class="mobile-platform platform-brand">
        <span>O</span>
        <div>Ordivo<small>PLATFORM</small></div>
      </div>
      <p class="eyebrow">ÁREA RESTRITA</p>
      <h2>Entrar como PlatformAdmin</h2>
      <p class="muted">Use suas credenciais globais da plataforma.</p>
      <label>Email<input formControlName="email" type="email" autocomplete="email" /></label
      ><label
        >Senha<input formControlName="password" type="password" autocomplete="current-password"
      /></label>
      @if (error()) {
        <div class="alert">{{ error() }}</div>
      }
      <button class="primary wide" [disabled]="form.invalid || loading()">
        {{ loading() ? 'Autenticando...' : 'Acessar plataforma' }} →</button
      ><a class="back" routerLink="/login">← Login de empresa</a>
    </form>
  </main>`,
  styles: [
    `
      .platform-auth {
        min-height: 100vh;
        display: grid;
        grid-template-columns: 1fr 1fr;
        background: #f5f6f2;
      }
      .platform-auth > section {
        padding: 50px 7vw;
        background: linear-gradient(145deg, #0b2026, #153c3a);
        color: #fff;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
      }
      .platform-brand {
        display: flex;
        align-items: center;
        gap: 12px;
        font-size: 21px;
        font-weight: 800;
      }
      .platform-brand > span {
        display: grid;
        place-items: center;
        width: 35px;
        height: 35px;
        background: #dfff6c;
        color: #15352f;
        border-radius: 10px;
        font-family: Georgia;
        font-style: italic;
      }
      .platform-brand small {
        display: block;
        color: #83a7a0;
        font-size: 7px;
        letter-spacing: 0.23em;
      }
      .platform-copy h1 {
        font-size: 50px;
        line-height: 1.04;
        letter-spacing: -0.05em;
        margin: 17px 0;
      }
      .platform-copy > p:last-child {
        color: #a9c1bd;
        max-width: 410px;
      }
      .platform-auth footer {
        font-size: 11px;
        color: #789b95;
      }
      .platform-auth form {
        align-self: center;
        width: min(420px, calc(100% - 50px));
        justify-self: center;
      }
      .platform-auth h2 {
        font-size: 30px;
        letter-spacing: -0.04em;
        margin: 9px 0;
      }
      .muted {
        color: #7e8c89;
        margin-bottom: 35px;
        font-size: 13px;
      }
      .platform-auth label {
        font-size: 12px;
        font-weight: 700;
        display: block;
        margin: 18px 0;
      }
      .platform-auth input {
        display: block;
        width: 100%;
        height: 50px;
        border: 1px solid #d9e0da;
        border-radius: 9px;
        margin-top: 8px;
        padding: 0 13px;
        outline: 0;
      }
      .wide {
        width: 100%;
        margin-top: 8px;
      }
      .back {
        display: block;
        text-align: center;
        font-size: 12px;
        color: #57706c;
        margin-top: 24px;
      }
      .mobile-platform {
        display: none;
      }
      @media (max-width: 760px) {
        .platform-auth {
          grid-template-columns: 1fr;
        }
        .platform-auth > section {
          display: none;
        }
        .platform-auth form {
          padding-top: 12vh;
          align-self: start;
        }
        .mobile-platform {
          display: flex;
          margin-bottom: 50px;
        }
      }
    `,
  ],
})
export class PlatformLoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);
  private router = inject(Router);
  loading = signal(false);
  error = signal('');
  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
  submit() {
    this.loading.set(true);
    this.error.set('');
    this.auth
      .platformLogin(this.form.getRawValue().email, this.form.getRawValue().password)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.router.navigateByUrl('/platform'),
        error: (e) => this.error.set(errorMessage(e)),
      });
  }
}

@Component({
  selector: 'app-platform-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="platform-shell">
    <aside>
      <a class="p-logo" routerLink="/platform"
        ><span>O</span>
        <div>Ordivo<small>PLATFORM</small></div></a
      >
      <nav>
        <a
          routerLink="/platform"
          [routerLinkActiveOptions]="{ exact: true }"
          routerLinkActive="active"
          >⌂ Visão geral</a
        ><a routerLink="/platform/tenants" routerLinkActive="active">▦ Tenants</a
        ><a routerLink="/platform/plans" routerLinkActive="active">◇ Planos</a>
      </nav>
      <button (click)="auth.logout()">↗ Sair</button>
    </aside>
    <section>
      <header>
        <div>
          <small>ADMINISTRAÇÃO GLOBAL</small><strong>{{ auth.user()?.name }}</strong>
        </div>
        <span>{{ initials() }}</span>
      </header>
      <main><router-outlet /></main>
    </section>
  </div>`,
  styles: [
    `
      .platform-shell {
        min-height: 100vh;
        background: #f3f5f1;
      }
      .platform-shell aside {
        position: fixed;
        inset: 0 auto 0 0;
        width: 230px;
        background: #0e292d;
        padding: 27px 17px;
        color: #c6d7d4;
        display: flex;
        flex-direction: column;
      }
      .p-logo {
        display: flex;
        gap: 10px;
        align-items: center;
        color: #fff;
        font-weight: 800;
        padding: 0 10px 32px;
      }
      .p-logo > span {
        width: 32px;
        height: 32px;
        border-radius: 9px;
        background: #dfff6c;
        color: #15352f;
        display: grid;
        place-items: center;
        font-family: Georgia;
      }
      .p-logo small {
        display: block;
        font-size: 7px;
        letter-spacing: 0.2em;
        color: #729994;
      }
      .platform-shell nav {
        display: grid;
        gap: 5px;
      }
      .platform-shell nav a,
      .platform-shell aside > button {
        padding: 11px 13px;
        border-radius: 8px;
        font-size: 12px;
        color: #91aaa6;
        border: 0;
        background: transparent;
        text-align: left;
      }
      .platform-shell nav a.active {
        background: #214147;
        color: #fff;
      }
      .platform-shell aside > button {
        margin-top: auto;
      }
      .platform-shell > section {
        margin-left: 230px;
      }
      .platform-shell header {
        height: 68px;
        background: #fff;
        border-bottom: 1px solid #e0e6e0;
        display: flex;
        align-items: center;
        justify-content: flex-end;
        gap: 12px;
        padding: 0 27px;
      }
      .platform-shell header div {
        display: grid;
        text-align: right;
      }
      .platform-shell header small {
        font-size: 8px;
        color: #8b9795;
      }
      .platform-shell header strong {
        font-size: 12px;
      }
      .platform-shell header > span {
        width: 34px;
        height: 34px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: #173e3b;
        color: #fff;
        font-size: 11px;
        font-weight: 800;
      }
      .platform-shell main {
        padding: 29px;
      }
      @media (max-width: 720px) {
        .platform-shell aside {
          position: static;
          width: 100%;
          height: 66px;
          flex-direction: row;
          align-items: center;
          padding: 10px 15px;
        }
        .p-logo {
          padding: 0;
          margin-right: auto;
        }
        .platform-shell nav {
          display: flex;
        }
        .platform-shell nav a {
          font-size: 0;
        }
        .platform-shell nav a:first-letter {
          font-size: 17px;
        }
        .platform-shell aside > button {
          margin: 0;
          width: auto;
        }
        .platform-shell > section {
          margin-left: 0;
        }
        .platform-shell header {
          display: none;
        }
      }
    `,
  ],
})
export class PlatformShellComponent {
  auth = inject(Auth);
  initials = () =>
    this.auth
      .user()
      ?.name.split(' ')
      .map((x) => x[0])
      .slice(0, 2)
      .join('')
      .toUpperCase() ?? 'PA';
}

@Component({
  selector: 'app-platform-overview',
  imports: [CurrencyPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="page-head">
      <div>
        <p class="eyebrow">PLATAFORMA</p>
        <h1>Visão geral</h1>
        <p>Acompanhe o crescimento da sua base SaaS.</p>
      </div>
    </div>
    <section class="metrics">
      <article>
        <div class="metric-icon green">▦</div>
        <small>Tenants</small><strong>{{ tenants().length }}</strong
        ><span>{{ active() }} ativos</span>
      </article>
      <article>
        <div class="metric-icon blue">◇</div>
        <small>Planos ativos</small><strong>{{ plans().length }}</strong
        ><span>No catálogo</span>
      </article>
      <article>
        <div class="metric-icon yellow">%</div>
        <small>Taxa ativa</small><strong>{{ rate() }}%</strong><span>Da base total</span>
      </article>
      <article>
        <div class="metric-icon purple">R$</div>
        <small>Ticket de catálogo</small
        ><strong>{{ average() | currency: 'BRL' : 'symbol' : '1.0-0' }}</strong
        ><span>Média dos planos</span>
      </article>
    </section>
    <article class="card">
      <div class="card-head">
        <div>
          <h2>Tenants recentes</h2>
          <p>Últimas contas provisionadas</p>
        </div>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>EMPRESA</th>
              <th>SLUG</th>
              <th>STATUS</th>
            </tr>
          </thead>
          <tbody>
            @for (t of tenants().slice(0, 8); track t.id) {
              <tr>
                <td>
                  <b>{{ t.name }}</b>
                </td>
                <td>{{ t.slug }}</td>
                <td>
                  <span class="status" [class.completed]="t.isActive">{{
                    t.isActive ? 'Ativo' : 'Suspenso'
                  }}</span>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </article>`,
})
export class PlatformOverviewComponent {
  private api = inject(Api);
  tenants = signal<PlatformTenant[]>([]);
  plans = signal<Plan[]>([]);
  constructor() {
    this.api.get<PlatformTenant[]>('/api/platform/tenants').subscribe((x) => this.tenants.set(x));
    this.api.get<Plan[]>('/api/platform/plans').subscribe((x) => this.plans.set(x));
  }
  active = () => this.tenants().filter((x) => x.isActive).length;
  rate = () =>
    this.tenants().length ? Math.round((this.active() / this.tenants().length) * 100) : 0;
  average = () =>
    this.plans().length ? this.plans().reduce((n, x) => n + x.price, 0) / this.plans().length : 0;
}

@Component({
  selector: 'app-platform-tenants',
  imports: [DatePipe, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="page-head">
      <div>
        <p class="eyebrow">CLIENTES SAAS</p>
        <h1>Tenants</h1>
        <p>Administre organizações e acesso à plataforma.</p>
      </div>
      <button class="primary" (click)="showForm.set(!showForm())">+ Novo tenant</button>
    </div>
    @if (showForm()) {
      <form class="card tenant-form" [formGroup]="form" (ngSubmit)="create()">
        <div class="form-title">
          <div>
            <h2>Provisionar tenant</h2>
            <p>Cria a empresa, o primeiro Owner e inicia o trial padrão.</p>
          </div>
          <button type="button" class="close" (click)="showForm.set(false)">×</button>
        </div>
        <label
          >Nome da empresa<input
            formControlName="tenantName"
            placeholder="Ex.: Oficina Central" /></label
        ><label
          >Nome do responsável<input
            formControlName="ownerName"
            placeholder="Nome completo" /></label
        ><label
          >Email do Owner<input
            formControlName="ownerEmail"
            type="email"
            placeholder="owner@empresa.com" /></label
        ><label
          >Senha inicial<input
            formControlName="ownerPassword"
            type="password"
            placeholder="Mínimo de 12 caracteres"
        /></label>
        @if (error()) {
          <div class="alert form-error">{{ error() }}</div>
        }
        <div class="form-actions">
          <button type="button" class="secondary" (click)="showForm.set(false)">Cancelar</button
          ><button class="primary" [disabled]="form.invalid || saving()">
            {{ saving() ? 'Criando...' : 'Criar tenant' }}
          </button>
        </div>
      </form>
    }
    @if (selected(); as tenant) {
      <form class="card impersonate-form" (submit)="$event.preventDefault(); confirmImpersonation()">
        <div>
          <p class="eyebrow">ACESSO DE SUPORTE</p>
          <h2>Acessar {{ tenant.name }}</h2>
          <p>A sessão dura 15 minutos e todas as alterações serão auditadas.</p>
        </div>
        <label>Motivo obrigatório<textarea [formControl]="reason" placeholder="Descreva o chamado ou motivo do acesso (mínimo 10 caracteres)"></textarea><small>{{ reason.value.length }}/500 caracteres · mínimo 10</small></label>
        @if (reason.touched && reason.invalid) { <div class="field-error">Informe um motivo com pelo menos 10 caracteres.</div> }
        @if (impersonationError()) { <div class="alert">{{ impersonationError() }}</div> }
        <div class="form-actions">
          <button type="button" class="secondary" (click)="selected.set(null)">Cancelar</button>
          <button type="submit" class="primary" [disabled]="impersonating()">{{ impersonating() ? 'Iniciando...' : 'Iniciar acesso seguro' }}</button>
        </div>
      </form>
    }
    <article class="card table-wrap">
      <table>
        <thead>
          <tr>
            <th>EMPRESA</th>
            <th>SLUG</th>
            <th>STATUS</th>
            <th>CRIADO EM</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (t of items(); track t.id) {
            <tr>
              <td>
                <b>{{ t.name }}</b>
              </td>
              <td>{{ t.slug }}</td>
              <td>
                <span class="status" [class.completed]="t.isActive">{{
                  t.isActive ? 'Ativo' : 'Suspenso'
                }}</span>
              </td>
              <td>{{ t.createdAt | date: 'dd/MM/yyyy' }}</td>
              <td>
                <div class="row-actions"><button class="table-action support" [disabled]="!t.isActive" (click)="start(t)">Acessar</button><button class="table-action" (click)="toggle(t)">
                  {{ t.isActive ? 'Suspender' : 'Ativar' }}
                </button></div>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="5" class="empty">Nenhum tenant provisionado.</td>
            </tr>
          }
        </tbody>
      </table>
    </article>`,
  styles: [
    `
      .tenant-form {
        padding: 22px;
        margin: -10px 0 22px;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 17px;
      }
      .impersonate-form{padding:22px;margin:-10px 0 22px;display:grid;grid-template-columns:1fr 1.4fr;gap:18px;align-items:end}.impersonate-form h2{font-size:18px;margin:7px 0}.impersonate-form p{font-size:11px;color:#74837f;margin:0}.impersonate-form label{font-size:11px;font-weight:800}.impersonate-form label small{display:block;margin-top:5px;color:#74837f;font-weight:500;text-align:right}.impersonate-form textarea{display:block;width:100%;min-height:78px;margin-top:7px;border:1px solid #d9e1da;border-radius:8px;padding:10px;resize:vertical;font:inherit}.impersonate-form .alert,.impersonate-form .field-error,.impersonate-form .form-actions{grid-column:1/-1}.field-error{color:#a12d2d;font-size:12px;font-weight:700}.row-actions{display:flex;gap:7px}.support{background:#173f39;color:#fff;border-color:#173f39}.support:disabled{opacity:.4}
      .form-title {
        grid-column: 1/-1;
        display: flex;
        justify-content: space-between;
        border-bottom: 1px solid #e7ebe6;
        padding-bottom: 14px;
      }
      .form-title h2 {
        font-size: 17px;
        margin: 0 0 4px;
      }
      .form-title p {
        font-size: 11px;
        color: #7d8987;
        margin: 0;
      }
      .close {
        border: 0;
        background: transparent;
        font-size: 23px;
        color: #7e8a88;
      }
      .tenant-form label {
        font-size: 11px;
        font-weight: 800;
        color: #52645f;
      }
      .tenant-form input {
        display: block;
        width: 100%;
        height: 43px;
        margin-top: 7px;
        border: 1px solid #d9e1da;
        border-radius: 8px;
        padding: 0 12px;
        outline: 0;
      }
      .tenant-form input:focus {
        border-color: #3e766c;
        box-shadow: 0 0 0 3px #3e766c13;
      }
      .form-error {
        grid-column: 1/-1;
        margin: 0;
      }
      .form-actions {
        grid-column: 1/-1;
        display: flex;
        justify-content: flex-end;
        gap: 9px;
      }
      .secondary {
        border: 1px solid #d6ded7;
        background: #fff;
        border-radius: 8px;
        padding: 0 15px;
        font-size: 12px;
        font-weight: 700;
      }
      @media (max-width: 650px) {
        .tenant-form {
          grid-template-columns: 1fr;
        }
      }
    `,
  ],
})
export class PlatformTenantsComponent {
  private api = inject(Api);
  private fb = inject(FormBuilder);
  private auth = inject(Auth);
  private router = inject(Router);
  items = signal<PlatformTenant[]>([]);
  showForm = signal(false);
  saving = signal(false);
  error = signal('');
  selected = signal<PlatformTenant|null>(null);
  impersonating = signal(false);
  impersonationError = signal('');
  reason = this.fb.nonNullable.control('',[Validators.required,Validators.minLength(10),Validators.maxLength(500)]);
  form = this.fb.nonNullable.group({
    tenantName: ['', Validators.required],
    ownerName: ['', Validators.required],
    ownerEmail: ['', [Validators.required, Validators.email]],
    ownerPassword: ['', [Validators.required, Validators.minLength(12)]],
  });
  constructor() {
    this.load();
  }
  load() {
    this.api.get<PlatformTenant[]>('/api/platform/tenants').subscribe((x) => this.items.set(x));
  }
  create() {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set('');
    this.api
      .post('/api/platform/tenants', this.form.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.form.reset();
          this.showForm.set(false);
          this.load();
        },
        error: (e) => this.error.set(errorMessage(e)),
      });
  }
  toggle(t: PlatformTenant) {
    this.api
      .patch(`/api/platform/tenants/${t.id}/status`, { isActive: !t.isActive })
      .subscribe(() => this.load());
  }
  start(t:PlatformTenant){this.selected.set(t);this.reason.reset();this.impersonationError.set('');}
  confirmImpersonation(){const tenant=this.selected();this.reason.markAsTouched();this.impersonationError.set('');if(!tenant)return;if(this.reason.invalid){this.impersonationError.set('Informe um motivo com pelo menos 10 caracteres para iniciar o acesso.');return;}this.impersonating.set(true);this.auth.startImpersonation(tenant.id,this.reason.value.trim()).pipe(finalize(()=>this.impersonating.set(false))).subscribe({next:()=>this.router.navigateByUrl('/app'),error:e=>this.impersonationError.set(errorMessage(e))});}
}

@Component({
  selector: 'app-platform-plans',
  imports: [CurrencyPipe, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="page-head">
      <div>
        <p class="eyebrow">COMERCIAL</p>
        <h1>Planos</h1>
        <p>Catálogo, preços e limites para novas contratações.</p>
      </div>
      <button class="primary" (click)="open()">+ Novo plano</button>
    </div>
    @if (editing()) {
      <form class="card plan-form" [formGroup]="form" (ngSubmit)="save()">
        <div class="form-title"><div><h2>{{ editingId() ? 'Editar plano' : 'Criar plano' }}</h2><p>Assinaturas existentes preservam o preço e os limites contratados.</p></div><button type="button" class="close" (click)="close()">×</button></div>
        <label>Nome<input formControlName="name" /></label><label>Código<input formControlName="code" /></label>
        <label>Preço<input formControlName="price" type="number" min="0" step="0.01" /></label><label>Moeda<input formControlName="currency" maxlength="3" /></label>
        <label>Periodicidade<select formControlName="interval"><option value="Monthly">Mensal</option><option value="Yearly">Anual</option></select></label><label>Dias de trial<input formControlName="trialDays" type="number" min="0" /></label>
        <label>Máximo de usuários<input formControlName="maxUsers" type="number" min="1" /></label><label>Máximo de clientes<input formControlName="maxCustomers" type="number" min="1" /></label>
        <label>Ordens por período<input formControlName="maxServiceOrders" type="number" min="1" /></label>
        @if (editingPlan()?.activeSubscriptions) { <div class="contract-warning">Este plano possui {{ editingPlan()!.activeSubscriptions }} assinatura(s). As alterações valerão somente para novas assinaturas; migrações são feitas explicitamente no tenant.</div> }
        @if (error()) { <div class="alert form-error">{{ error() }}</div> }
        <div class="form-actions"><button type="button" class="secondary" (click)="close()">Cancelar</button><button class="primary" [disabled]="form.invalid || saving()">{{ saving() ? 'Salvando...' : 'Salvar plano' }}</button></div>
      </form>
    }
    <form class="card migration-form" (submit)="$event.preventDefault(); migrate()">
      <div><p class="eyebrow">MIGRAÇÃO EXPLÍCITA</p><h2>Alterar contrato de um tenant</h2><p>Substitui o snapshot comercial somente para o tenant selecionado.</p></div>
      <label>Buscar tenant<input [formControl]="tenantSearch" placeholder="Nome ou slug" autocomplete="off" /></label>
      <label>Tenant encontrado<select [formControl]="migrationTenant"><option value="">Selecione</option>@for(t of filteredTenants();track t.id){<option [value]="t.id">{{t.name}} · {{t.slug}}</option>}</select><small>{{filteredTenants().length}} resultado(s)</small></label>
      <label>Novo plano<select [formControl]="migrationPlan"><option value="">Selecione</option>@for(p of items();track p.id){@if(p.isActive){<option [value]="p.id">{{p.name}}</option>}}</select></label>
      <button class="primary" [disabled]="migrationTenant.invalid || migrationPlan.invalid || migrating()">{{migrating()?'Migrando...':'Migrar assinatura'}}</button>
      @if (error() && !editing()) { <div class="alert migration-error">{{error()}}</div> }
    </form>
    <section class="plan-grid">
      @for (p of items(); track p.id) {
        <article class="card p-plan">
          <p class="eyebrow">{{ p.code }}</p>
          <h2>{{ p.name }}</h2>
          <strong
            >{{ p.price | currency: p.currency
            }}<small>/{{ p.interval === 'Monthly' ? 'mês' : 'ano' }}</small></strong
          >
          <ul>
            <li>{{ p.maxUsers }} usuários</li>
            <li>{{ p.maxCustomers }} clientes</li>
            <li>{{ p.maxServiceOrders }} ordens por período</li>
            <li>{{ p.trialDays }} dias de trial</li>
          </ul>
          <p>{{ p.activeSubscriptions }} assinatura(s) · {{ p.isActive ? 'Ativo' : 'Inativo' }}</p>
          <div class="row-actions"><button class="table-action" (click)="open(p)">Editar</button><button class="table-action" (click)="toggle(p)">{{ p.isActive ? 'Desativar' : 'Ativar' }}</button></div>
        </article>
      }
    </section>`,
  styles: [`
    .plan-form{padding:22px;margin:-10px 0 22px;display:grid;grid-template-columns:repeat(3,1fr);gap:16px}.plan-form .form-title,.plan-form .contract-warning,.plan-form .form-error,.plan-form .form-actions{grid-column:1/-1}.form-title{display:flex;align-items:flex-start;justify-content:space-between;border-bottom:1px solid #e7ebe6;padding-bottom:14px}.form-title h2{margin:0 0 5px}.form-title p{margin:0;color:#74837f;font-size:11px}.plan-form label,.migration-form label{font-size:11px;font-weight:800}.plan-form input,.plan-form select,.migration-form input,.migration-form select{display:block;width:100%;height:39px;box-sizing:border-box;margin-top:7px;border:1px solid #d9e1da;border-radius:8px;padding:9px 10px;background:#fff}.close{width:32px;height:32px;border:1px solid #d9e1da;border-radius:8px;background:#fff;color:#52645f;font-size:20px;line-height:1;cursor:pointer}.close:hover{background:#f1f4ef;color:#173f39}.form-actions{display:flex;justify-content:flex-end;gap:9px}.secondary{border:1px solid #cfd8d1;border-radius:8px;background:#fff;color:#334b45;padding:10px 17px;font-weight:800;cursor:pointer}.secondary:hover{background:#f1f4ef;border-color:#aebdb2}.contract-warning{padding:12px;border-radius:8px;background:#fff5d8;color:#6c5310;font-size:12px}.migration-form{padding:18px;margin:-10px 0 22px;display:grid;grid-template-columns:1.35fr 1fr 1.15fr 1fr auto;gap:14px;align-items:start}.migration-form h2{font-size:16px;margin:4px 0}.migration-form p{font-size:11px;color:#74837f;margin:0}.migration-form button.primary{margin-top:18px;height:39px}.migration-form label small{display:block;margin-top:4px;color:#74837f;font-weight:500}.migration-error{grid-column:1/-1}.p-plan>p{color:#74837f;font-size:11px}.row-actions{display:flex;gap:8px;margin-top:14px}@media(max-width:1000px){.migration-form{grid-template-columns:1fr 1fr}.migration-form>div:first-child,.migration-form button.primary{grid-column:1/-1}.migration-form button.primary{margin-top:0}}@media(max-width:800px){.plan-form,.migration-form{grid-template-columns:1fr}.migration-form>*{grid-column:1!important}}
  `],
})
export class PlatformPlansComponent {
  private api = inject(Api);
  private fb = inject(FormBuilder);
  items = signal<Plan[]>([]);
  tenants = signal<PlatformTenant[]>([]);
  editing = signal(false); editingId = signal<string|null>(null); editingPlan = signal<Plan|null>(null); saving = signal(false); error = signal('');
  migrating = signal(false); tenantSearch = this.fb.nonNullable.control(''); migrationTenant = this.fb.nonNullable.control('',Validators.required); migrationPlan = this.fb.nonNullable.control('',Validators.required);
  form = this.fb.nonNullable.group({name:['',Validators.required],code:['',Validators.required],price:[0,[Validators.required,Validators.min(0)]],currency:['BRL',[Validators.required,Validators.minLength(3),Validators.maxLength(3)]],interval:['Monthly' as 'Monthly'|'Yearly',Validators.required],trialDays:[14,[Validators.required,Validators.min(0)]],maxUsers:[10,[Validators.required,Validators.min(1)]],maxCustomers:[500,[Validators.required,Validators.min(1)]],maxServiceOrders:[200,[Validators.required,Validators.min(1)]]});
  constructor() {
    this.load(); this.api.get<PlatformTenant[]>('/api/platform/tenants').subscribe(x=>this.tenants.set(x.filter(t=>t.isActive)));
  }
  load(){this.api.get<Plan[]>('/api/platform/plans').subscribe((x)=>this.items.set(x));}
  filteredTenants(){const search=this.tenantSearch.value.trim().toLocaleLowerCase('pt-BR');return this.tenants().filter(t=>!search||t.name.toLocaleLowerCase('pt-BR').includes(search)||t.slug.toLocaleLowerCase('pt-BR').includes(search)).slice(0,20);}
  open(p?:Plan){this.editing.set(true);this.editingId.set(p?.id??null);this.editingPlan.set(p??null);this.error.set('');this.form.reset(p?{name:p.name,code:p.code,price:p.price,currency:p.currency,interval:p.interval,trialDays:p.trialDays,maxUsers:p.maxUsers,maxCustomers:p.maxCustomers,maxServiceOrders:p.maxServiceOrders}:{name:'',code:'',price:0,currency:'BRL',interval:'Monthly',trialDays:14,maxUsers:10,maxCustomers:500,maxServiceOrders:200});}
  close(){this.editing.set(false);this.editingId.set(null);this.editingPlan.set(null);}
  save(){if(this.form.invalid)return;const id=this.editingId();const request=id?this.api.put<Plan>(`/api/platform/plans/${id}`,this.form.getRawValue()):this.api.post<Plan>('/api/platform/plans',this.form.getRawValue());this.saving.set(true);request.pipe(finalize(()=>this.saving.set(false))).subscribe({next:()=>{this.close();this.load();},error:e=>this.error.set(errorMessage(e))});}
  toggle(p:Plan){const warning=p.activeSubscriptions?`O plano possui ${p.activeSubscriptions} assinatura(s). Os contratos atuais serão preservados. Deseja ${p.isActive?'desativar':'ativar'} o plano?`:`Deseja ${p.isActive?'desativar':'ativar'} este plano?`;if(!confirm(warning))return;this.api.patch<Plan>(`/api/platform/plans/${p.id}/status`,{isActive:!p.isActive}).subscribe(()=>this.load());}
  migrate(){if(this.migrationTenant.invalid||this.migrationPlan.invalid)return;const tenant=this.tenants().find(x=>x.id===this.migrationTenant.value);const plan=this.items().find(x=>x.id===this.migrationPlan.value);if(!tenant||!plan||!confirm(`Migrar ${tenant.name} para ${plan.name}? O preço e os limites contratados serão substituídos.`))return;this.migrating.set(true);this.api.put(`/api/platform/tenants/${tenant.id}/subscription`,{planId:plan.id,gatewayCustomerId:null,gatewaySubscriptionId:null}).pipe(finalize(()=>this.migrating.set(false))).subscribe({next:()=>{this.tenantSearch.reset();this.migrationTenant.reset();this.migrationPlan.reset();this.load();},error:e=>this.error.set(errorMessage(e))});}
}
