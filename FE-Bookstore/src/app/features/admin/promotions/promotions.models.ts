import { Nullable, PagedResult } from '../../../shared/models/api.models';

export enum DiscountType {
  Percent = 'Percent',
  FixedAmount = 'FixedAmount',
  FreeShipping = 'FreeShipping',
  BuyXGetY = 'BuyXGetY'
}

export enum CouponStatus {
  Draft = 'Draft',
  Active = 'Active',
  Scheduled = 'Scheduled',
  Expired = 'Expired',
  Paused = 'Paused'
}

export enum CouponAudience {
  Public = 'Public',
  VipOnly = 'VipOnly',
  NewCustomer = 'NewCustomer'
}

export interface CouponListItemResponse {
  id: string;
  code: string;
  name: string;
  description?: Nullable<string>;
  discountType: DiscountType;
  discountValue: number;
  minOrderValue: number;
  maxDiscountAmount?: Nullable<number>;
  usageLimit: number;
  usedCount: number;
  perUserUsageLimit: number;
  startDate: string;
  endDate: string;
  audience: CouponAudience;
  status: CouponStatus;
  createdAt: string;
}

export interface CouponDetailResponse extends CouponListItemResponse {
  updatedAt?: Nullable<string>;
  isDeleted: boolean;
  rowVersion?: Nullable<string>;
}

/** BE: ValidateCouponResponse - POST /coupons/validate. */
export interface ValidateCouponResponse {
  isValid: boolean;
  message: string;
  couponId: string;
  couponCode: string;
  discountAmount: number;
  finalAmount: number;
  minOrderValue: number;
  coupon?: Nullable<CouponDetailResponse>;
}

/** BE: ValidateCouponRequest - POST /coupons/validate. */
export interface ValidateCouponRequest {
  code: string;
  customerId: string;
  orderAmount: number;
}

export interface CouponSearchRequest {
  keyword?: string;
  discountType?: DiscountType;
  audience?: CouponAudience;
  status?: CouponStatus;
  page?: number;
  pageSize?: number;
}

export interface CreateCouponRequest {
  code: string;
  name: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  minOrderValue: number;
  maxDiscountAmount?: number;
  usageLimit: number;
  perUserUsageLimit: number;
  startDate: string;
  endDate: string;
  audience: CouponAudience;
  status: CouponStatus;
  campaignId?: string;
}

/** BE: UpdateCouponRequest - chi nhan field kha null. */
export interface UpdateCouponRequest {
  name?: string;
  description?: string;
  discountType?: DiscountType;
  discountValue?: number;
  minOrderValue?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  perUserUsageLimit?: number;
  startDate?: string;
  endDate?: string;
  audience?: CouponAudience;
  status?: CouponStatus;
  campaignId?: string;
  rowVersion?: string;
}

export type CouponPagedResult = PagedResult<CouponListItemResponse>;
