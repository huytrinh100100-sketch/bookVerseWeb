import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/core.tokens';
import { ApiResponse } from '../../shared/models/api.models';
import {
  CartState,
  CatalogDetailResponse,
  CatalogQuery,
  CatalogResult,
  CheckoutPayload,
  CreateShopAddressRequest,
  CustomerAddress,
  ShopCustomerProfile,
  ShopOrderDetail,
  ShopOrderSummary,
  ShopReviewPayload,
  StorefrontHome,
  UpdateShopProfileRequest,
  WishlistItem
} from './storefront.models';

/** BE: ShopCouponResponse. */
export interface ShopCoupon {
  id: string;
  code: string;
  name: string;
  discountType: string;
  discountValue: number;
  minOrderValue: number;
  maxDiscountAmount?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  usageLimit: number;
  usedCount: number;
  audience?: string | null;
}

@Injectable({ providedIn: 'root' })
export class StorefrontApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getHome(): Observable<ApiResponse<StorefrontHome>> {
    return this.http.get<ApiResponse<StorefrontHome>>(`${this.baseUrl}/storefront/home`);
  }

  searchCatalog(query: CatalogQuery): Observable<ApiResponse<CatalogResult>> {    let httpParams = new HttpParams();
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null || value === '') {
        return;
      }
      if (Array.isArray(value)) {
        value.forEach((item) => (httpParams = httpParams.append(key, String(item))));
      } else {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<CatalogResult>>(`${this.baseUrl}/shop/catalog`, { params: httpParams });
  }

  getBookBySlug(slug: string): Observable<ApiResponse<CatalogDetailResponse>> {
    return this.http.get<ApiResponse<CatalogDetailResponse>>(`${this.baseUrl}/shop/books/${slug}`);
  }

  getCart(): Observable<ApiResponse<CartState>> {
    return this.http.get<ApiResponse<CartState>>(`${this.baseUrl}/shop/cart`);
  }

  addToCart(bookId: string, unitType: string, quantity: number): Observable<ApiResponse<CartState>> {
    return this.http.post<ApiResponse<CartState>>(`${this.baseUrl}/shop/cart/items`, { bookId, unitType, quantity });
  }
  updateCartItem(itemId: string, quantity: number): Observable<ApiResponse<CartState>> {
    return this.http.put<ApiResponse<CartState>>(`${this.baseUrl}/shop/cart/items/${itemId}`, { quantity });
  }

  removeCartItem(itemId: string): Observable<ApiResponse<CartState>> {
    return this.http.delete<ApiResponse<CartState>>(`${this.baseUrl}/shop/cart/items/${itemId}`);
  }

  applyCoupon(code: string): Observable<ApiResponse<CartState>> {
    return this.http.post<ApiResponse<CartState>>(`${this.baseUrl}/shop/cart/coupon`, { code });
  }

  getAddresses(): Observable<ApiResponse<CustomerAddress[]>> {
    return this.http.get<ApiResponse<CustomerAddress[]>>(`${this.baseUrl}/shop/addresses`);
  }

  createAddress(payload: CreateShopAddressRequest): Observable<ApiResponse<CustomerAddress>> {
    return this.http.post<ApiResponse<CustomerAddress>>(`${this.baseUrl}/shop/addresses`, payload);
  }

  updateAddress(id: string, payload: CreateShopAddressRequest): Observable<ApiResponse<CustomerAddress>> {
    return this.http.put<ApiResponse<CustomerAddress>>(`${this.baseUrl}/shop/addresses/${id}`, payload);
  }

  deleteAddress(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/shop/addresses/${id}`);
  }

  checkout(payload: CheckoutPayload): Observable<ApiResponse<ShopOrderSummary>> {
    return this.http.post<ApiResponse<ShopOrderSummary>>(`${this.baseUrl}/shop/checkout`, payload);
  }

  getMyOrders(): Observable<ApiResponse<ShopOrderSummary[]>> {
    return this.http.get<ApiResponse<ShopOrderSummary[]>>(`${this.baseUrl}/shop/orders`);
  }

  getMyProfile(): Observable<ApiResponse<ShopCustomerProfile>> {
    return this.http.get<ApiResponse<ShopCustomerProfile>>(`${this.baseUrl}/shop/profile`);
  }

  updateMyProfile(payload: UpdateShopProfileRequest): Observable<ApiResponse<ShopCustomerProfile>> {
    return this.http.put<ApiResponse<ShopCustomerProfile>>(`${this.baseUrl}/shop/profile`, payload);
  }

  getMyOrderDetail(id: string): Observable<ApiResponse<ShopOrderDetail>> {
    return this.http.get<ApiResponse<ShopOrderDetail>>(`${this.baseUrl}/shop/orders/${id}`);
  }

  cancelMyOrder(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/shop/orders/${id}/cancel`, {});
  }

  getWishlist(): Observable<ApiResponse<WishlistItem[]>> {
    return this.http.get<ApiResponse<WishlistItem[]>>(`${this.baseUrl}/shop/wishlist`);
  }

  toggleWishlist(bookId: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/shop/wishlist/${bookId}/toggle`, {});
  }

  getCoupons(): Observable<ApiResponse<ShopCoupon[]>> {
    return this.http.get<ApiResponse<ShopCoupon[]>>(`${this.baseUrl}/shop/coupons`);
  }

  submitReview(payload: ShopReviewPayload): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/shop/reviews`, payload);
  }
}
