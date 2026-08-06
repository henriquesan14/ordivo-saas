import { computed, inject, Injectable, signal } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { tap } from 'rxjs';
import { Api } from './api';
import { SessionUser } from './models';

@Injectable({ providedIn: 'root' })
export class Auth {
  private api = inject(Api); private router = inject(Router);
  readonly user = signal<SessionUser | null>(this.read()); readonly authenticated = computed(() => !!this.user()); readonly isPlatform = computed(() => this.user()?.mode === 'platform' || this.user()?.role === 'PlatformAdmin');
  login(email: string, password: string) { return this.api.post<SessionUser>('/api/auth/login', { email, password }).pipe(tap(user => this.store({...user,mode:'tenant'}))); }
  platformLogin(email: string, password: string) { return this.api.post<SessionUser>('/api/platform/auth/login', { email, password }).pipe(tap(user => this.store({...user,mode:'platform'}))); }
  logout() { this.api.post<void>('/api/auth/logout', {}).subscribe({ complete: () => this.clear(), error: () => this.clear() }); }
  private clear() { const platform=this.isPlatform(); this.user.set(null); sessionStorage.removeItem('ordivo.user'); this.router.navigateByUrl(platform?'/platform/login':'/login'); }
  private store(user:SessionUser){this.user.set(user);sessionStorage.setItem('ordivo.user',JSON.stringify(user));}
  private read(): SessionUser | null { try { return JSON.parse(sessionStorage.getItem('ordivo.user') ?? 'null'); } catch { return null; } }
}
export const authGuard: CanActivateFn = () => { const auth = inject(Auth); return auth.authenticated() && !auth.isPlatform() || inject(Router).createUrlTree([auth.isPlatform()?'/platform':'/login']); };
export const guestGuard: CanActivateFn = () => { const auth = inject(Auth); return !auth.authenticated() || inject(Router).createUrlTree([auth.isPlatform()?'/platform':'/app']); };
export const platformGuard: CanActivateFn = () => { const auth=inject(Auth); return auth.authenticated() && auth.isPlatform() || inject(Router).createUrlTree(['/platform/login']); };
export const platformGuestGuard: CanActivateFn = () => { const auth=inject(Auth); return !auth.isPlatform() || inject(Router).createUrlTree(['/platform']); };
