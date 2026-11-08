import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { SettingsApiService } from './settings-api.service';
import { CarrierResponse, PaymentGatewayResponse, StoreSettingResponse, UpdateStoreSettingRequest } from './settings.models';

@Injectable({ providedIn: 'root' })
export class SettingsStore {
  private readonly api = inject(SettingsApiService);
  private readonly toast = inject(ToastService);

  readonly store = signal<StoreSettingResponse | null>(null);
  readonly paymentGateways = signal<PaymentGatewayResponse[]>([]);
  readonly carriers = signal<CarrierResponse[]>([]);
  readonly loading = signal(false);

  async loadAll(): Promise<void> {
    this.loading.set(true);
    try {
      const [store, gateways, carriers] = await Promise.all([
        firstValueFrom(this.api.getStore()),
        firstValueFrom(this.api.getPaymentGateways()),
        firstValueFrom(this.api.getCarriers())
      ]);
      this.store.set(store.data ?? null);
      this.paymentGateways.set(gateways.data ?? []);
      this.carriers.set(carriers.data ?? []);
    } finally {
      this.loading.set(false);
    }
  }

  async save(payload: UpdateStoreSettingRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.updateStore(payload));
    if (response.isSuccess) {
      this.toast.success('Lưu cấu hình thành công.');
      this.store.set(response.data ?? null);
      return true;
    }
    return false;
  }
}
