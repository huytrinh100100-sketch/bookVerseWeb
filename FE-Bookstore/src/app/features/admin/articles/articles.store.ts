import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { ArticlesApiService } from './articles-api.service';
import { ArticleListItemResponse, ArticleSearchRequest, CreateArticleRequest, UpdateArticleRequest } from './articles.models';

@Injectable({ providedIn: 'root' })
export class ArticlesStore {
  private readonly api = inject(ArticlesApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<ArticleListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<ArticleSearchRequest>({ page: 1, pageSize: 20 });

  async load(params?: Partial<ArticleSearchRequest>): Promise<void> {
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

  async create(payload: CreateArticleRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo bài viết thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể tạo bài viết.');
    return false;
  }

  async update(id: string, payload: UpdateArticleRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật bài viết thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể cập nhật bài viết.');
    return false;
  }

  async publish(id: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.publish(id));
    if (response.isSuccess) {
      this.toast.success('Đã xuất bản bài viết.');
      await this.load();
      return true;
    }
    return false;
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm bài viết.');
      await this.load();
    }
  }
}
