import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/core.tokens';
import { ApiResponse } from '../../../shared/models/api.models';
import {
  AssignRoleRequest,
  AssignPermissionsRequest,
  CreateRoleRequest,
  PermissionPagedResult,
  PermissionResponse,
  RoleDetailResponse,
  RolePagedResult,
  RoleSearchRequest,
  StaffMemberResponse,
  UpdateRoleRequest
} from './permissions.models';

@Injectable({ providedIn: 'root' })
export class PermissionsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listPermissions(): Observable<ApiResponse<PermissionResponse[]>> {
    return this.http.get<ApiResponse<PermissionResponse[]>>(`${this.baseUrl}/permissions`);
  }

  searchRoles(params: RoleSearchRequest): Observable<ApiResponse<RolePagedResult>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<RolePagedResult>>(`${this.baseUrl}/roles`, { params: httpParams });
  }

  getRole(id: string): Observable<ApiResponse<RoleDetailResponse>> {
    return this.http.get<ApiResponse<RoleDetailResponse>>(`${this.baseUrl}/roles/${id}`);
  }

  createRole(payload: CreateRoleRequest): Observable<ApiResponse<RoleDetailResponse>> {
    return this.http.post<ApiResponse<RoleDetailResponse>>(`${this.baseUrl}/roles`, payload);
  }

  updateRole(id: string, payload: UpdateRoleRequest): Observable<ApiResponse<RoleDetailResponse>> {
    return this.http.put<ApiResponse<RoleDetailResponse>>(`${this.baseUrl}/roles/${id}`, payload);
  }

  /** BE: POST /roles/{id}/permissions - gan ma quyen cho vai tro. */
  assignPermissions(id: string, payload: AssignPermissionsRequest): Observable<ApiResponse<RoleDetailResponse>> {
    return this.http.post<ApiResponse<RoleDetailResponse>>(`${this.baseUrl}/roles/${id}/permissions`, payload);
  }

  cloneRole(id: string): Observable<ApiResponse<RoleDetailResponse>> {
    return this.http.post<ApiResponse<RoleDetailResponse>>(`${this.baseUrl}/roles/${id}/clone`, {});
  }

  assignRole(payload: AssignRoleRequest): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/roles/assign`, payload);
  }

  listStaff(): Observable<ApiResponse<StaffMemberResponse[]>> {
    return this.http.get<ApiResponse<StaffMemberResponse[]>>(`${this.baseUrl}/users/staff`);
  }
}
