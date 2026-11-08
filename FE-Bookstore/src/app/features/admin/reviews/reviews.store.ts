import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { ReviewsApiService } from './reviews-api.service';
import { ReviewListItemResponse, ReviewSearchRequest, ReviewStatus } from './reviews.models';

@Injectable({ providedIn: 'root' })
export class ReviewsStore {
  private readonly api = inject(ReviewsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<ReviewListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<ReviewSearchRequest>({ page: 1, pageSize: 20 });

  readonly pendingCount = computed(() => this.items().filter((r) => r.status === ReviewStatus.Pending).length);
  readonly approvedCount = computed(() => this.items().filter((r) => r.status === ReviewStatus.Approved).length);
  readonly flaggedCount = computed(() => this.items().filter((r) => r.status === ReviewStatus.Flagged).length);

  async load(params?: Partial<ReviewSearchRequest>): Promise<void> {
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

  async approve(id: string, replyContent?: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.approve(id, { replyContent }));
    if (response.isSuccess) {
      this.toast.success('Đã duyệt và xuất bản đánh giá.');
      await this.load();
      return true;
    }
    return false;
  }

  async reject(id: string, reason: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.reject(id, reason));
    if (response.isSuccess) {
      this.toast.success('Đã từ chối đánh giá.');
      await this.load();
      return true;
    }
    return false;
  }
}
