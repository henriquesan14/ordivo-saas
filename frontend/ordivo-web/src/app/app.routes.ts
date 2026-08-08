import { Routes } from '@angular/router';
import { authGuard, guestGuard, platformGuard, platformGuestGuard, tenantAdminGuard, tenantOwnerGuard } from './core/auth';
import { LoginComponent } from './features/login.component';
import { ShellComponent } from './layout/shell.component';
import { CustomersComponent, DashboardComponent, OrdersComponent, SettingsComponent } from './features/pages';
import { TenantBillingComponent } from './features/billing.component';
import { PlatformLoginComponent, PlatformOverviewComponent, PlatformPlansComponent, PlatformShellComponent, PlatformTenantsComponent } from './features/platform.component';
import { TenantUsersComponent } from './features/users.component';
import { AcceptInvitationComponent } from './features/accept-invitation.component';
import { SignupComponent } from './features/signup.component';
import { VerifyEmailComponent } from './features/verify-email.component';

export const routes: Routes = [
  { path:'login', component:LoginComponent, canActivate:[guestGuard], title:'Entrar · Ordivo' },
  { path:'signup', component:SignupComponent, canActivate:[guestGuard], title:'Comece agora · Ordivo' },
  { path:'verify-email', component:VerifyEmailComponent, title:'Confirmar e-mail · Ordivo' },
  { path:'accept-invitation', component:AcceptInvitationComponent, title:'Aceitar convite · Ordivo' },
  { path:'platform/login', component:PlatformLoginComponent, canActivate:[platformGuestGuard], title:'PlatformAdmin · Ordivo' },
  { path:'platform', component:PlatformShellComponent, canActivate:[platformGuard], children:[
    {path:'',component:PlatformOverviewComponent,title:'Plataforma · Ordivo'},
    {path:'tenants',component:PlatformTenantsComponent,title:'Tenants · Ordivo'},
    {path:'plans',component:PlatformPlansComponent,title:'Planos · Ordivo'}
  ]},
  { path:'app', component:ShellComponent, canActivate:[authGuard], children:[
    {path:'',component:DashboardComponent,title:'Visão geral · Ordivo'},
    {path:'orders',component:OrdersComponent,title:'Ordens · Ordivo'},
    {path:'customers',component:CustomersComponent,title:'Clientes · Ordivo'},
    {path:'users',component:TenantUsersComponent,canActivate:[tenantAdminGuard],title:'Equipe · Ordivo'},
    {path:'billing',component:TenantBillingComponent,canActivate:[tenantOwnerGuard],title:'Plano e cobrança · Ordivo'},
    {path:'settings',component:SettingsComponent,canActivate:[tenantAdminGuard],title:'Configurações · Ordivo'}
  ]},
  {path:'',pathMatch:'full',redirectTo:'app'}, {path:'**',redirectTo:'app'}
];
