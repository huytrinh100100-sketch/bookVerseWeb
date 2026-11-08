import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  CreatePartnerRequest,
  PartnerDetailResponse,
  PartnerPagedResult,
  PartnerSearchRequest,
  UpdatePartnerRequest
} from './partners.models';

@Injectable({ providedIn: 'root' })
export class PartnersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: PartnerSearchRequest): Observable<ApiResponse<PartnerPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<PartnerPagedResult>>(`${this.baseUrl}/partners`, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<PartnerDetailResponse>> {
    return this.http.get<ApiResponse<PartnerDetailResponse>>(`${this.baseUrl}/partners/${id}`);
  }

  create(payload: CreatePartnerRequest): Observable<ApiResponse<PartnerDetailResponse>> {
    return this.http.post<ApiResponse<PartnerDetailResponse>>(`${this.baseUrl}/partners`, payload);
  }

  update(id: string, payload: UpdatePartnerRequest): Observable<ApiResponse<PartnerDetailResponse>> {
    return this.http.put<ApiResponse<PartnerDetailResponse>>(`${this.baseUrl}/partners/${id}`, payload);
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/partners/${id}`);
  }
}
