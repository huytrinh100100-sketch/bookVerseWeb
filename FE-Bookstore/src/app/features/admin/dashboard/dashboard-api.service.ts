import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import { DashboardOverviewResponse } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getOverview(): Observable<ApiResponse<DashboardOverviewResponse>> {
    return this.http.get<ApiResponse<DashboardOverviewResponse>>(`${this.baseUrl}/dashboard/overview`);
  }
}
