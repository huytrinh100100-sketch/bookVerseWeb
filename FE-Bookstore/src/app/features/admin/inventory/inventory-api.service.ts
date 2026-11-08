import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  AuditSheetResponse,
  DraftPurchaseOrderRequest,
  DraftPurchaseOrderResponse,
  InventoryPagedResult,
  InventoryRowResponse,
  InventorySearchRequest,
  WarehouseResponse
} from './inventory.models';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: InventorySearchRequest): Observable<ApiResponse<InventoryPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<InventoryPagedResult>>(`${this.baseUrl}/inventory`, { params: httpParams });
  }

  getWarehouses(): Observable<ApiResponse<WarehouseResponse[]>> {
    return this.http.get<ApiResponse<WarehouseResponse[]>>(`${this.baseUrl}/warehouses`);
  }

  draftPurchaseOrder(payload: DraftPurchaseOrderRequest): Observable<ApiResponse<DraftPurchaseOrderResponse>> {
    return this.http.post<ApiResponse<DraftPurchaseOrderResponse>>(`${this.baseUrl}/inventory/purchase-orders/draft`, payload);
  }

  restockAll(): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/inventory/restock-all`, {});
  }

  exportAuditSheet(): Observable<ApiResponse<AuditSheetResponse>> {
    return this.http.get<ApiResponse<AuditSheetResponse>>(`${this.baseUrl}/inventory/audit-sheet`);
  }
}
