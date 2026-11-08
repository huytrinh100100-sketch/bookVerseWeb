import { Nullable, PagedResult } from '../../../shared/models/api.models';
import { UnitType } from '../books/books.models';

export enum OrderStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Processing = 'Processing',
  Shipped = 'Shipped',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded'
}

export enum PaymentMethod {
  COD = 'COD',
  MoMo = 'MoMo',
  VNPay = 'VNPay',
  OnePay = 'OnePay',
  BankTransfer = 'BankTransfer'
}

export interface OrderListItemResponse {
  id: string;
  code: string;
  customerId: string;
  customerName: string;
  customerPhone?: Nullable<string>;
  orderDate: string;
  itemCount: number;
  totalAmount: number;
  shippingAmount: number;
  discountAmount: number;
  finalAmount: number;
  paymentMethod: PaymentMethod;
  orderStatus: OrderStatus;
  isPaid: boolean;
  paidAt?: Nullable<string>;
  shippingAddress?: Nullable<string>;
  provinceName?: Nullable<string>;
  communeName?: Nullable<string>;
  couponCode?: Nullable<string>;
  totalItems: number;
}

/** BE: OrderDetailResponse - khong ke thua OrderListItemResponse (BE tao class rieng). */
export interface OrderDetailResponse {
  id: string;
  code: string;
  customerId: string;
  customerName: string;
  customerEmail?: Nullable<string>;
  customerPhone?: Nullable<string>;
  staffId?: Nullable<string>;
  staffName?: Nullable<string>;
  couponId?: Nullable<string>;
  couponCode?: Nullable<string>;
  orderDate: string;
  itemCount: number;
  orderStatus: OrderStatus;
  paymentMethod: PaymentMethod;
  totalAmount: number;
  shippingAmount: number;
  discountAmount: number;
  finalAmount: number;
  isPaid: boolean;
  paidAt?: Nullable<string>;
  cancelledAt?: Nullable<string>;
  cancelledBy?: Nullable<string>;
  cancelReason?: Nullable<string>;
  shippingAddress?: Nullable<string>;
  provinceName?: Nullable<string>;
  communeName?: Nullable<string>;
  trackingCode?: Nullable<string>;
  customerNote?: Nullable<string>;
  staffNote?: Nullable<string>;
  isDeleted: boolean;
  createdAt: string;
  rowVersion?: Nullable<string>;
  details: OrderDetailResponseItem[];
}

export interface OrderDetailResponseItem {
  id: string;
  orderId: string;
  bookId: string;
  bookTitle: string;
  bookSlug?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  authorName?: Nullable<string>;
  isbnSnapshot?: Nullable<string>;
  unitType: UnitType;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  lineTotal: number;
  dueDate?: Nullable<string>;
  returnedDate?: Nullable<string>;
}

export interface OrderSearchRequest {
  keyword?: string;
  orderStatus?: OrderStatus;
  paymentMethod?: PaymentMethod;
  customerId?: string;
  staffId?: string;
  isPaid?: boolean;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  isDeleted?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

/** BE: UpdateOrderStatusRequest. */
export interface UpdateOrderStatusRequest {
  orderStatus: OrderStatus;
  reason?: string;
  staffNote?: string;
  rowVersion?: string;
}

/** BE: CancelOrderRequest. */
export interface CancelOrderRequest {
  reason: string;
}

/** BE: RefundOrderRequest. */
export interface RefundOrderRequest {
  reason: string;
  amount: number;
}

/** BE: MarkPaidRequest. */
export interface MarkPaidRequest {
  paymentMethod: PaymentMethod;
  transactionCode?: string;
}

export interface CreateOrderRequest {
  customerId: string;
  details: {
    bookId: string;
    unitType: UnitType;
    quantity: number;
    unitPrice?: number;
    dueDate?: string;
  }[];
  shippingAddress: string;
  provinceId?: string;
  communeId?: string;
  couponCode?: string;
  shippingAmount?: number;
  customerNote?: string;
  paymentMethod: PaymentMethod;
}

/** BE: OrderStatusHistoryResponse. */
export interface OrderStatusHistoryResponse {
  id: string;
  fromStatus: OrderStatus;
  toStatus: OrderStatus;
  note?: Nullable<string>;
  changedBy?: Nullable<string>;
  changedByName?: Nullable<string>;
  changedAt: string;
}

export type OrderPagedResult = PagedResult<OrderListItemResponse>;
