import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  ArticleDetailResponse,
  ArticleListItemResponse,
  ArticlePagedResult,
  ArticleSearchRequest,
  CreateArticleRequest,
  UpdateArticleRequest
} from './articles.models';

@Injectable({ providedIn: 'root' })
export class ArticlesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: ArticleSearchRequest): Observable<ApiResponse<ArticlePagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<ArticlePagedResult>>(`${this.baseUrl}/articles`, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<ArticleDetailResponse>> {
    return this.http.get<ApiResponse<ArticleDetailResponse>>(`${this.baseUrl}/articles/${id}`);
  }

  create(payload: CreateArticleRequest): Observable<ApiResponse<ArticleDetailResponse>> {
    return this.http.post<ApiResponse<ArticleDetailResponse>>(`${this.baseUrl}/articles`, payload);
  }

  update(id: string, payload: UpdateArticleRequest): Observable<ApiResponse<ArticleDetailResponse>> {
    return this.http.put<ApiResponse<ArticleDetailResponse>>(`${this.baseUrl}/articles/${id}`, payload);
  }

  publish(id: string): Observable<ApiResponse<ArticleDetailResponse>> {
    return this.http.put<ApiResponse<ArticleDetailResponse>>(`${this.baseUrl}/articles/${id}/publish`, {});
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/articles/${id}`);
  }
}
