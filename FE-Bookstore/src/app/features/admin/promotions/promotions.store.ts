import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { PromotionsApiService } from './promotions-api.service';
import {
  CouponListItemResponse,
  CouponSearchRequest,
  CouponStatus,
  CreateCouponRequest,
  UpdateCouponRequest
} from './promotions.models';

@Injectable({ providedIn: 'root' })
export class PromotionsStore {
  private readonly api = inject(PromotionsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<CouponListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<CouponSearchRequest>({ page: 1, pageSize: 20 });

  readonly activeCount = computed(() => this.items().filter((c) => c.status === CouponStatus.Active).length);
  readonly scheduledCount = computed(() => this.items().filter((c) => c.status === CouponStatus.Scheduled).length);
  readonly expiredCount = computed(() => this.items().filter((c) => c.status === CouponStatus.Expired).length);

  async load(params?: Partial<CouponSearchRequest>): Promise<void> {
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

  async create(payload: CreateCouponRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo voucher thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể tạo voucher.');
    return false;
  }

  async update(id: string, payload: UpdateCouponRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật voucher thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể cập nhật voucher.');
    return false;
  }

  async pause(id: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.pause(id));
    if (response.isSuccess) {
      this.toast.success('Đã tạm dừng voucher.');
      await this.load();
      return true;
    }
    return false;
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm voucher.');
      await this.load();
    }
  }
}
