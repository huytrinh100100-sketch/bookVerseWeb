import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  CouponDetailResponse,
  CouponListItemResponse,
  CouponPagedResult,
  CouponSearchRequest,
  CreateCouponRequest,
  UpdateCouponRequest,
  ValidateCouponRequest,
  ValidateCouponResponse
} from './promotions.models';

@Injectable({ providedIn: 'root' })
export class PromotionsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: CouponSearchRequest): Observable<ApiResponse<CouponPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<CouponPagedResult>>(`${this.baseUrl}/coupons`, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<CouponDetailResponse>> {
    return this.http.get<ApiResponse<CouponDetailResponse>>(`${this.baseUrl}/coupons/${id}`);
  }

  /** BE: GET /coupons/validate/{code} -> CouponDetailResponse. */
  validateByCode(code: string): Observable<ApiResponse<CouponDetailResponse>> {
    return this.http.get<ApiResponse<CouponDetailResponse>>(`${this.baseUrl}/coupons/validate/${code}`);
  }

  /** BE: POST /coupons/validate -> ValidateCouponResponse. */
  validate(payload: ValidateCouponRequest): Observable<ApiResponse<ValidateCouponResponse>> {
    return this.http.post<ApiResponse<ValidateCouponResponse>>(`${this.baseUrl}/coupons/validate`, payload);
  }

  create(payload: CreateCouponRequest): Observable<ApiResponse<CouponDetailResponse>> {
    return this.http.post<ApiResponse<CouponDetailResponse>>(`${this.baseUrl}/coupons`, payload);
  }

  update(id: string, payload: UpdateCouponRequest): Observable<ApiResponse<CouponDetailResponse>> {
    return this.http.put<ApiResponse<CouponDetailResponse>>(`${this.baseUrl}/coupons/${id}`, payload);
  }

  pause(id: string): Observable<ApiResponse<CouponDetailResponse>> {
    return this.http.put<ApiResponse<CouponDetailResponse>>(`${this.baseUrl}/coupons/${id}/pause`, {});
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/coupons/${id}`);
  }
}
