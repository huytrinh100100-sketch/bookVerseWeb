import { Nullable } from '../../shared/models/api.models';
import { BookStatus, UnitType } from '../admin/books/books.models';
import { OrderStatus, PaymentMethod } from '../admin/orders/orders.models';
import { VipTier } from '../admin/customers/customers.models';
import { ReviewListItemResponse } from '../admin/reviews/reviews.models';

export interface CatalogQuery {
  keyword?: string;
  categoryId?: string;
  authorId?: string;
  publisherId?: string;
  unitType?: UnitType;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  languages?: string[];
  formats?: string[];
  inStock?: boolean;
  sortBy?: 'curator' | 'price-asc' | 'price-desc' | 'bestseller' | 'newest';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CatalogFacet {
  categoryId: string;
  categoryName: string;
  count: number;
}

export interface CatalogResult {
  items: CatalogItemResponse[];
  totalItems: number;
  page: number;
  pageSize: number;
  facets: {
    categories: CatalogFacet[];
    publishers: CatalogFacet[];
    languages: CatalogFacet[];
    formats: CatalogFacet[];
    priceRanges: { label: string; min: number; max: number; count: number }[];
  };
}

/** Item trong danh muc storefront - khop voi BE CatalogItemResponse. */
export interface CatalogItemResponse {
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
  originalPrice: number;
  discountPercent: number;
  pageCount: number;
  totalQuantity: number;
  language?: Nullable<string>;
  format?: Nullable<string>;
  stockForSell: number;
  stockForRent: number;
  averageRating?: Nullable<number>;
  reviewCount: number;
  soldCount: number;
  status: BookStatus;
  isActive: boolean;
  isFeatured: boolean;
  isNew: boolean;
  createdAt: string;
  updatedAt?: Nullable<string>;
}

/** Chi tiet sach tren storefront - khop voi BE CatalogDetailResponse. */
export interface CatalogDetailResponse {
  id: string;
  code: string;
  title: string;
  subtitle?: Nullable<string>;
  slug: string;
  isbn?: Nullable<string>;
  description?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  categoryId?: Nullable<string>;
  categoryName?: Nullable<string>;
  authorId?: Nullable<string>;
  authorName?: Nullable<string>;
  publisherId?: Nullable<string>;
  publisherName?: Nullable<string>;
  priceForSell: number;
  priceForRent: number;
  pageCount: number;
  publicationYear?: Nullable<number>;
  language?: Nullable<string>;
  format?: Nullable<string>;
  weight?: Nullable<number>;
  dimensions?: Nullable<string>;
  totalQuantity: number;
  stockForSell: number;
  stockForRent: number;
  averageRating?: Nullable<number>;
  reviewCount: number;
  soldCount: number;
  status: BookStatus;
  isActive: boolean;
  isFeatured: boolean;
  createdAt: string;
  deletedAt?: Nullable<string>;
  deletedBy?: Nullable<string>;
  reviews: ReviewListItemResponse[];
  relatedBooks: CatalogItemResponse[];
}

/** BE: CartItemResponse. */
export interface CartItem {
  id: string;
  bookId: string;
  title: string;
  bookTitle?: Nullable<string>;
  bookSlug?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  unitType: UnitType;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  isWishlisted: boolean;
  availableStock: number;
}

/** BE: CartResponse. */
export interface CartState {
  items: CartItem[];
  couponCode?: Nullable<string>;
  discountAmount: number;
  shippingAmount: number;
  freeShippingThreshold: number;
  totalAmount: number;
  itemCount: number;
  subTotal: number;
  finalAmount: number;
  totalQuantity: number;
  freeShippingRemaining?: Nullable<number>;
}

export interface CustomerAddress {
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

export interface CreateShopAddressRequest {
  recipientName: string;
  phoneNumber: string;
  address: string;
  provinceId: string;
  communeId: string;
  isDefault: boolean;
  note?: string;
}

/** BE: CheckoutRequest. */
export interface CheckoutPayload {
  addressId?: string;
  carrierCode?: string;
  shippingAmount?: number;
  shippingAddress?: string;
  provinceId?: string;
  communeId?: string;
  recipientName?: string;
  phoneNumber?: string;
  paymentMethod: PaymentMethod;
  couponCode?: string;
  customerNote?: string;
  note?: string;
  saveAddress?: boolean;
  details: { bookId: string; unitType: UnitType; quantity: number }[];
}

/** BE: CheckoutResponse. */
export interface ShopOrderSummary {
  id: string;
  orderId: string;
  code: string;
  orderCode: string;
  orderDate: string;
  totalAmount: number;
  finalAmount: number;
  shippingAmount: number;
  discountAmount: number;
  paymentMethod: PaymentMethod;
  orderStatus: OrderStatus;
  itemCount: number;
  previewTitles: string[];
  trackingCode?: Nullable<string>;
  paymentUrl?: Nullable<string>;
  message: string;
}

export interface ShopReviewPayload {
  bookId: string;
  rating: number;
  title?: string;
  content: string;
  mediaUrls?: string[];
}

export interface ShopCustomerProfile {
  id: string;
  fullName: string;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  avatarUrl?: Nullable<string>;
  vipTier: VipTier;
  rewardPoints: number;
  wishlistBookCount: number;
  addressCount: number;
}

export interface UpdateShopProfileRequest {
  fullName: string;
  email?: string;
  phoneNumber?: string;
  avatarUrl?: string;
}

/** BE: HomeResponse. */
export interface StorefrontHome {
  banners: HomeBanner[];
  categories: HomeCategory[];
  featuredBooks: CatalogItemResponse[];
  newBooks: CatalogItemResponse[];
  bestSellers: CatalogItemResponse[];
  articles: HomeArticle[];
  coupons: HomeCoupon[];
}

export interface HomeBanner {
  id: string;
  title: string;
  subtitle?: Nullable<string>;
  description?: Nullable<string>;
  imageUrl?: Nullable<string>;
  linkUrl?: Nullable<string>;
  badgeText?: Nullable<string>;
  sortOrder: number;
  isActive: boolean;
}

export interface HomeCategory {
  id: string;
  code: string;
  name: string;
  slug?: Nullable<string>;
  bookCount?: number;
}

export interface HomeArticle {
  id: string;
  title: string;
  slug: string;
  summary?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  categoryName?: Nullable<string>;
  authorName?: Nullable<string>;
  viewCount: number;
  publishedAt?: Nullable<string>;
}

export interface HomeCoupon {
  id: string;
  code: string;
  name: string;
  discountType: string;
  discountValue: number;
  minOrderValue: number;
  maxDiscountAmount?: Nullable<number>;
  endDate?: Nullable<string>;
}

/** BE: WishlistItemResponse. */
export interface WishlistItem {
  id: string;
  bookId: string;
  bookTitle: string;
  bookSlug?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  priceForSell: number;
  priceForRent: number;
  createdAt: string;
}

/** BE: ShopOrderLineResponse. */
export interface ShopOrderLine {
  bookId: string;
  title: string;
  slug?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  unitType: UnitType;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

/** BE: OrderStatusHistoryResponse. */
export interface ShopOrderTimelineEntry {
  id: string;
  fromStatus: OrderStatus;
  toStatus: OrderStatus;
  note?: Nullable<string>;
  changedBy?: Nullable<string>;
  changedAt: string;
}

/** BE: ShopOrderDetailResponse. */
export interface ShopOrderDetail {
  id: string;
  code: string;
  orderCode: string;
  orderDate: string;
  totalAmount: number;
  finalAmount: number;
  shippingAmount: number;
  discountAmount: number;
  paymentMethod: PaymentMethod;
  orderStatus: OrderStatus;
  itemCount: number;
  previewTitles: string[];
  trackingCode?: Nullable<string>;
  paymentUrl?: Nullable<string>;
  message: string;
  recipientName?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  shippingAddress?: Nullable<string>;
  carrierName?: Nullable<string>;
  customerNote?: Nullable<string>;
  items: ShopOrderLine[];
  timeline: ShopOrderTimelineEntry[];
}

