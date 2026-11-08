import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/core.tokens';
import { ApiResponse } from '../../shared/models/api.models';

/** BE: UpdateProfileRequest. */
export interface UpdateProfilePayload {
  fullName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  address?: string;
  provinceId?: string | null;
  communeId?: string | null;
  birthday?: string | null;
}

/** BE: ChangePasswordRequest. */
export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

/** BE: ProfileResponse. */
export interface ProfileResponse {
  id: string;
  userName: string;
  fullName: string;
  email?: string | null;
  phoneNumber?: string | null;
  avatarUrl?: string | null;
  address?: string | null;
  provinceId?: string | null;
  provinceName?: string | null;
  communeId?: string | null;
  communeName?: string | null;
  birthday?: string | null;
  vipTier: string;
  rewardPoints: number;
  lifetimeSpend: number;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
  roles: string[];
}

/** API cho ho so nguoi dung (dung chung storefront + admin). */
@Injectable({ providedIn: 'root' })
export class ProfileApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get(): Observable<ApiResponse<ProfileResponse>> {
    return this.http.get<ApiResponse<ProfileResponse>>(`${this.baseUrl}/profile`);
  }

  update(payload: UpdateProfilePayload): Observable<ApiResponse<ProfileResponse>> {
    return this.http.put<ApiResponse<ProfileResponse>>(`${this.baseUrl}/profile`, payload);
  }

  changePassword(payload: ChangePasswordPayload): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/profile/change-password`, payload);
  }
}
