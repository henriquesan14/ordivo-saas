import { Routes } from '@angular/router';
import { authGuard, guestGuard, platformGuard, platformGuestGuard } from './core/auth';
import { LoginComponent } from './features/login.component';
import { ShellComponent } from './layout/shell.component';
import { BillingComponent, CustomersComponent, DashboardComponent, OrdersComponent, SettingsComponent, UsersComponent } from './features/pages';
import { PlatformLoginComponent, PlatformOverviewComponent, PlatformPlansComponent, PlatformShellComponent, PlatformTenantsComponent } from './features/platform.component';

export const routes: Routes = [
  { path:'login', component:LoginComponent, canActivate:[guestGuard], title:'Entrar · Ordivo' },
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
    {path:'users',component:UsersComponent,title:'Equipe · Ordivo'},
    {path:'billing',component:BillingComponent,title:'Plano e cobrança · Ordivo'},
    {path:'settings',component:SettingsComponent,title:'Configurações · Ordivo'}
  ]},
  {path:'',pathMatch:'full',redirectTo:'app'}, {path:'**',redirectTo:'app'}
];
