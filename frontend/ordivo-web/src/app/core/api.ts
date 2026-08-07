import { HttpClient, HttpContextToken, HttpErrorResponse, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, filter, finalize, map, Observable, shareReplay, switchMap, tap, throwError } from 'rxjs';
import { Customer, Paged, ServiceOrder, Subscription, Tenant, User } from './models';

const SKIP_CSRF = new HttpContextToken(() => false);
@Injectable({ providedIn: 'root' })
export class Api {
  private http = inject(HttpClient);
  get<T>(url: string, params?: Record<string, string | number | boolean>) { return this.http.get<T>(url, { params }); }
  post<T>(url: string, body: unknown) { return this.http.post<T>(url, body); }
  put<T>(url: string, body: unknown) { return this.http.put<T>(url, body); }
  patch<T>(url: string, body: unknown) { return this.http.patch<T>(url, body); }
  delete<T>(url: string) { return this.http.delete<T>(url); }
  customers(search = '') { return this.get<Paged<Customer>>('/api/customers', { name: search, page: 1, pageSize: 50 }); }
  orders(search = '') { return this.get<Paged<ServiceOrder>>('/api/service-orders', { search, page: 1, pageSize: 50 }); }
  users() { return this.get<User[]>('/api/users'); }
  subscription() { return this.get<Subscription>('/api/billing/subscription'); }
  tenant() { return this.get<Tenant>('/api/tenant'); }
}

let csrf$: Observable<{token:string}> | undefined;
let refresh$: Observable<unknown> | undefined;
export function resetCsrfToken(){ csrf$ = undefined; }
export const apiInterceptor: HttpInterceptorFn = (request, next) => {
  const http = inject(HttpClient); const unsafe = !['GET','HEAD','OPTIONS'].includes(request.method);
  const send = (token?: string) => {
    const authenticatedRequest = request.clone({ withCredentials: true, setHeaders: token ? { 'X-CSRF-TOKEN': token } : {} });
    return next(authenticatedRequest).pipe(catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthenticationRequest(request.url)) return throwError(() => error);
      const current = readStoredUser();
      if (!current) return throwError(() => error);
      const platform = current.mode === 'platform' || current.mode === 'impersonation' || current.role === 'PlatformAdmin';
      refresh$ ??= http.get<{token:string}>('/api/auth/csrf', { withCredentials: true }).pipe(
        switchMap(freshCsrf => next(new HttpRequest('POST', platform ? '/api/platform/auth/refresh' : '/api/auth/refresh', {}, { withCredentials: true, headers: authenticatedRequest.headers.set('X-CSRF-TOKEN', freshCsrf.token) }))),
        filter(event => event instanceof HttpResponse),
        map(event => (event as HttpResponse<any>).body),
        tap((user: any) => {
          const renewed = { ...user, mode: platform ? 'platform' : 'tenant' };
          sessionStorage.setItem('ordivo.user', JSON.stringify(renewed));
          if (current.mode === 'impersonation') sessionStorage.removeItem('ordivo.platform-user');
          resetCsrfToken();
          window.dispatchEvent(new CustomEvent('ordivo:session-refreshed', { detail: renewed }));
        }),
        finalize(() => { refresh$ = undefined; }),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
      return refresh$.pipe(
        switchMap(() => next(authenticatedRequest)),
        catchError(refreshError => { sessionStorage.removeItem('ordivo.user'); sessionStorage.removeItem('ordivo.platform-user'); window.dispatchEvent(new Event('ordivo:session-expired')); return throwError(() => refreshError); }),
      );
    }));
  };
  if (!unsafe || request.context.get(SKIP_CSRF) || request.url.endsWith('/api/auth/csrf')) return send();
  csrf$ ??= http.get<{token:string}>('/api/auth/csrf', { withCredentials: true }).pipe(shareReplay(1));
  return csrf$.pipe(switchMap(result => send(result.token)), catchError(error => { csrf$ = undefined; return throwError(() => error); }));
};

function isAuthenticationRequest(url: string) { return url.includes('/auth/login') || url.includes('/auth/refresh') || url.includes('/auth/logout'); }
function readStoredUser(): any | null { try { return JSON.parse(sessionStorage.getItem('ordivo.user') ?? 'null'); } catch { return null; } }

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) return error.error?.detail ?? error.error?.error?.description ?? 'Não foi possível concluir a operação.';
  return 'Ocorreu um erro inesperado.';
}
