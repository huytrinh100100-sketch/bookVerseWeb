import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { StorefrontStore } from './storefront.store';

@Component({
  selector: 'app-storefront-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="shop">
      <header class="shop__header">
        <a class="shop__brand" routerLink="/shop">
          <img class="shop__logo" src="assets/logo-bookverse-mark.png" alt="BookVerse" />
          <span>BookVerse</span>
        </a>
        <nav class="shop__nav">
          <a routerLink="/shop" routerLinkActive="shop__link--active">Trang chủ</a>
          <a routerLink="/shop/catalog" routerLinkActive="shop__link--active">Danh mục</a>
          <a routerLink="/shop/curations" routerLinkActive="shop__link--active">Tuyển chọn</a>
          <a routerLink="/shop/account" routerLinkActive="shop__link--active">Tài khoản</a>
        </nav>
        <div class="shop__actions">
          <input type="search" placeholder="Tìm theo tên, tác giả, ISBN..." />
          <a class="shop__icon" routerLink="/shop/wishlist"><i class="pi pi-bookmark"></i></a>
          <a class="shop__icon" routerLink="/shop/cart">
            <i class="pi pi-shopping-bag"></i>
            @if (store.cartItemCount() > 0) {
              <span class="shop__badge">{{ store.cartItemCount() }}</span>
            }
          </a>
          @if (auth.isAuthenticated()) {
            <span class="shop__user"><i class="pi pi-user"></i> {{ auth.displayName() }}</span>
            <button type="button" class="shop__logout" (click)="logout()">Đăng xuất</button>
          } @else {
            <a class="shop__login" routerLink="/login"><i class="pi pi-sign-in"></i> Đăng nhập</a>
          }
        </div>
      </header>

      <main class="shop__main">
        <router-outlet />
      </main>

      <footer class="shop__footer">
        <div class="shop__footer-col">
          <strong>BookVerse</strong>
          <p>Khuyến chỉ đọc tuyển chọn mỗi tuần.</p>
        </div>
        <div class="shop__footer-col">
          <strong>Chính sách</strong>
          <a routerLink="/shop/shipping-policy">Giao hàng</a>
          <a routerLink="/shop/returns">Đổi trả</a>
          <a routerLink="/shop/terms">Điều khoản</a>
        </div>
        <div class="shop__footer-col">
          <strong>Liên hệ</strong>
          <a href="mailto:support@bookverse.vn">support&#64;bookverse.vn</a>
          <span>1900 1234</span>
        </div>
      </footer>
    </div>
  `,
  styles: [
    `
      .shop { min-height: 100vh; display: flex; flex-direction: column; background: var(--surface, #f7f9fb); }
      .shop__header { display: flex; align-items: center; gap: 1.5rem; padding: 1rem 2rem; background: var(--surface-container-lowest, #fff); border-bottom: 1px solid var(--outline-variant, #c6c6cd); position: sticky; top: 0; z-index: 20; }
      .shop__brand { font-weight: 800; font-size: 1.25rem; text-decoration: none; color: var(--on-surface, #191c1e); display: flex; align-items: center; gap: 0.5rem; }
      .shop__logo { width: 2.5rem; height: 2.5rem; object-fit: contain; flex-shrink: 0; }
      .shop__nav { display: flex; gap: 1rem; }
      .shop__nav a { text-decoration: none; color: var(--on-surface-variant, #45464d); font-size: 0.9375rem; }
      .shop__link--active { color: var(--secondary, #006c49) !important; font-weight: 700; }
      .shop__actions { margin-left: auto; display: flex; align-items: center; gap: 0.75rem; }
      .shop__actions input { padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: none; background: var(--surface-container-low, #f2f4f6); width: 18rem; }
      .shop__icon { position: relative; font-size: 1.25rem; color: inherit; text-decoration: none; }
      .shop__badge { position: absolute; top: -0.25rem; right: -0.375rem; background: var(--error, #ba1a1a); color: #fff; border-radius: 9999px; font-size: 0.625rem; min-width: 1rem; text-align: center; padding: 0 0.25rem; }
      .shop__user { display: inline-flex; align-items: center; gap: 0.375rem; font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .shop__login, .shop__logout { display: inline-flex; align-items: center; gap: 0.375rem; font-size: 0.875rem; padding: 0.375rem 0.75rem; border-radius: 0.5rem; border: 1px solid var(--outline-variant, #c6c6cd); background: transparent; color: var(--on-surface, #191c1e); text-decoration: none; cursor: pointer; font-weight: 600; }
      .shop__login:hover, .shop__logout:hover { background: var(--surface-container-low, #f2f4f6); }
      .shop__main { flex: 1; padding: 1.5rem 2rem; }
      .shop__footer { display: grid; grid-template-columns: repeat(3, 1fr); gap: 2rem; padding: 2rem; background: var(--primary-container, #131b2e); color: var(--on-primary-container, #d8e2ff); }
      .shop__footer-col { display: flex; flex-direction: column; gap: 0.375rem; }
      .shop__footer-col a { color: inherit; font-size: 0.875rem; }
    `
  ]
})
export class StorefrontLayoutComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  async ngOnInit(): Promise<void> {
    // Chi tai gioi hang khi da dang nhap - API tra 401 neu khong co token.
    if (this.auth.isAuthenticated()) {
      await this.store.loadCart();
    }
  }

  async logout(): Promise<void> {
    this.auth.clearSession();
    this.store.reset();
    await this.router.navigate(['/shop']);
  }
}
