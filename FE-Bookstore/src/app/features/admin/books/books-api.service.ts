import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  BookCopyPagedResult,
  BookCopyResponse,
  BookCopySearchRequest,
  BookDetailResponse,
  BookListItemResponse,
  BookPagedResult,
  BookSearchRequest,
  BookStockAdjustRequest,
  CreateBookCopyRequest,
  CreateBookRequest,
  UpdateBookRequest
} from './books.models';

@Injectable({ providedIn: 'root' })
export class BooksApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(params: BookSearchRequest): Observable<ApiResponse<BookPagedResult>> {
    return this.http.get<ApiResponse<BookPagedResult>>(`${this.baseUrl}/books`, {
      params: this.toParams(params)
    });
  }

  getById(id: string): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.get<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books/${id}`);
  }

  getBySlug(slug: string): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.get<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books/slug/${slug}`);
  }

  create(payload: CreateBookRequest): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.post<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books`, payload);
  }

  update(id: string, payload: UpdateBookRequest): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.put<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books/${id}`, payload);
  }

  updateStatus(id: string, isActive: boolean): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.put<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books/${id}/status`, { isActive });
  }

  adjustStock(id: string, payload: BookStockAdjustRequest): Observable<ApiResponse<BookDetailResponse>> {
    return this.http.put<ApiResponse<BookDetailResponse>>(`${this.baseUrl}/books/${id}/stock`, payload);
  }

  softDelete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/books/${id}`);
  }

  restore(id: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/books/${id}/restore`, {});
  }

  // ---- Ban sao vat ly
  searchCopies(params: BookCopySearchRequest): Observable<ApiResponse<BookCopyPagedResult>> {
    return this.http.get<ApiResponse<BookCopyPagedResult>>(`${this.baseUrl}/book-copies`, {
      params: this.toParams(params)
    });
  }

  createCopy(payload: CreateBookCopyRequest): Observable<ApiResponse<BookCopyResponse>> {
    return this.http.post<ApiResponse<BookCopyResponse>>(`${this.baseUrl}/book-copies`, payload);
  }

  updateCopy(id: string, payload: Partial<CreateBookCopyRequest>): Observable<ApiResponse<BookCopyResponse>> {
    return this.http.put<ApiResponse<BookCopyResponse>>(`${this.baseUrl}/book-copies/${id}`, payload);
  }

  deleteCopy(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/book-copies/${id}`);
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
