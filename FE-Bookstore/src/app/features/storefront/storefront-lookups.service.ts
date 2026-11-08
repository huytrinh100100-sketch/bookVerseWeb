import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/core.tokens';
import { ApiResponse } from '../../shared/models/api.models';

/** BE: SettingService.GetCarriersAsync -> CarrierResponse. */
export interface CarrierOption {
  id: string;
  code: string;
  name: string;
  price: number;
  estimatedDays: number;
  isEnabled: boolean;
}

/** BE: SettingService.GetPaymentGatewaysAsync -> PaymentGatewayResponse. */
export interface PaymentGatewayOption {
  id: string;
  code: string;
  name: string;
  provider: string;
  isEnabled: boolean;
  isTestMode: boolean;
  supportedMethods: string[];
}

/**
 * Tai danh sach don vi van chuyen + cong thanh toan cho trang checkout storefront.
 *
 * Cac endpoint nay thuoc quyen `setting.view` (chi admin/seller), nen storefront
 * tu fallback ve danh sach mac dinh khi goi that bai (khong chan luong mua hang).
 */
@Injectable({ providedIn: 'root' })
export class StorefrontLookupsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  readonly carriers = signal<CarrierOption[]>([]);
  readonly gateways = signal<PaymentGatewayOption[]>([]);

  private loaded = false;

  async ensureLoaded(): Promise<void> {
    if (this.loaded) {
      return;
    }
    this.loaded = true;

    const [carriers, gateways] = await Promise.all([
      this.fetchCarriers(),
      this.fetchGateways()
    ]);

    this.carriers.set(carriers);
    this.gateways.set(gateways);
  }

  private async fetchCarriers(): Promise<CarrierOption[]> {
    try {
      const response = await firstValueFrom(
        this.http.get<ApiResponse<CarrierOption[]>>(`${this.baseUrl}/settings/carriers`)
      );
      return (response.data ?? []).filter((x) => x.isEnabled);
    } catch {
      return [];
    }
  }

  private async fetchGateways(): Promise<PaymentGatewayOption[]> {
    try {
      const response = await firstValueFrom(
        this.http.get<ApiResponse<PaymentGatewayOption[]>>(`${this.baseUrl}/settings/payment-gateways`)
      );
      return (response.data ?? []).filter((x) => x.isEnabled);
    } catch {
      return [];
    }
  }
}
