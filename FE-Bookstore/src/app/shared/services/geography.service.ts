import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/core.tokens';
import { ApiResponse } from '../models/api.models';

export interface ProvinceOption {
  id: string;
  code: string;
  name: string;
  kind: string;
  isActive: boolean;
}

export interface CommuneOption {
  id: string;
  provinceId: string;
  provinceName: string;
  code: string;
  name: string;
  kind: string;
  isActive: boolean;
}

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

/**
 * Tai danh sach tinh / xa phuong cho form dia chi cua khach hang.
 *
 * Duoc dung chung cho storefront (dia chi cua khach) va admin (dia chi khach hang),
 * nen giu mot instance singleton de khong goi lai API nhieu lan.
 */
@Injectable({ providedIn: 'root' })
export class GeographyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private readonly provinces = signal<ProvinceOption[]>([]);
  private readonly communes = signal<CommuneOption[]>([]);
  private readonly loaded = signal(false);

  readonly provinceOptions = computed(() =>
    this.provinces().map((p) => ({ label: p.name, value: p.id }))
  );

  async ensureLoaded(): Promise<void> {
    if (this.loaded()) {
      return;
    }
    await this.fetchAll();
  }

  private async fetchAll(): Promise<void> {
    try {
      const provinceRes = await firstValueFrom(
        this.http.get<ApiResponse<ProvinceOption[]>>(`${this.baseUrl}/provinces/all`)
      );
      this.provinces.set(provinceRes.data ?? []);
    } catch {
      this.provinces.set([]);
    }

    try {
      const params = new HttpParams().set('pageSize', '500').set('isActive', 'true');
      const communeRes = await firstValueFrom(
        this.http.get<ApiResponse<PagedResult<CommuneOption>>>(`${this.baseUrl}/communes`, { params })
      );
      this.communes.set(communeRes.data?.items ?? []);
    } catch {
      this.communes.set([]);
    }

    this.loaded.set(true);
  }

  /** Danh sach xa/phuong thuoc tinh dang chon. */
  communeOptionsFor(provinceId: string): { label: string; value: string }[] {
    if (!provinceId) {
      return [];
    }
    return this.communes()
      .filter((c) => c.provinceId === provinceId)
      .map((c) => ({ label: c.name, value: c.id }));
  }
}
