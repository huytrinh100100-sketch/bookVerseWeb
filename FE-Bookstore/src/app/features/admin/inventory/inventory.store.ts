import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { InventoryApiService } from './inventory-api.service';
import { InventoryRowResponse, InventorySearchRequest, WarehouseResponse } from './inventory.models';

@Injectable({ providedIn: 'root' })
export class InventoryStore {
  private readonly api = inject(InventoryApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<InventoryRowResponse[]>([]);
  readonly warehouses = signal<WarehouseResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<InventorySearchRequest>({ page: 1, pageSize: 20 });

  readonly criticalCount = computed(() => this.items().filter((r) => r.stockForSell > 0 && r.stockForSell < 5).length);
  readonly lowCount = computed(() => this.items().filter((r) => r.stockForSell >= 5 && r.stockForSell < 10).length);
  readonly outOfStockCount = computed(() => this.items().filter((r) => r.stockForSell === 0).length);
  readonly totalStockValue = computed(() => this.items().reduce((sum, r) => sum + r.totalQuantity, 0));

  async load(params?: Partial<InventorySearchRequest>): Promise<void> {
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

  async loadWarehouses(): Promise<void> {
    const response = await firstValueFrom(this.api.getWarehouses());
    this.warehouses.set(response.data ?? []);
  }

  async restockAll(): Promise<boolean> {
    const response = await firstValueFrom(this.api.restockAll());
    if (response.isSuccess) {
      this.toast.success('Đã tạo phiếu nhập cho các mục thấp.');
      await this.load();
      return true;
    }
    return false;
  }
}
