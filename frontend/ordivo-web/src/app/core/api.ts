import { HttpClient, HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, shareReplay, switchMap, throwError } from 'rxjs';
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
export const apiInterceptor: HttpInterceptorFn = (request, next) => {
  const http = inject(HttpClient); const unsafe = !['GET','HEAD','OPTIONS'].includes(request.method);
  const send = (token?: string) => next(request.clone({ withCredentials: true, setHeaders: token ? { 'X-CSRF-TOKEN': token } : {} })).pipe(
    catchError((error: HttpErrorResponse) => { if (error.status === 401 && !request.url.includes('/auth/refresh')) sessionStorage.removeItem('ordivo.user'); return throwError(() => error); }));
  if (!unsafe || request.context.get(SKIP_CSRF) || request.url.endsWith('/api/auth/csrf')) return send();
  csrf$ ??= http.get<{token:string}>('/api/auth/csrf', { withCredentials: true }).pipe(shareReplay(1));
  return csrf$.pipe(switchMap(result => send(result.token)), catchError(error => { csrf$ = undefined; return throwError(() => error); }));
};

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) return error.error?.detail ?? error.error?.error?.description ?? 'Não foi possível concluir a operação.';
  return 'Ocorreu um erro inesperado.';
}
