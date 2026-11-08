import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import { CarrierResponse, PaymentGatewayResponse, StoreSettingResponse, UpdateStoreSettingRequest } from './settings.models';

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getStore(): Observable<ApiResponse<StoreSettingResponse>> {
    return this.http.get<ApiResponse<StoreSettingResponse>>(`${this.baseUrl}/settings/store`);
  }

  updateStore(payload: UpdateStoreSettingRequest): Observable<ApiResponse<StoreSettingResponse>> {
    return this.http.put<ApiResponse<StoreSettingResponse>>(`${this.baseUrl}/settings/store`, payload);
  }

  getPaymentGateways(): Observable<ApiResponse<PaymentGatewayResponse[]>> {
    return this.http.get<ApiResponse<PaymentGatewayResponse[]>>(`${this.baseUrl}/settings/payment-gateways`);
  }

  getCarriers(): Observable<ApiResponse<CarrierResponse[]>> {
    return this.http.get<ApiResponse<CarrierResponse[]>>(`${this.baseUrl}/settings/carriers`);
  }
}
