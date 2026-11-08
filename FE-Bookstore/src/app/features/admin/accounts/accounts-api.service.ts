import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  AccountDetailResponse,
  AccountListItemResponse,
  AccountPagedResult,
  AccountSearchRequest,
  AssignRolesRequest,
  CreateAccountRequest,
  UpdateAccountRequest
} from './accounts.models';

@Injectable({ providedIn: 'root' })
export class AccountsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: AccountSearchRequest): Observable<ApiResponse<AccountPagedResult>> {
    return this.http.get<ApiResponse<AccountPagedResult>>(`${this.baseUrl}/accounts`, {
      params: this.toParams(params)
    });
  }

  getById(id: string): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.get<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts/${id}`);
  }

  create(payload: CreateAccountRequest): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.post<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts`, payload);
  }

  update(id: string, payload: UpdateAccountRequest): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.put<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts/${id}`, payload);
  }

  assignRoles(id: string, payload: AssignRolesRequest): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.post<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts/${id}/roles`, payload);
  }

  lock(id: string): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.post<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts/${id}/lock`, {});
  }

  unlock(id: string): Observable<ApiResponse<AccountDetailResponse>> {
    return this.http.post<ApiResponse<AccountDetailResponse>>(`${this.baseUrl}/accounts/${id}/unlock`, {});
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/accounts/${id}`);
  }

  restore(id: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/accounts/${id}/restore`, {});
  }

  hardDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/accounts/${id}/permanent`);
  }

  private toParams(params: object): HttpParams {
    let httpParams = new HttpParams();
    Object.entries(params as Record<string, unknown>).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return httpParams;
  }
}
