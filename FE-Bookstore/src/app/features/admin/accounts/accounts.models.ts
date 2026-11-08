import { Nullable, PagedResult } from '../../../shared/models/api.models';

export interface AccountListItemResponse {
  id: string;
  userName: string;
  fullName: string;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  isActive: boolean;
  isDeleted: boolean;
  isLocked: boolean;
  lockoutEnd?: Nullable<string>;
  roles: string[];
  createdAt: string;
}

export interface AccountDetailResponse {
  id: string;
  userName: string;
  fullName: string;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  avatarUrl?: Nullable<string>;
  isActive: boolean;
  isDeleted: boolean;
  emailConfirmed: boolean;
  lockoutEnd?: Nullable<string>;
  createdAt: string;
  updatedAt?: Nullable<string>;
  roles: string[];
}

export interface AccountSearchRequest {
  keyword?: string;
  isActive?: boolean;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateAccountRequest {
  userName: string;
  fullName: string;
  email: string;
  password: string;
  phoneNumber?: string;
  isActive: boolean;
  roles: string[];
}

export interface UpdateAccountRequest {
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  avatarUrl?: string;
  isActive?: boolean;
}

export interface AssignRolesRequest {
  roles: string[];
}

/** BE: RoleListItemResponse - rút gọn cho danh sách chọn vai trò. */
export interface RoleOption {
  id: string;
  code: string;
  name: string;
  description?: Nullable<string>;
  isSystem: boolean;
  isDefault: boolean;
  isActive: boolean;
  userCount: number;
  permissionCount: number;
}

export type AccountPagedResult = PagedResult<AccountListItemResponse>;
