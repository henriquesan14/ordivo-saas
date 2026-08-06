export type Role = 'Owner' | 'Admin' | 'Member';
export interface SessionUser {
  userId: string;
  tenantId?: string;
  name: string;
  email: string;
  role: Role | 'PlatformAdmin';
  expiresAt: string;
  mode?: 'tenant' | 'platform' | 'impersonation';
  impersonationSessionId?: string;
  impersonationReason?: string;
}
export interface Customer {
  id: string;
  name: string;
  document: string;
  phone: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
}
export interface ServiceOrder {
  id: string;
  customerId: string;
  number: string;
  title: string;
  description: string;
  price: number;
  status: 'Draft' | 'Open' | 'InProgress' | 'Completed' | 'Canceled';
  assignedUserId?: string;
  scheduledAt?: string;
  completedAt?: string;
  createdAt: string;
}
export interface User {
  id: string;
  name: string;
  email: string;
  role: Role;
  isActive: boolean;
  isEmailVerified: boolean;
  createdAt: string;
}
export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface Plan {
  id: string;
  name: string;
  code: string;
  price: number;
  currency: string;
  interval: 'Monthly' | 'Yearly';
  trialDays: number;
  maxUsers: number;
  maxCustomers: number;
  maxServiceOrders: number;
  isActive: boolean;
  activeSubscriptions: number;
}
export interface Subscription {
  id: string;
  status: string;
  trialEndsAt?: string;
  periodStartsAt: string;
  periodEndsAt: string;
  accessBlocked: boolean;
  usersUsed: number;
  customersUsed: number;
  serviceOrdersUsed: number;
  plan: Plan;
}
export interface Tenant {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
}
export interface PlatformTenant extends Tenant {
  createdAt: string;
}
