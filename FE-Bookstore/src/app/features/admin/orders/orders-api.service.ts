import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  CreateOrderRequest,
  MarkPaidRequest,
  OrderDetailResponse,
  OrderListItemResponse,
  OrderPagedResult,
  OrderSearchRequest,
  OrderStatusHistoryResponse,
  UpdateOrderStatusRequest
} from './orders.models';

@Injectable({ providedIn: 'root' })
export class OrdersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: OrderSearchRequest): Observable<ApiResponse<OrderPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<OrderPagedResult>>(`${this.baseUrl}/orders`, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<OrderDetailResponse>> {
    return this.http.get<ApiResponse<OrderDetailResponse>>(`${this.baseUrl}/orders/${id}`);
  }

  create(payload: CreateOrderRequest): Observable<ApiResponse<OrderDetailResponse>> {
    return this.http.post<ApiResponse<OrderDetailResponse>>(`${this.baseUrl}/orders`, payload);
  }

  updateStatus(id: string, payload: UpdateOrderStatusRequest): Observable<ApiResponse<OrderDetailResponse>> {
    return this.http.put<ApiResponse<OrderDetailResponse>>(`${this.baseUrl}/orders/${id}/status`, payload);
  }

  cancel(id: string, reason: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/orders/${id}/cancel`, { reason });
  }

  refund(id: string, reason: string, amount: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/orders/${id}/refund`, { reason, amount });
  }

  markPaid(id: string, payload: MarkPaidRequest): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/orders/${id}/mark-paid`, payload);
  }

  history(id: string): Observable<ApiResponse<OrderStatusHistoryResponse[]>> {
    return this.http.get<ApiResponse<OrderStatusHistoryResponse[]>>(`${this.baseUrl}/orders/${id}/history`);
  }

  /** BE: GET /orders/export tra ve file CSV (FileResult), khong phai ApiResponse. */
  exportOrders(params: OrderSearchRequest): Observable<HttpResponse<Blob>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get(`${this.baseUrl}/orders/export`, {
      params: httpParams,
      observe: 'response',
      responseType: 'blob'
    });
  }
}
