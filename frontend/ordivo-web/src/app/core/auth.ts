import { computed, inject, Injectable, signal } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { tap } from 'rxjs';
import { Api, resetCsrfToken } from './api';
import { SessionUser } from './models';

@Injectable({ providedIn: 'root' })
export class Auth {
  private api = inject(Api);
  private router = inject(Router);
  readonly user = signal<SessionUser | null>(this.read());
  readonly authenticated = computed(() => !!this.user());
  readonly isPlatform = computed(
    () => this.user()?.mode === 'platform' || this.user()?.role === 'PlatformAdmin',
  );
  readonly isImpersonating = computed(() => this.user()?.mode === 'impersonation');
  constructor() {
    window.addEventListener('ordivo:session-refreshed', (event) => this.user.set((event as CustomEvent<SessionUser>).detail));
    window.addEventListener('ordivo:session-expired', () => this.clear());
  }
  login(email: string, password: string) {
    return this.api
      .post<SessionUser>('/api/auth/login', { email, password })
      .pipe(tap((user) => { this.store({ ...user, mode: 'tenant' }); resetCsrfToken(); }));
  }
  platformLogin(email: string, password: string) {
    return this.api
      .post<SessionUser>('/api/platform/auth/login', { email, password })
      .pipe(tap((user) => { this.store({ ...user, mode: 'platform' }); resetCsrfToken(); }));
  }
  startImpersonation(tenantId: string, reason: string) {
    const platform = this.user();
    return this.api.post<any>('/api/platform/impersonations', { tenantId, reason }).pipe(
      tap((x) => {
        if (platform) sessionStorage.setItem('ordivo.platform-user', JSON.stringify(platform));
        this.store({
          userId: x.userId,
          tenantId: x.tenantId,
          name: x.userName,
          email: x.userEmail,
          role: x.role,
          expiresAt: x.expiresAt,
          mode: 'impersonation',
          impersonationSessionId: x.sessionId,
          impersonationReason: x.reason,
        });
        resetCsrfToken();
      }),
    );
  }
  endImpersonation() {
    return this.api.post<void>('/api/impersonation/end', {}).pipe(
      tap(() => {
        const stored = sessionStorage.getItem('ordivo.platform-user');
        if (stored) {
          this.store(JSON.parse(stored));
          sessionStorage.removeItem('ordivo.platform-user');
        }
        resetCsrfToken();
        this.router.navigateByUrl('/platform');
      }),
    );
  }
  logout() {
    this.api
      .post<void>('/api/auth/logout', {})
      .subscribe({ complete: () => this.clear(), error: () => this.clear() });
  }
  private clear() {
    const platform = this.isPlatform() || this.isImpersonating();
    this.user.set(null);
    sessionStorage.removeItem('ordivo.user');
    sessionStorage.removeItem('ordivo.platform-user');
    this.router.navigateByUrl(platform ? '/platform/login' : '/login');
  }
  private store(user: SessionUser) {
    this.user.set(user);
    sessionStorage.setItem('ordivo.user', JSON.stringify(user));
  }
  private read(): SessionUser | null {
    try {
      const stored = JSON.parse(sessionStorage.getItem('ordivo.user') ?? 'null');
      const user = stored?.body ?? stored;
      if (!user?.name || !user?.email || !user?.role) return null;
      user.mode ??= user.role === 'PlatformAdmin' ? 'platform' : 'tenant';
      if (stored?.body) sessionStorage.setItem('ordivo.user', JSON.stringify(user));
      return user;
    } catch {
      return null;
    }
  }
}
export const authGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  return (
    (auth.authenticated() && !auth.isPlatform()) ||
    inject(Router).createUrlTree([auth.isPlatform() ? '/platform' : '/login'])
  );
};
export const guestGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  return (
    !auth.authenticated() ||
    inject(Router).createUrlTree([auth.isPlatform() ? '/platform' : '/app'])
  );
};
export const platformGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  return (
    (auth.authenticated() && auth.isPlatform()) || inject(Router).createUrlTree(['/platform/login'])
  );
};
export const platformGuestGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  return !auth.isPlatform() || inject(Router).createUrlTree(['/platform']);
};
