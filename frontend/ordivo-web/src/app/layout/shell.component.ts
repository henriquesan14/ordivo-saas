import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from '../core/auth';
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: ` <div class="app-shell" [class.menu-open]="menu()">
    <aside>
      <a class="brand" routerLink="/app"><span>O</span> Ordivo</a>
      <nav>
        @for (item of nav; track item.path) {
          <a
            [routerLink]="item.path"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
            ><i>{{ item.icon }}</i
            >{{ item.label }}</a
          >
        }
      </nav>
      <div class="sidebar-bottom">
        <a routerLink="/app/settings" routerLinkActive="active"><i>⚙</i>Configurações</a
        ><button (click)="exit()"><i>↗</i>{{ auth.isImpersonating() ? 'Encerrar suporte' : 'Sair' }}</button>
      </div>
    </aside>
    <div class="backdrop" (click)="menu.set(false)"></div>
    <section class="workspace">
      <header>
        <button class="menu-btn" (click)="menu.set(!menu())">☰</button>
        <div class="search">⌕ <span>Buscar no Ordivo...</span><kbd>⌘ K</kbd></div>
        <div class="header-actions">
          <button class="icon-btn">?</button><button class="icon-btn">♢<b></b></button>
          <div class="avatar">{{ initials() }}</div>
          <div class="identity">
            <strong>{{ auth.user()?.name }}</strong
            ><small>{{ auth.user()?.role }}</small>
          </div>
        </div>
      </header>
      @if (auth.isImpersonating()) {
        <div class="impersonation-banner">
          <div>
            <b>Modo de suporte ativo</b
            ><span
              >Você está acessando como {{ auth.user()?.name }} ·
              {{ auth.user()?.impersonationReason }}</span
            >
          </div>
          <span class="expires">Expira às {{ expiresAt() }}</span
          ><button (click)="end()" [disabled]="ending()">
            {{ ending() ? 'Encerrando...' : 'Encerrar acesso' }}
          </button>
        </div>
      }
      <main><router-outlet /></main>
    </section>
  </div>`,
  styles: [
    `
      :host {
        display: block;
      }
      .app-shell {
        min-height: 100vh;
        background: #f4f6f1;
      }
      aside {
        position: fixed;
        inset: 0 auto 0 0;
        width: 244px;
        background: #112c29;
        color: #d5e1de;
        padding: 27px 18px;
        display: flex;
        flex-direction: column;
        z-index: 20;
      }
      .brand {
        display: flex;
        align-items: center;
        gap: 11px;
        color: #fff;
        font-weight: 800;
        font-size: 21px;
        padding: 0 10px 30px;
      }
      .brand span {
        display: grid;
        place-items: center;
        width: 32px;
        height: 32px;
        border-radius: 9px;
        background: #e4ff72;
        color: #17312e;
        font-family: Georgia;
        font-style: italic;
      }
      nav {
        display: grid;
        gap: 5px;
      }
      nav a,
      .sidebar-bottom a,
      .sidebar-bottom button {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 11px 13px;
        border-radius: 9px;
        color: #9fb6b2;
        font-size: 13px;
        font-weight: 650;
        border: 0;
        background: transparent;
        width: 100%;
        cursor: pointer;
      }
      nav a i,
      .sidebar-bottom i {
        font-style: normal;
        width: 19px;
        text-align: center;
        font-size: 16px;
      }
      nav a.active,
      .sidebar-bottom a.active {
        background: #25443f;
        color: #f3ffed;
        box-shadow: inset 3px 0 #dfff68;
      }
      .sidebar-bottom {
        margin-top: auto;
        border-top: 1px solid #ffffff12;
        padding-top: 14px;
      }
      .workspace {
        margin-left: 244px;
        min-height: 100vh;
      }
      header {
        height: 72px;
        background: #fff;
        border-bottom: 1px solid #e2e7e0;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 30px;
        position: sticky;
        top: 0;
        z-index: 10;
      }
      .search {
        width: 340px;
        background: #f4f6f2;
        border-radius: 9px;
        padding: 10px 12px;
        color: #7c8986;
        font-size: 13px;
      }
      .search kbd {
        float: right;
        background: #fff;
        border: 1px solid #dce1dc;
        border-radius: 5px;
        padding: 1px 6px;
      }
      .header-actions {
        display: flex;
        align-items: center;
        gap: 10px;
      }
      .icon-btn,
      .menu-btn {
        border: 0;
        background: transparent;
        font-size: 17px;
        position: relative;
      }
      .icon-btn b {
        position: absolute;
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background: #d9f85f;
        right: 1px;
      }
      .avatar {
        width: 35px;
        height: 35px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: #d9f0e9;
        color: #24554d;
        font-weight: 800;
        font-size: 12px;
      }
      .identity {
        display: grid;
      }
      .identity strong {
        font-size: 12px;
      }
      .identity small {
        font-size: 10px;
        color: #86918f;
        margin-top: 2px;
      }
      .menu-btn {
        display: none;
      }
      .impersonation-banner {
        min-height: 58px;
        background: #fff2c2;
        border-bottom: 1px solid #ead58b;
        padding: 10px 28px;
        display: flex;
        align-items: center;
        gap: 20px;
        color: #654f0a;
      }
      .impersonation-banner > div {
        display: grid;
        margin-right: auto;
      }
      .impersonation-banner b {
        font-size: 12px;
      }
      .impersonation-banner span {
        font-size: 10px;
      }
      .impersonation-banner button {
        border: 1px solid #b89522;
        background: #fff9e5;
        border-radius: 7px;
        padding: 7px 11px;
        color: #684f00;
        font-size: 10px;
        font-weight: 800;
      }
      main {
        padding: 30px;
      }
      .backdrop {
        display: none;
      }
      @media (max-width: 850px) {
        aside {
          transform: translateX(-100%);
          transition: 0.2s;
        }
        .menu-open aside {
          transform: none;
        }
        .menu-open .backdrop {
          display: block;
          position: fixed;
          inset: 0;
          background: #0006;
          z-index: 15;
        }
        .workspace {
          margin-left: 0;
        }
        .menu-btn {
          display: block;
        }
        .search {
          display: none;
        }
        main {
          padding: 20px;
        }
        header {
          padding: 0 20px;
        }
        .identity,
        .expires {
          display: none;
        }
        .impersonation-banner {
          padding: 8px 15px;
        }
      }
    `,
  ],
})
export class ShellComponent {
  auth = inject(Auth);
  menu = signal(false);
  ending = signal(false);
  nav = [
    { path: '/app', label: 'Visão geral', icon: '⌂', exact: true },
    { path: '/app/orders', label: 'Ordens de serviço', icon: '▣' },
    { path: '/app/customers', label: 'Clientes', icon: '♙' },
    { path: '/app/users', label: 'Equipe', icon: '♧' },
    { path: '/app/billing', label: 'Plano e cobrança', icon: '◇' },
  ];
  initials = () =>
    this.auth
      .user()
      ?.name.split(' ')
      .slice(0, 2)
      .map((x) => x[0])
      .join('')
      .toUpperCase() ?? 'OR';
  expiresAt = () =>
    new Date(this.auth.user()?.expiresAt ?? '').toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  end() {
    this.ending.set(true);
    this.auth.endImpersonation().subscribe({ error: () => this.ending.set(false) });
  }
  exit() {
    if (this.auth.isImpersonating()) this.end();
    else this.auth.logout();
  }
}
