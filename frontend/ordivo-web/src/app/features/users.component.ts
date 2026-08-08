import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Api, errorMessage } from '../core/api';
import { User } from '../core/models';
import { Auth } from '../core/auth';

@Component({
  selector: 'app-tenant-users', imports: [ReactiveFormsModule, DatePipe], changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
  <div class="page-head"><div><p class="eyebrow">EQUIPE</p><h1>Usuários</h1><p>Pessoas com acesso ao seu espaço de trabalho.</p></div>@if(auth.canManageUsers()){<button class="primary" (click)="openInvite()">+ Convidar usuário</button>}</div>
  @if(showInvite()){<form class="invite-form card" [formGroup]="form" (ngSubmit)="invite()"><div class="invite-head"><div><h2>Convidar usuário</h2><p>O usuário receberá um link para definir a própria senha.</p></div><button type="button" class="close" aria-label="Fechar" (click)="cancel()">×</button></div><div class="invite-fields"><label>Nome<input formControlName="name" placeholder="Nome completo" autocomplete="name"></label><label>Email<input type="email" formControlName="email" placeholder="usuario@empresa.com" autocomplete="email"></label><label>Perfil<select formControlName="role"><option value="Member">Membro</option><option value="Admin">Administrador</option>@if(auth.isOwner()){<option value="Owner">Proprietário</option>}</select></label></div>@if(error()){<div class="alert">{{error()}}</div>}<div class="invite-actions"><button type="button" class="secondary" (click)="cancel()">Cancelar</button><button class="primary" [disabled]="form.invalid||sending()">{{sending()?'Enviando...':'Enviar convite'}}</button></div></form>}
  @if(success()){<div class="success-banner">Convite enviado para {{success()}}. O link é válido por 7 dias.</div>}
  <article class="card table-wrap"><table><thead><tr><th>USUÁRIO</th><th>PAPEL</th><th>EMAIL VERIFICADO</th><th>STATUS</th><th>CRIADO EM</th></tr></thead><tbody>@for(user of items();track user.id){<tr><td><div class="name-cell"><span>{{user.name?.[0]||'?'}}</span><div><b>{{user.name}}</b><small>{{user.email}}</small></div></div></td><td>{{roleLabel(user.role)}}</td><td>{{user.isEmailVerified?'Sim':'Pendente'}}</td><td><span class="status" [class.completed]="user.isActive">{{user.isActive?'Ativo':'Convite pendente'}}</span></td><td>{{user.createdAt|date:'dd/MM/yyyy'}}</td></tr>}@empty{<tr><td colspan="5" class="empty">Nenhum usuário encontrado.</td></tr>}</tbody></table></article>`,
  styles: [`.invite-form{padding:20px;margin:-12px 0 20px}.invite-head{display:flex;align-items:start;justify-content:space-between;margin-bottom:18px}.invite-head h2{font-size:18px;margin:0 0 5px}.invite-head p{font-size:12px;color:#74817f;margin:0}.close{width:34px;height:34px;border:1px solid #d7dfd8;border-radius:8px;background:#fff;color:#52625f;font-size:20px;cursor:pointer}.invite-fields{display:grid;grid-template-columns:1fr 1.2fr .8fr;gap:12px}.invite-fields label{font-size:11px;font-weight:800}.invite-fields input,.invite-fields select{display:block;width:100%;height:42px;margin-top:6px;padding:0 12px;border:1px solid #dce2dc;border-radius:8px;background:#fff;outline:none}.invite-fields input:focus,.invite-fields select:focus{border-color:#2d625a;box-shadow:0 0 0 3px #2d625a16}.invite-actions{display:flex;justify-content:flex-end;gap:9px;margin-top:18px}.secondary{min-height:42px;padding:0 17px;border:1px solid #ccd7cf;border-radius:9px;background:#fff;color:#334b45;font-weight:800;cursor:pointer}.success-banner{padding:12px 14px;margin-bottom:14px;border-radius:9px;background:#e5f2ed;color:#286658;font-size:12px}.alert{margin-top:14px}@media(max-width:760px){.invite-fields{grid-template-columns:1fr}.invite-actions>*{flex:1}}`]
})
export class TenantUsersComponent {
  private api=inject(Api); private fb=inject(FormBuilder); readonly auth=inject(Auth);
  items=signal<User[]>([]); showInvite=signal(false); sending=signal(false); error=signal(''); success=signal('');
  form=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(120)]],email:['',[Validators.required,Validators.email,Validators.maxLength(254)]],role:['Member' as User['role'],Validators.required]});
  constructor(){this.load()} load(){this.api.users().subscribe({next:users=>this.items.set(users),error:error=>this.error.set(errorMessage(error))})}
  openInvite(){if(!this.auth.canManageUsers())return;this.error.set('');this.success.set('');this.showInvite.set(true)}
  cancel(){this.showInvite.set(false);this.error.set('');this.form.reset({name:'',email:'',role:'Member'})}
  invite(){if(!this.auth.canManageUsers()||this.form.invalid||this.sending())return;if(this.form.controls.role.value==='Owner'&&!this.auth.isOwner()){this.error.set('Somente o proprietário pode convidar outro proprietário.');return}this.sending.set(true);this.error.set('');this.api.post<User>('/api/users/invitations',this.form.getRawValue()).pipe(finalize(()=>this.sending.set(false))).subscribe({next:user=>{this.cancel();this.success.set(user.email);this.load()},error:error=>this.error.set(errorMessage(error))})}
  roleLabel(role:User['role']){return ({Owner:'Proprietário',Admin:'Administrador',Member:'Membro'} as const)[role]}
}
