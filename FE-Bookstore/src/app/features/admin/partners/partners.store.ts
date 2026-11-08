import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { PartnersApiService } from './partners-api.service';
import { CreatePartnerRequest, PartnerListItemResponse, PartnerSearchRequest, PartnerType, UpdatePartnerRequest } from './partners.models';

@Injectable({ providedIn: 'root' })
export class PartnersStore {
  private readonly api = inject(PartnersApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<PartnerListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<PartnerSearchRequest>({ page: 1, pageSize: 20 });

  readonly authorCount = computed(() => this.items().filter((p) => p.type === PartnerType.Author).length);
  readonly publisherCount = computed(() => this.items().filter((p) => p.type === PartnerType.Publisher).length);

  async load(params?: Partial<PartnerSearchRequest>): Promise<void> {
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

  async create(payload: CreatePartnerRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Thêm tác giả / NXB thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể tạo đối tác.');
    return false;
  }

  async update(id: string, payload: UpdatePartnerRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật đối tác thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể cập nhật đối tác.');
    return false;
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm tác giả / NXB.');
      await this.load();
    }
  }
}
