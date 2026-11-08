import { Nullable, PagedResult } from '../../../shared/models/api.models';
import { ReviewListItemResponse } from '../reviews/reviews.models';

/** Don vị mua ban cua BookVerse: doc theo quy uoc da chot (Sell / Rent). */
export enum UnitType {
  Sell = 'Sell',
  Rent = 'Rent'
}

/** Trang thai hien thi tren storefront. */
export enum BookStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived'
}

/** Trang thai ban sao vat ly (nguon su that cho nghiep vu muon). */
export enum BookCopyStatus {
  Available = 'Available',
  Lent = 'Lent',
  Reserved = 'Reserved',
  Lost = 'Lost',
  Damaged = 'Damaged',
  Returned = 'Returned'
}

export interface BookListItemResponse {
  id: string;
  code: string;
  title: string;
  subtitle?: Nullable<string>;
  slug: string;
  isbn?: Nullable<string>;
  categoryId?: Nullable<string>;
  categoryName?: Nullable<string>;
  authorId?: Nullable<string>;
  authorName?: Nullable<string>;
  publisherId?: Nullable<string>;
  publisherName?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  priceForSell: number;
  priceForRent: number;
  pageCount: number;
  totalQuantity: number;
  stockForSell: number;
  stockForRent: number;
  status: BookStatus;
  isActive: boolean;
  isFeatured: boolean;
  averageRating?: Nullable<number>;
  reviewCount: number;
  soldCount: number;
  isDeleted: boolean;
  createdAt: string;
  updatedAt?: Nullable<string>;
}

export interface BookDetailResponse extends BookListItemResponse {
  description?: Nullable<string>;
  publicationYear?: Nullable<number>;
  language?: Nullable<string>;
  format?: Nullable<string>;
  weight?: Nullable<number>;
  dimensions?: Nullable<string>;
  deletedAt?: Nullable<string>;
  deletedBy?: Nullable<string>;
  rowVersion?: Nullable<string>;
  copies: BookCopyResponse[];
  reviews: ReviewListItemResponse[];
}

export interface BookSearchRequest {
  keyword?: string;
  categoryId?: string;
  authorId?: string;
  publisherId?: string;
  status?: BookStatus;
  unitType?: UnitType;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  stockLevel?: 'in-stock' | 'low-stock' | 'out-of-stock';
  isFeatured?: boolean;
  isActive?: boolean;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateBookRequest {
  code?: string;
  title: string;
  subtitle?: string;
  slug?: string;
  isbn?: string;
  categoryId?: string;
  authorId?: string;
  publisherId?: string;
  coverImageUrl?: string;
  description?: string;
  priceForSell: number;
  priceForRent: number;
  pageCount?: number;
  publicationYear?: number;
  language?: string;
  format?: string;
  weight?: number;
  dimensions?: string;
  quantity?: number;
  status: BookStatus;
  isActive: boolean;
  isFeatured: boolean;
}

/** BE: UpdateBookRequest - moi truong chi nhan field kha null. */
export interface UpdateBookRequest {
  code?: string;
  title?: string;
  subtitle?: string;
  slug?: string;
  isbn?: string;
  description?: string;
  coverImageUrl?: string;
  categoryId?: string;
  authorId?: string;
  publisherId?: string;
  priceForSell?: number;
  priceForRent?: number;
  pageCount?: number;
  publicationYear?: number;
  language?: string;
  format?: string;
  weight?: number;
  dimensions?: string;
  status?: BookStatus;
  isFeatured?: boolean;
  isActive?: boolean;
  rowVersion?: string;
}

export interface BookStockAdjustRequest {
  stockForSell: number;
  stockForRent: number;
  reason: string;
}

export type BookPagedResult = PagedResult<BookListItemResponse>;

/** BE: BookCopyListItemResponse. */
export interface BookCopyResponse {
  id: string;
  bookId: string;
  bookTitle: string;
  barcode: string;
  conditionStatus: string;
  shelfCode?: Nullable<string>;
  warehouseId?: Nullable<string>;
  warehouseName?: Nullable<string>;
  acquiredDate?: Nullable<string>;
  dueDate?: Nullable<string>;
  returnedDate?: Nullable<string>;
  status: BookCopyStatus;
  currentOrderId?: Nullable<string>;
  notes?: Nullable<string>;
  isDeleted: boolean;
}

/** BE: CreateBookCopyRequest. */
export interface CreateBookCopyRequest {
  bookId: string;
  barcode?: string;
  conditionStatus: string;
  shelfCode?: string;
  warehouseId?: string;
  acquiredDate?: string;
  notes?: string;
  quantity?: number;
}

/** BE: UpdateBookCopyRequest. */
export interface UpdateBookCopyRequest {
  conditionStatus?: string;
  shelfCode?: string;
  warehouseId?: string;
  acquiredDate?: string;
  dueDate?: string;
  returnedDate?: string;
  status?: BookCopyStatus;
  notes?: string;
}

export interface BookCopySearchRequest {
  keyword?: string;
  bookId?: string;
  warehouseId?: string;
  status?: BookCopyStatus;
  page?: number;
  pageSize?: number;
}

export type BookCopyPagedResult = PagedResult<BookCopyResponse>;
