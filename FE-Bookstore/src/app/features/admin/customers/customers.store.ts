import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { CustomersApiService } from './customers-api.service';
import { CreateCustomerRequest, CustomerListItemResponse, CustomerSearchRequest, UpdateCustomerRequest, VipTier } from './customers.models';

@Injectable({ providedIn: 'root' })
export class CustomersStore {
  private readonly api = inject(CustomersApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<CustomerListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<CustomerSearchRequest>({ page: 1, pageSize: 20 });

  readonly platinumCount = computed(() => this.items().filter((c) => c.vipTier === VipTier.Platinum).length);
  readonly goldCount = computed(() => this.items().filter((c) => c.vipTier === VipTier.Gold).length);
  readonly totalLifetimeSpend = computed(() => this.items().reduce((sum, c) => sum + c.lifetimeSpend, 0));

  async load(params?: Partial<CustomerSearchRequest>): Promise<void> {
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

  async create(payload: CreateCustomerRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Thêm khách hàng thành công.');
      await this.load();
      return true;
    }
    return false;
  }

  async update(id: string, payload: UpdateCustomerRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật khách hàng thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể cập nhật khách hàng.');
    return false;
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm khách hàng.');
      await this.load();
    }
  }

  async sendVoucher(id: string, couponCode: string): Promise<boolean> {
    const response = await firstValueFrom(this.api.sendVoucher(id, couponCode));
    if (response.isSuccess) {
      this.toast.success('Đã gửi voucher cho khách hàng.');
      return true;
    }
    return false;
  }
}
