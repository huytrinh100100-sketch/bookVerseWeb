import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  ApproveReviewRequest,
  ReviewListItemResponse,
  ReviewPagedResult,
  ReviewSearchRequest
} from './reviews.models';

@Injectable({ providedIn: 'root' })
export class ReviewsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: ReviewSearchRequest): Observable<ApiResponse<ReviewPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<ReviewPagedResult>>(`${this.baseUrl}/reviews`, { params: httpParams });
  }

  approve(id: string, payload: ApproveReviewRequest): Observable<ApiResponse<ReviewListItemResponse>> {
    return this.http.put<ApiResponse<ReviewListItemResponse>>(`${this.baseUrl}/reviews/${id}/approve`, payload);
  }

  reject(id: string, reason: string): Observable<ApiResponse<ReviewListItemResponse>> {
    return this.http.put<ApiResponse<ReviewListItemResponse>>(`${this.baseUrl}/reviews/${id}/reject`, { reason });
  }

  flag(id: string, reason: string): Observable<ApiResponse<ReviewListItemResponse>> {
    return this.http.put<ApiResponse<ReviewListItemResponse>>(`${this.baseUrl}/reviews/${id}/flag`, { reason });
  }

  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/reviews/${id}`);
  }
}
