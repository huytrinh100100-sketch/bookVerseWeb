import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UnitType } from '../../admin/books/books.models';
import { CatalogItemResponse } from '../storefront.models';
import { StorefrontStore } from '../storefront.store';

type HomeTab = 'bestSellers' | 'newBooks' | 'featured';

@Component({
  selector: 'app-shop-home',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (banner(); as b) {
      <section class="hero">
        <div>
          <p class="hero__eyebrow">{{ b.badgeText || 'Tuyển chọn của ngày hôm nay' }}</p>
          <h1 class="hero__title">{{ b.title }}</h1>
          <p class="hero__desc">{{ b.subtitle || b.description }}</p>
          <a class="btn btn--primary" [routerLink]="b.linkUrl || '/shop/catalog'"><i class="pi pi-arrow-right"></i> Khám phá danh mục</a>
        </div>
      </section>
    }

    @if (categories().length > 0) {
      <section class="cats">
        @for (cat of categories(); track cat.id) {
          <a class="cats__item" [routerLink]="['/shop/catalog']">
            <i class="pi pi-book"></i>
            <span>{{ cat.name }}</span>
            @if (cat.bookCount) {
              <span class="cats__count">{{ cat.bookCount }}</span>
            }
          </a>
        }
      </section>
    }

    <section class="tabs">
      @for (tab of tabs; track tab.value) {
        <button type="button" [class.tabs__item--active]="activeTab() === tab.value" (click)="activeTab.set(tab.value)">{{ tab.label }}</button>
      }
    </section>

    <section class="grid">
      @for (book of tabBooks(); track book.id) {
        <article class="book-card">
          <a class="book-card__cover" [routerLink]="['/shop/books', book.slug]">
            @if (book.coverImageUrl) {
              <img [src]="book.coverImageUrl" [alt]="book.title" />
            } @else {
              <span class="book-card__placeholder"><i class="pi pi-book"></i></span>
            }
          </a>
          <h3 class="book-card__title">{{ book.title }}</h3>
          <p class="book-card__author">{{ book.authorName }}</p>
          <p class="book-card__price">{{ book.priceForSell | number }} d</p>
          <div class="book-card__actions">
            <button type="button" class="btn btn--ghost" [class.btn--active]="isWishlisted(book.id)" (click)="toggleWishlist(book.id)" title="Yêu thích">
              <i class="pi pi-bookmark"></i>
            </button>
            <button type="button" class="btn btn--primary" (click)="addToCart(book.id)">
              <i class="pi pi-shopping-bag"></i> Thêm vào giỏ
            </button>
          </div>
        </article>
      } @empty {
        <div class="home-empty">
          <i class="pi pi-book"></i>
          <h3>Chưa có sách nào</h3>
          <p>Cửa hàng đang được thiết lập. Vui lòng quay lại sau.</p>
        </div>
      }
    </section>

    @if (coupons().length > 0) {
      <section class="promos">
        <h2>Khuyến mại đang chạy</h2>
        <div class="promos__grid">
          @for (coupon of coupons(); track coupon.id) {
            <div class="promo">
              <span class="promo__code">{{ coupon.code }}</span>
              <span class="promo__name">{{ coupon.name }}</span>
              <span class="promo__value">
                @if (coupon.discountType === 'Percent') {
                  {{ coupon.discountValue }}%
                } @else {
                  {{ coupon.discountValue | number }} d
                }
              </span>
              <span class="promo__min">Đơn tối thiểu {{ coupon.minOrderValue | number }} d</span>
            </div>
          }
        </div>
      </section>
    }
  `,
  styles: [
    `
      .hero { background: var(--secondary-container, #6cf8bb); border-radius: 1rem; padding: 3rem 2rem; margin-bottom: 1.5rem; }
      .hero__eyebrow { text-transform: uppercase; letter-spacing: 0.05em; font-size: 0.75rem; margin: 0 0 0.5rem; }
      .hero__title { font-size: 2.25rem; margin: 0 0 0.75rem; letter-spacing: -0.025em; }
      .hero__desc { margin: 0 0 1.25rem; max-width: 40rem; }
      .cats { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
      .cats__item { display: inline-flex; align-items: center; gap: 0.375rem; padding: 0.5rem 0.875rem; border-radius: 9999px; background: var(--surface-container-low, #f2f4f6); text-decoration: none; color: var(--on-surface, #191c1e); font-size: 0.8125rem; font-weight: 600; }
      .cats__item:hover { background: var(--secondary-container, #6cf8bb); }
      .cats__count { font-size: 0.6875rem; background: var(--surface-container-high, #e6e8ea); border-radius: 9999px; padding: 0.0625rem 0.375rem; }
      .tabs { display: flex; gap: 0.5rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
      .tabs__item--active { font-weight: 700; color: var(--secondary, #006c49); border-bottom: 2px solid var(--secondary, #006c49); }
      .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(13rem, 1fr)); gap: 1.25rem; }
      .book-card { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); display: flex; flex-direction: column; gap: 0.375rem; }
      .book-card__cover { display: block; aspect-ratio: 2/3; background: var(--surface-container-low, #f2f4f6); border-radius: 0.5rem; overflow: hidden; }
      .book-card__cover img { width: 100%; height: 100%; object-fit: cover; }
      .book-card__placeholder { display: grid; place-items: center; height: 100%; font-size: 2rem; color: var(--outline, #76777d); }
      .book-card__title { font-size: 0.9375rem; margin: 0.25rem 0 0; font-weight: 600; }
      .book-card__author { margin: 0; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .book-card__price { margin: 0.25rem 0 0.5rem; font-weight: 700; }
      .book-card__actions { display: flex; gap: 0.375rem; margin-top: auto; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.75rem; cursor: pointer; font-size: 0.8125rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--active { background: var(--tertiary-fixed, #d8e2ff); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .home-empty { grid-column: 1 / -1; text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .home-empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .home-empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .home-empty p { margin: 0; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .promos { margin-top: 2rem; }
      .promos h2 { font-size: 1.25rem; margin: 0 0 0.75rem; }
      .promos__grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr)); gap: 0.75rem; }
      .promo { display: flex; flex-direction: column; gap: 0.25rem; padding: 0.875rem; border-radius: 0.75rem; background: var(--surface-container-lowest, #fff); box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); border-left: 3px solid var(--secondary, #006c49); }
      .promo__code { font-family: monospace; font-weight: 700; color: var(--secondary, #006c49); }
      .promo__name { font-size: 0.875rem; font-weight: 600; }
      .promo__value { font-size: 1.125rem; font-weight: 800; }
      .promo__min { font-size: 0.75rem; color: var(--on-surface-variant, #45464d); }
    `
  ]
})
export class StorefrontHomePageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  readonly activeTab = signal<HomeTab>('bestSellers');

  readonly tabs: { value: HomeTab; label: string }[] = [
    { value: 'bestSellers', label: 'Bán chạy nhất' },
    { value: 'newBooks', label: 'Mới phát hành' },
    { value: 'featured', label: 'Nổi bật' }
  ];

  readonly banner = computed(() => this.store.home()?.banners?.[0] ?? null);
  readonly categories = computed(() => this.store.home()?.categories ?? []);
  readonly coupons = computed(() => this.store.home()?.coupons ?? []);

  readonly tabBooks = computed<CatalogItemResponse[]>(() => {
    const home = this.store.home();
    if (!home) {
      return [];
    }
    switch (this.activeTab()) {
      case 'newBooks':
        return home.newBooks ?? [];
      case 'featured':
        return home.featuredBooks ?? [];
      default:
        return home.bestSellers ?? [];
    }
  });

  async ngOnInit(): Promise<void> {
    await this.store.loadHome();
    await this.store.loadWishlist();
  }

  isWishlisted(bookId: string): boolean {
    return this.store.isWishlisted(bookId);
  }

  async toggleWishlist(bookId: string): Promise<void> {
    await this.store.toggleWishlist(bookId);
  }

  async addToCart(bookId: string): Promise<void> {
    await this.store.addToCart(bookId, UnitType.Sell, 1);
  }
}
