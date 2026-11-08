import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { OrdersApiService } from './orders-api.service';
import {
  CreateOrderRequest,
  MarkPaidRequest,
  OrderDetailResponse,
  OrderListItemResponse,
  OrderSearchRequest,
  OrderStatus
} from './orders.models';

@Injectable({ providedIn: 'root' })
export class OrdersStore {
  private readonly api = inject(OrdersApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<OrderListItemResponse[]>([]);
  readonly detail = signal<OrderDetailResponse | null>(null);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly detailLoading = signal(false);
  readonly filters = signal<OrderSearchRequest>({ page: 1, pageSize: 20 });

  readonly pendingCount = computed(() => this.items().filter((o) => o.orderStatus === OrderStatus.Pending).length);
  readonly processingCount = computed(() => this.items().filter((o) => o.orderStatus === OrderStatus.Processing).length);
  readonly completedCount = computed(() => this.items().filter((o) => o.orderStatus === OrderStatus.Completed).length);
  readonly cancelledCount = computed(() => this.items().filter((o) => o.orderStatus === OrderStatus.Cancelled).length);
  readonly revenue = computed(() => this.items().filter((o) => o.isPaid).reduce((sum, o) => sum + o.finalAmount, 0));

  async load(params?: Partial<OrderSearchRequest>): Promise<void> {
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

  async create(payload: CreateOrderRequest): Promise<OrderDetailResponse | null> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo đơn hàng thành công.');
      this.detail.set(response.data ?? null);
      await this.load();
      return response.data ?? null;
    }
    return null;
  }

  async updateStatus(id: string, orderStatus: OrderStatus, staffNote?: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.updateStatus(id, { orderStatus, staffNote }));
    if (response.isSuccess) {
      this.toast.success('Cập nhật trạng thái đơn hàng thành công.');
      this.detail.set(response.data ?? null);
      await this.load();
      return true;
    }
    return false;
  }

  async cancel(id: string, reason: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.cancel(id, reason));
    if (response.isSuccess) {
      this.toast.success('Đã hủy đơn hàng.');
      await this.loadDetail(id);
      await this.load();
      return true;
    }
    return false;
  }

  async refund(id: string, reason: string, amount: number): Promise<boolean> {
    const response = await firstValueFrom(this.api.refund(id, reason, amount));
    if (response.isSuccess) {
      this.toast.success('Đã hoàn đơn hàng.');
      await this.loadDetail(id);
      await this.load();
      return true;
    }
    return false;
  }

  async markPaid(id: string, payload: MarkPaidRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.markPaid(id, payload));
    if (response.isSuccess) {
      this.toast.success('Đã xác nhận đơn hàng đã thanh toán.');
      await this.loadDetail(id);
      await this.load();
      return true;
    }
    return false;
  }
}
