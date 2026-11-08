import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { BooksApiService } from './books-api.service';
import {
  BookCopyResponse,
  BookDetailResponse,
  BookListItemResponse,
  BookSearchRequest,
  CreateBookCopyRequest,
  CreateBookRequest,
  UpdateBookRequest
} from './books.models';

@Injectable({ providedIn: 'root' })
export class BooksStore {
  private readonly api = inject(BooksApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<BookListItemResponse[]>([]);
  readonly detail = signal<BookDetailResponse | null>(null);
  readonly copies = signal<BookCopyResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly detailLoading = signal(false);
  readonly filters = signal<BookSearchRequest>({ page: 1, pageSize: 20, isDeleted: false });

  readonly lowStockCount = computed(() => this.items().filter((b) => b.stockForSell > 0 && b.stockForSell <= 10).length);
  readonly outOfStockCount = computed(() => this.items().filter((b) => b.stockForSell === 0).length);
  readonly publishedCount = computed(() => this.items().filter((b) => b.status === 'Published').length);

  async load(params?: Partial<BookSearchRequest>): Promise<void> {
    this.loading.set(true);
    try {
      this.filters.update((current) => ({ ...current, ...params }));
      const response = await firstValueFrom(this.api.search(this.filters()));
      this.items.set(response.data?.items ?? []);
      this.totalItems.set(response.data?.totalItems ?? 0);
    } finally {
      this.loading.set(false);
    }
  }

  async loadDetail(id: string): Promise<void> {
    this.detailLoading.set(true);
    try {
      const response = await firstValueFrom(this.api.getById(id));
      this.detail.set(response.data ?? null);
    } finally {
      this.detailLoading.set(false);
    }
  }

  async create(payload: CreateBookRequest): Promise<BookDetailResponse | null> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Thêm mới sách thành công.');
      this.detail.set(response.data ?? null);
      await this.load();
      return response.data ?? null;
    }
    return null;
  }

  async update(id: string, payload: UpdateBookRequest): Promise<BookDetailResponse | null> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật sách thành công.');
      this.detail.set(response.data ?? null);
      await this.load();
      return response.data ?? null;
    }
    return null;
  }

  async toggleStatus(id: string, isActive: boolean): Promise<void> {
    const response = await firstValueFrom(this.api.updateStatus(id, isActive));
    if (response.isSuccess) {
      this.toast.success(isActive ? 'Đã hiển thị sách trên storefront.' : 'Đã ẩn sách khỏi storefront.');
      await this.load();
    }
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm sách.');
      await this.load();
    }
  }

  async restore(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.restore(id));
    if (response.isSuccess) {
      this.toast.success('Đã khôi phục sách.');
      await this.load();
    }
  }

  async loadCopies(bookId?: string): Promise<void> {
    const response = await firstValueFrom(this.api.searchCopies({ bookId, page: 1, pageSize: 50 }));
    this.copies.set(response.data?.items ?? []);
  }

  async createCopy(payload: CreateBookCopyRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.createCopy(payload));
    if (response.isSuccess) {
      this.toast.success('Thêm bản sao thành công.');
      await this.loadCopies(payload.bookId);
      return true;
    }
    return false;
  }
}
