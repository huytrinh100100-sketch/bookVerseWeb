import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  CreateCustomerRequest,
  CustomerDetailResponse,
  CustomerPagedResult,
  CustomerSearchRequest,
  ExportPiiResponse,
  UpdateCustomerRequest
} from './customers.models';

@Injectable({ providedIn: 'root' })
export class CustomersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: CustomerSearchRequest): Observable<ApiResponse<CustomerPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<CustomerPagedResult>>(`${this.baseUrl}/customers`, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<CustomerDetailResponse>> {
    return this.http.get<ApiResponse<CustomerDetailResponse>>(`${this.baseUrl}/customers/${id}`);
  }

  create(payload: CreateCustomerRequest): Observable<ApiResponse<CustomerDetailResponse>> {
    return this.http.post<ApiResponse<CustomerDetailResponse>>(`${this.baseUrl}/customers`, payload);
  }

  update(id: string, payload: UpdateCustomerRequest): Observable<ApiResponse<CustomerDetailResponse>> {
    return this.http.put<ApiResponse<CustomerDetailResponse>>(`${this.baseUrl}/customers/${id}`, payload);
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/customers/${id}`);
  }

  exportPii(ids: string[]): Observable<ApiResponse<ExportPiiResponse>> {
    return this.http.post<ApiResponse<ExportPiiResponse>>(`${this.baseUrl}/customers/export-pii`, { ids });
  }

  sendVoucher(id: string, couponCode: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/customers/${id}/voucher`, { couponCode });
  }
}
