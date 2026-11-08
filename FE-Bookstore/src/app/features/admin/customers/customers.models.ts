import { Nullable, PagedResult } from '../../../shared/models/api.models';
import { OrderListItemResponse } from '../orders/orders.models';

export enum CustomerStatus {
  Active = 'Active',
  Suspended = 'Suspended'
}

export enum VipTier {
  Standard = 'Standard',
  Silver = 'Silver',
  Gold = 'Gold',
  Platinum = 'Platinum'
}

export interface CustomerListItemResponse {
  id: string;
  code: string;
  fullName: string;
  userName?: Nullable<string>;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  avatarUrl?: Nullable<string>;
  provinceId?: Nullable<string>;
  provinceName?: Nullable<string>;
  communeId?: Nullable<string>;
  communeName?: Nullable<string>;
  address?: Nullable<string>;
  vipTier: VipTier;
  orderCount: number;
  lifetimeSpend: number;
  preferredCategory?: Nullable<string>;
  status: CustomerStatus;
  createdAt: string;
  lastOrderDate?: Nullable<string>;
}

export interface CustomerDetailResponse extends CustomerListItemResponse {
  birthday?: Nullable<string>;
  note?: Nullable<string>;
  isActive: boolean;
  isDeleted: boolean;
  lastLoginAt?: Nullable<string>;
  wishlistBookCount: number;
  rewardPoints: number;
  addressCount: number;
  recentOrders: OrderListItemResponse[];
  addresses: CustomerAddressResponse[];
}

export interface CustomerSearchRequest {
  keyword?: string;
  vipTier?: VipTier;
  status?: CustomerStatus;
  provinceId?: string;
  minLifetimeSpend?: number;
  maxLifetimeSpend?: number;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

/** BE: CustomerAddressResponse. */
export interface CustomerAddressResponse {
  id: string;
  recipientName: string;
  phoneNumber: string;
  address: string;
  provinceId: string;
  provinceName?: Nullable<string>;
  communeId: string;
  communeName?: Nullable<string>;
  isDefault: boolean;
  note?: Nullable<string>;
}

export interface CreateCustomerRequest {
  userName?: string;
  fullName: string;
  email: string;
  password?: string;
  phoneNumber?: string;
  provinceId?: string;
  communeId?: string;
  address?: string;
  birthday?: string;
  vipTier?: VipTier;
  note?: string;
}

/** BE: UpdateCustomerRequest. */
export interface UpdateCustomerRequest {
  fullName?: string;
  phoneNumber?: string;
  address?: string;
  provinceId?: string;
  communeId?: string;
  birthday?: string;
  vipTier?: VipTier;
  status?: CustomerStatus;
  preferredCategoryId?: string;
  note?: string;
  rewardPoints?: number;
}

export type CustomerPagedResult = PagedResult<CustomerListItemResponse>;

/** BE: ExportPiiResponse - ket qua xuat du lieu PII khach hang. */
export interface ExportPiiResponse {
  totalCustomers: number;
  rows: CustomerPiiRow[];
  exportedAt: string;
}

/** BE: CustomerPiiRow. */
export interface CustomerPiiRow {
  id: string;
  fullName: string;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  address?: Nullable<string>;
  provinceName?: Nullable<string>;
  communeName?: Nullable<string>;
  birthday?: Nullable<string>;
}
