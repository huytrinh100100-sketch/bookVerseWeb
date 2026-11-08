import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';
import { StorefrontApiService } from './storefront-api.service';
import {
  CartItem,
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

const EMPTY_CART: CartState = {
  items: [],
  discountAmount: 0,
  shippingAmount: 0,
  freeShippingThreshold: 500000,
  totalAmount: 0,
  itemCount: 0,
  subTotal: 0,
  finalAmount: 0,
  totalQuantity: 0
};

@Injectable({ providedIn: 'root' })
export class StorefrontStore {
  private readonly api = inject(StorefrontApiService);
  private readonly toast = inject(ToastService);

  readonly catalog = signal<CatalogResult | null>(null);
  readonly home = signal<StorefrontHome | null>(null);
  readonly bookDetail = signal<CatalogDetailResponse | null>(null);
  readonly cart = signal<CartState>(EMPTY_CART);
  readonly addresses = signal<CustomerAddress[]>([]);
  readonly orders = signal<ShopOrderSummary[]>([]);
  readonly profile = signal<ShopCustomerProfile | null>(null);
  readonly wishlist = signal<WishlistItem[]>([]);
  readonly loading = signal(false);

  readonly cartItemCount = computed(() => this.cart().itemCount);
  readonly hasFreeShipping = computed(() => this.cart().totalAmount >= this.cart().freeShippingThreshold);

  async loadHome(): Promise<void> {
    const response = await firstValueFrom(this.api.getHome());
    this.home.set(response.data ?? null);
  }

  async searchCatalog(query: CatalogQuery): Promise<void> {    this.loading.set(true);
    try {
      const response = await firstValueFrom(this.api.searchCatalog(query));
      this.catalog.set(response.data ?? null);
    } finally {
      this.loading.set(false);
    }
  }

  async loadBookDetail(slug: string): Promise<void> {
    this.loading.set(true);
    try {
      const response = await firstValueFrom(this.api.getBookBySlug(slug));
      this.bookDetail.set(response.data ?? null);
    } finally {
      this.loading.set(false);
    }
  }

  async loadCart(): Promise<void> {
    const response = await firstValueFrom(this.api.getCart());
    this.cart.set(response.data ?? EMPTY_CART);
  }

  async addToCart(bookId: string, unitType: string, quantity: number): Promise<void> {
    const response = await firstValueFrom(this.api.addToCart(bookId, unitType, quantity));
    if (response.isSuccess) {
      this.cart.set(response.data ?? EMPTY_CART);
      this.toast.success('Đã thêm vào giỏ sách.');
    }
  }

  async updateQuantity(itemId: string, quantity: number): Promise<void> {
    const response = await firstValueFrom(this.api.updateCartItem(itemId, quantity));
    if (response.isSuccess) {
      this.cart.set(response.data ?? EMPTY_CART);
    }
  }

  async removeItem(itemId: string): Promise<void> {
    const response = await firstValueFrom(this.api.removeCartItem(itemId));
    if (response.isSuccess) {
      this.cart.set(response.data ?? EMPTY_CART);
      this.toast.info('Đã xóa sản phẩm khỏi giỏ.');
    }
  }

  async applyCoupon(code: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.applyCoupon(code));
    if (response.isSuccess) {
      this.cart.set(response.data ?? EMPTY_CART);
      this.toast.success('Đã áp dụng mã giảm giá.');
      return true;
    }
    return false;
  }

  async toggleWishlist(bookId: string): Promise<void> {
    const response = await firstValueFrom(this.api.toggleWishlist(bookId));
    if (response.isSuccess) {
      await this.loadWishlist();
      this.toast.info('Đã cập nhật danh sách yêu thích.');
    }
  }

  isWishlisted(bookId: string): boolean {
    return this.wishlist().some((item) => item.bookId === bookId);
  }

  async loadWishlist(): Promise<void> {
    const response = await firstValueFrom(this.api.getWishlist());
    this.wishlist.set(response.data ?? []);
  }

  async loadAddresses(): Promise<void> {
    const response = await firstValueFrom(this.api.getAddresses());
    this.addresses.set(response.data ?? []);
  }

  async createAddress(payload: CreateShopAddressRequest): Promise<CustomerAddress | null> {
    const response = await firstValueFrom(this.api.createAddress(payload));
    if (response.isSuccess) {
      this.toast.success('Thêm địa chỉ thành công.');
      await this.loadAddresses();
      return response.data ?? null;
    }
    return null;
  }

  async updateAddress(id: string, payload: CreateShopAddressRequest): Promise<CustomerAddress | null> {
    const response = await firstValueFrom(this.api.updateAddress(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật địa chỉ thành công.');
      await this.loadAddresses();
      return response.data ?? null;
    }
    return null;
  }

  async deleteAddress(id: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.deleteAddress(id));
    if (response.isSuccess) {
      this.toast.info('Đã xóa địa chỉ.');
      await this.loadAddresses();
      return true;
    }
    return false;
  }

  async loadMyOrders(): Promise<void> {
    const response = await firstValueFrom(this.api.getMyOrders());
    this.orders.set(response.data ?? []);
  }

  async loadProfile(): Promise<void> {
    const response = await firstValueFrom(this.api.getMyProfile());
    this.profile.set(response.data ?? null);
  }

  async updateProfile(payload: UpdateShopProfileRequest): Promise<ShopCustomerProfile | null> {
    const response = await firstValueFrom(this.api.updateMyProfile(payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật hồ sơ thành công.');
      this.profile.set(response.data ?? null);
      return response.data ?? null;
    }
    return null;
  }

  async cancelOrder(id: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.cancelMyOrder(id));
    if (response.isSuccess) {
      this.toast.success('Hủy đơn hàng thành công.');
      await this.loadMyOrders();
      return true;
    }
    return false;
  }

  async loadOrderDetail(id: string): Promise<ShopOrderDetail | null> {
    const response = await firstValueFrom(this.api.getMyOrderDetail(id));
    return response.data ?? null;
  }

  async submitReview(payload: ShopReviewPayload): Promise<boolean> {
    const response = await firstValueFrom(this.api.submitReview(payload));
    if (response.isSuccess) {
      this.toast.success('Gửi đánh giá thành công. Đánh giá đang chờ duyệt.');
      return true;
    }
    return false;
  }

  async checkout(payload: CheckoutPayload): Promise<ShopOrderSummary | null> {
    const response = await firstValueFrom(this.api.checkout(payload));
    if (response.isSuccess) {
      this.toast.success('Đặt hàng thành công.');
      await this.loadCart();
      return response.data ?? null;
    }
    return null;
  }

  /** Xoa du lieu cua user dang dang nhap (goi khi dang xuat). */
  reset(): void {
    this.cart.set(EMPTY_CART);
    this.orders.set([]);
    this.addresses.set([]);
    this.profile.set(null);
    this.wishlist.set([]);
  }
}
