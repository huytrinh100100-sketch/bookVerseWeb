import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UnitType } from '../../admin/books/books.models';
import { StorefrontStore } from '../storefront.store';

/** Trang "Tủ sách & Yêu thích" cua khach hang. */
@Component({
  selector: 'app-shop-wishlist',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      <header class="page__head">
        <div>
          <h1>Tủ sách & Yêu thích</h1>
          <p>{{ items().length }} cuốn sách bạn đã lưu lại.</p>
        </div>
      </header>

      <div class="grid">
        @for (book of items(); track book.id) {
          <article class="book-card">
            <a class="book-card__cover" [routerLink]="['/shop/books', book.bookSlug]">
              @if (book.coverImageUrl) {
                <img [src]="book.coverImageUrl" [alt]="book.bookTitle" />
              } @else {
                <span class="book-card__placeholder"><i class="pi pi-book"></i></span>
              }
            </a>
            <h3 class="book-card__title">{{ book.bookTitle }}</h3>
            <p class="book-card__price">{{ book.priceForSell | number }} d</p>
            <div class="book-card__actions">
              <button type="button" class="btn btn--primary" (click)="addToCart(book.bookId)">
                <i class="pi pi-shopping-bag"></i> Thêm vào giỏ
              </button>
              <button type="button" class="btn btn--ghost" title="Bỏ yêu thích" (click)="remove(book.bookId)">
                <i class="pi pi-trash"></i>
              </button>
            </div>
          </article>
        } @empty {
          <div class="empty">
            <i class="pi pi-bookmark"></i>
            <h3>Chưa có sách yêu thích</h3>
            <p>Nhấn biểu tượng bookmark trên mỗi cuốn sách để lưu lại tại đây.</p>
            <a class="btn btn--primary" routerLink="/shop/catalog"><i class="pi pi-arrow-left"></i> Xem danh mục</a>
          </div>
        }
      </div>

      <a class="back-link" routerLink="/shop/account"><i class="pi pi-arrow-left"></i> Quay lại tài khoản</a>
    </div>
  `,
  styles: [
    `
      .page { max-width: 68rem; margin: 0 auto; }
      .page__head { margin-bottom: 1.25rem; }
      .page__head h1 { margin: 0 0 0.25rem; font-size: 1.5rem; }
      .page__head p { margin: 0; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(12rem, 1fr)); gap: 1.25rem; }
      .book-card { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); display: flex; flex-direction: column; gap: 0.375rem; }
      .book-card__cover { display: block; aspect-ratio: 2/3; background: var(--surface-container-low, #f2f4f6); border-radius: 0.5rem; overflow: hidden; }
      .book-card__cover img { width: 100%; height: 100%; object-fit: cover; }
      .book-card__placeholder { display: grid; place-items: center; height: 100%; font-size: 2rem; color: var(--outline, #76777d); }
      .book-card__title { font-size: 0.875rem; margin: 0.25rem 0 0; font-weight: 600; }
      .book-card__price { margin: 0.25rem 0 0.5rem; font-weight: 700; }
      .book-card__actions { display: flex; gap: 0.375rem; margin-top: auto; }
      .empty { grid-column: 1 / -1; text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .empty p { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .back-link { display: inline-flex; align-items: center; gap: 0.375rem; margin-top: 1.25rem; color: var(--secondary, #006c49); font-size: 0.875rem; font-weight: 600; text-decoration: none; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.75rem; cursor: pointer; font-size: 0.8125rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class StorefrontWishlistPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  readonly loading = signal(false);

  readonly items = computed(() => this.store.wishlist());

  async ngOnInit(): Promise<void> {
    await this.store.loadWishlist();
  }

  async addToCart(bookId: string): Promise<void> {
    await this.store.addToCart(bookId, UnitType.Sell, 1);
  }

  async remove(bookId: string): Promise<void> {
    await this.store.toggleWishlist(bookId);
  }
}
