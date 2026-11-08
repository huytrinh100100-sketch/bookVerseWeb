import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { UnitType } from '../../admin/books/books.models';
import { StorefrontStore } from '../storefront.store';

@Component({
  selector: 'app-shop-book-details',
  standalone: true,
  imports: [DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (book(); as item) {
      <div class="detail">
        <div class="detail__cover">
          @if (item.coverImageUrl) {
            <img [src]="item.coverImageUrl" [alt]="item.title" />
          } @else {
            <span class="detail__placeholder"><i class="pi pi-book"></i></span>
          }
        </div>

        <div class="detail__info">
          <h1>{{ item.title }}</h1>
          @if (item.subtitle) {
            <p class="detail__subtitle">{{ item.subtitle }}</p>
          }
          <p class="detail__author">{{ item.authorName }} · {{ item.publisherName }}</p>
          <p class="detail__meta">{{ item.isbn }} · {{ item.pageCount }} trang</p>
          <p class="detail__price">{{ item.priceForSell | number }} d</p>

          <div class="detail__qty">
            <button type="button" (click)="decrease()"><i class="pi pi-minus"></i></button>
            <span>{{ quantity() }}</span>
            <button type="button" (click)="increase()"><i class="pi pi-plus"></i></button>
          </div>

          <div class="detail__actions">
            <button type="button" class="btn btn--ghost" (click)="store.toggleWishlist(item.id)">
              <i class="pi pi-bookmark"></i> Yêu thích
            </button>
            <button type="button" class="btn btn--primary" (click)="addToCart(item.id)">
              <i class="pi pi-shopping-bag"></i> Thêm vào giỏ
            </button>
            <button type="button" class="btn btn--accent" (click)="buyNow(item.id)">
              <i class="pi pi-bolt"></i> Mua ngay
            </button>
          </div>

          <section class="specs">
            <h2>Thông số của cuốn sách</h2>
            <dl>
              <dt>ISBN-13</dt><dd>{{ item.isbn }}</dd>
              <dt>Số trang</dt><dd>{{ item.pageCount }}</dd>
              <dt>Năm xuất bản</dt><dd>{{ item.publicationYear ?? 2024 }}</dd>
              <dt>Ngôn ngữ</dt><dd>{{ item.language ?? 'Tiếng Việt' }}</dd>
              <dt>Tồn kho bán</dt><dd>{{ item.stockForSell }} bản</dd>
              <dt>Cho mượn</dt><dd>{{ item.priceForRent | number }} d / tháng</dd>
            </dl>
          </section>
        </div>
      </div>

      <section class="reviews">
        <h2>Đánh giá & Điểm đánh giá</h2>
        <div class="reviews__filters">
          <button type="button">Tất cả (142)</button>
          <button type="button">5 sao (119)</button>
          <button type="button">4 sao (16)</button>
          <button type="button">Có ảnh (38)</button>
          <button type="button">Người đã mua xác thực (135)</button>
        </div>
        <article class="review">
          <strong>Trần Bảo Linh</strong>
          <span class="review__stars"><i class="pi pi-star-fill"></i><i class="pi pi-star-fill"></i><i class="pi pi-star-fill"></i><i class="pi pi-star-fill"></i><i class="pi pi-star-fill"></i></span>
          <p>Bản dịch xuất sắc, chân thương và đọc lại cảm xúc văn bản gốc.</p>
        </article>
      </section>
    }
  `,
  styles: [
    `
      .detail { display: grid; grid-template-columns: 20rem 1fr; gap: 2.5rem; }
      .detail__cover { aspect-ratio: 2/3; background: var(--surface-container-low, #f2f4f6); border-radius: 0.75rem; overflow: hidden; }
      .detail__cover img { width: 100%; height: 100%; object-fit: cover; }
      .detail__placeholder { display: grid; place-items: center; height: 100%; font-size: 3rem; color: var(--outline, #76777d); }
      .detail__info h1 { font-size: 2rem; margin: 0 0 0.25rem; }
      .detail__subtitle { margin: 0 0 0.5rem; color: var(--on-surface-variant, #45464d); }
      .detail__author { margin: 0 0 0.25rem; font-weight: 600; }
      .detail__meta { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .detail__price { font-size: 1.5rem; font-weight: 800; margin: 0 0 1rem; }
      .detail__qty { display: inline-flex; align-items: center; gap: 0.75rem; border: 1px solid var(--outline-variant, #c6c6cd); border-radius: 0.5rem; padding: 0.25rem; margin-bottom: 1rem; }
      .detail__qty button { border: none; background: transparent; cursor: pointer; width: 2rem; height: 2rem; border-radius: 0.375rem; }
      .detail__actions { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 2rem; }
      .specs h2, .reviews h2 { font-size: 1.25rem; margin: 0 0 0.75rem; }
      .specs dl { display: grid; grid-template-columns: 10rem 1fr; gap: 0.375rem; font-size: 0.875rem; }
      .specs dt { color: var(--on-surface-variant, #45464d); }
      .specs dd { margin: 0; }
      .reviews { margin-top: 3rem; }
      .reviews__filters { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem; }
      .review { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; margin-bottom: 0.75rem; }
      .review__stars { color: #a4600a; margin-left: 0.5rem; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.625rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--accent { background: var(--tertiary-container, #001a42); color: #fff; }
    `
  ]
})
export class StorefrontBookDetailsPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  private readonly route = inject(ActivatedRoute);
  readonly quantity = signal(1);

  /** Chi tiet sach tu BE (GET /shop/books/{slug}). */
  readonly book = computed(() => this.store.bookDetail());

  async ngOnInit(): Promise<void> {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      await this.store.loadBookDetail(slug);
    }
  }

  increase(): void {
    this.quantity.update((q) => q + 1);
  }

  decrease(): void {
    this.quantity.update((q) => Math.max(1, q - 1));
  }

  async addToCart(bookId: string): Promise<void> {
    await this.store.addToCart(bookId, UnitType.Sell, this.quantity());
  }

  async buyNow(bookId: string): Promise<void> {
    await this.addToCart(bookId);
    window.location.href = '/shop/checkout';
  }
}
