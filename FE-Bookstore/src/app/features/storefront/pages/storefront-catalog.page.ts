import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UnitType } from '../../admin/books/books.models';
import { StorefrontStore } from '../storefront.store';
import { CatalogQuery } from '../storefront.models';

@Component({
  selector: 'app-shop-catalog',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="catalog">
      <aside class="catalog__filters">
        <h3>Danh mục</h3>
        @for (facet of facets().categories; track facet.categoryId) {
          <button type="button" class="filter" [class.filter--active]="query().categoryId === facet.categoryId" (click)="pickCategory(facet.categoryId)">
            <span>{{ facet.categoryName }}</span>
            <span class="filter__count">{{ facet.count }}</span>
          </button>
        }
        <h3>Nhà xuất bản</h3>
        @for (facet of facets().publishers; track facet.categoryId) {
          <button type="button" class="filter" [class.filter--active]="query().publisherId === facet.categoryId" (click)="pickPublisher(facet.categoryId)">
            <span>{{ facet.categoryName }}</span>
            <span class="filter__count">{{ facet.count }}</span>
          </button>
        }
        <h3>Khoảng giá</h3>
        @for (range of facets().priceRanges; track range.label) {
          <button type="button" class="filter" [class.filter--active]="query().minPrice === range.min && query().maxPrice === range.max" (click)="pickPriceRange(range.min, range.max)">
            <span>{{ range.label }}</span>
            <span class="filter__count">{{ range.count }}</span>
          </button>
        }
        <h3>Đánh giá</h3>
        <button type="button" class="filter" [class.filter--active]="query().minRating === 4.5" (click)="pickRating(4.5)"><i class="pi pi-star-fill"></i> 4.5 &amp; trở lên</button>
        <button type="button" class="filter" [class.filter--active]="query().minRating === 4" (click)="pickRating(4)"><i class="pi pi-star-fill"></i> 4.0 &amp; trở lên</button>
      </aside>

      <section class="catalog__main">
        <header class="catalog__head">
          <h1>{{ activeCategoryName() || 'Danh mục sách' }}</h1>
          <select [value]="query().sortBy" (change)="onSort($event)">
            <option value="curator">Đề xuất của người duyệt</option>
            <option value="price-asc">Giá thấp đến cao</option>
            <option value="price-desc">Giá cao đến thấp</option>
            <option value="bestseller">Bán chạy nhất</option>
            <option value="newest">Mới đến</option>
          </select>
        </header>

        <div class="grid">
          @for (book of items(); track book.id) {
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
              <button type="button" class="btn btn--primary" (click)="addToCart(book.id)">
                <i class="pi pi-shopping-bag"></i> Thêm vào giỏ
              </button>
            </article>
          } @empty {
            <div class="catalog-empty">
              <i class="pi pi-search"></i>
              <h3>Không tìm thấy sách phù hợp</h3>
              <p>Thử bỏ bớt bộ lọc hoặc tìm kiếm với từ khoá khác.</p>
              <button type="button" class="btn btn--ghost" (click)="resetFilters()"><i class="pi pi-filter-slash"></i> Xóa bộ lọc</button>
            </div>
          }
        </div>
      </section>
    </div>
  `,
  styles: [
    `
      .catalog { display: grid; grid-template-columns: 16rem 1fr; gap: 2rem; }
      .catalog__filters h3 { font-size: 0.8125rem; text-transform: uppercase; letter-spacing: 0.03em; margin: 1.25rem 0 0.5rem; }
      .filter { display: flex; justify-content: space-between; width: 100%; border: none; background: transparent; padding: 0.375rem 0.5rem; border-radius: 0.375rem; cursor: pointer; font-size: 0.875rem; text-align: left; }
      .filter--active { background: var(--secondary-container, #6cf8bb); font-weight: 700; }
      .filter__count { font-size: 0.75rem; color: var(--on-surface-variant, #45464d); }
      .catalog__head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.25rem; }
      .catalog__head h1 { margin: 0; font-size: 1.75rem; }
      .catalog__head select { padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: none; background: var(--surface-container-low, #f2f4f6); }
      .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(12rem, 1fr)); gap: 1.25rem; }
      .book-card { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); display: flex; flex-direction: column; gap: 0.375rem; }
      .book-card__cover { display: block; aspect-ratio: 2/3; background: var(--surface-container-low, #f2f4f6); border-radius: 0.5rem; overflow: hidden; }
      .book-card__cover img { width: 100%; height: 100%; object-fit: cover; }
      .book-card__placeholder { display: grid; place-items: center; height: 100%; font-size: 2rem; color: var(--outline, #76777d); }
      .book-card__title { font-size: 0.875rem; margin: 0.25rem 0 0; font-weight: 600; }
      .book-card__author { margin: 0; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .book-card__price { margin: 0.25rem 0 0.5rem; font-weight: 700; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.75rem; cursor: pointer; font-size: 0.8125rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class StorefrontCatalogPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  readonly query = signal<CatalogQuery>({ page: 1, pageSize: 24, sortBy: 'curator' });

  readonly items = computed(() => this.store.catalog()?.items ?? []);
  readonly facets = computed(() => this.store.catalog()?.facets ?? { categories: [], publishers: [], languages: [], formats: [], priceRanges: [] });
  readonly activeCategoryName = computed(() =>
    this.facets().categories.find((c) => c.categoryId === this.query().categoryId)?.categoryName ?? ''
  );

  async ngOnInit(): Promise<void> {
    if (!this.store.catalog()) {
      await this.store.searchCatalog(this.query());
    }
  }

  async resetFilters(): Promise<void> {
    this.query.set({ page: 1, pageSize: 24, sortBy: 'curator' });
    await this.store.searchCatalog(this.query());
  }

  async pickCategory(categoryId: string): Promise<void> {
    this.query.update((q) => ({ ...q, categoryId: q.categoryId === categoryId ? undefined : categoryId, page: 1 }));
    await this.store.searchCatalog(this.query());
  }

  async pickPublisher(publisherId: string): Promise<void> {
    this.query.update((q) => ({ ...q, publisherId: q.publisherId === publisherId ? undefined : publisherId, page: 1 }));
    await this.store.searchCatalog(this.query());
  }

  async pickPriceRange(min: number, max: number): Promise<void> {
    const isActive = this.query().minPrice === min && this.query().maxPrice === max;
    this.query.update((q) => ({ ...q, minPrice: isActive ? undefined : min, maxPrice: isActive ? undefined : max, page: 1 }));
    await this.store.searchCatalog(this.query());
  }

  async pickRating(minRating: number): Promise<void> {
    this.query.update((q) => ({ ...q, minRating: q.minRating === minRating ? undefined : minRating, page: 1 }));
    await this.store.searchCatalog(this.query());
  }

  async onSort(event: Event): Promise<void> {
    const sortBy = (event.target as HTMLSelectElement).value as CatalogQuery['sortBy'];
    this.query.update((q) => ({ ...q, sortBy }));
    await this.store.searchCatalog(this.query());
  }

  async addToCart(bookId: string): Promise<void> {
    await this.store.addToCart(bookId, UnitType.Sell, 1);
  }
}
