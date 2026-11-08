import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  CategoryDetailResponse,
  CategoryListItemResponse,
  CategoryPagedResult,
  CategoryTreeNodeResponse,
  CreateCategoryRequest,
  MoveCategoryRequest,
  UpdateCategoryRequest
} from './categories.models';

@Injectable({ providedIn: 'root' })
export class CategoriesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: Record<string, unknown>): Observable<ApiResponse<CategoryPagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<CategoryPagedResult>>(`${this.baseUrl}/categories`, { params: httpParams });
  }

  getTree(rootCode?: string): Observable<ApiResponse<CategoryTreeNodeResponse[]>> {
    const params = rootCode ? new HttpParams().set('rootCode', rootCode) : new HttpParams();
    return this.http.get<ApiResponse<CategoryTreeNodeResponse[]>>(`${this.baseUrl}/categories/tree`, { params });
  }

  getById(id: string): Observable<ApiResponse<CategoryDetailResponse>> {
    return this.http.get<ApiResponse<CategoryDetailResponse>>(`${this.baseUrl}/categories/${id}`);
  }

  create(payload: CreateCategoryRequest): Observable<ApiResponse<CategoryDetailResponse>> {
    return this.http.post<ApiResponse<CategoryDetailResponse>>(`${this.baseUrl}/categories`, payload);
  }

  update(id: string, payload: UpdateCategoryRequest): Observable<ApiResponse<CategoryDetailResponse>> {
    return this.http.put<ApiResponse<CategoryDetailResponse>>(`${this.baseUrl}/categories/${id}`, payload);
  }

  move(id: string, payload: MoveCategoryRequest): Observable<ApiResponse<CategoryDetailResponse>> {
    return this.http.put<ApiResponse<CategoryDetailResponse>>(`${this.baseUrl}/categories/${id}/move`, payload);
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/categories/${id}`);
  }

  restore(id: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/categories/${id}/restore`, {});
  }
}
