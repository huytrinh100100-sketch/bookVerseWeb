import { Nullable, PagedResult } from '../../../shared/models/api.models';
import { BookCopyStatus } from '../books/books.models';

export interface WarehouseResponse {
  id: string;
  code: string;
  name: string;
  address?: Nullable<string>;
  provinceId?: Nullable<string>;
  provinceName?: Nullable<string>;
  isActive: boolean;
  isDeleted: boolean;
  copyCount: number;
  createdAt: string;
}

/** BE: InventoryListItemResponse. */
export interface InventoryRowResponse {
  bookId: string;
  title: string;
  coverImageUrl?: Nullable<string>;
  isbn?: Nullable<string>;
  authorId?: Nullable<string>;
  authorName?: Nullable<string>;
  publisherId?: Nullable<string>;
  publisherName?: Nullable<string>;
  categoryId?: Nullable<string>;
  categoryName?: Nullable<string>;
  warehouseId?: Nullable<string>;
  warehouseName?: Nullable<string>;
  priceForSell: number;
  priceForRent: number;
  totalQuantity: number;
  stockForSell: number;
  stockForRent: number;
  reservedQuantity: number;
  lentQuantity: number;
  velocity: 'fast' | 'medium' | 'slow';
  safetyStock: number;
  reorderPoint: number;
  availableCopies: number;
  threshold: number;
  isLowStock: boolean;
}

export interface InventorySearchRequest {
  keyword?: string;
  warehouseId?: string;
  publisherId?: string;
  stockLevel?: 'critical' | 'low' | 'healthy' | 'out';
  status?: BookCopyStatus;
  lowStockThreshold?: number;
  page?: number;
  pageSize?: number;
}

export interface DraftPurchaseOrderRequest {
  warehouseId: string;
  leadTimeDays: number;
  notes?: string;
  details: { bookId: string; quantity: number }[];
}

/** BE: DraftPurchaseOrderResponse. */
export interface DraftPurchaseOrderResponse {
  id: string;
  code: string;
  warehouseId: string;
  leadTimeDays: number;
  notes?: Nullable<string>;
  totalQuantity: number;
  createdAt: string;
}

/** BE: AuditSheetResponse. */
export interface AuditSheetResponse {
  warehouseName: string;
  generatedAt: string;
  totalBooks: number;
  totalCopies: number;
  rows: AuditSheetRow[];
}

/** BE: AuditSheetRow. */
export interface AuditSheetRow {
  bookId: string;
  title: string;
  isbn?: Nullable<string>;
  systemQuantity: number;
  actualQuantity: number;
  difference: number;
  note?: Nullable<string>;
}

export type InventoryPagedResult = PagedResult<InventoryRowResponse>;
