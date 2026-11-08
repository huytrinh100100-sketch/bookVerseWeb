import { Nullable, PagedResult } from '../../../shared/models/api.models';

export interface PermissionResponse {
  id: string;
  code: string;
  name: string;
  description?: Nullable<string>;
  groupCode: string;
  groupName: string;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
}

export interface RoleListItemResponse {
  id: string;
  code: string;
  name: string;
  description?: Nullable<string>;
  isSystem: boolean;
  isDefault: boolean;
  isActive: boolean;
  isDeleted: boolean;
  userCount: number;
  permissionCount: number;
  permissionCodes: string[];
}

export interface RoleDetailResponse extends RoleListItemResponse {
  createdAt: string;
  updatedAt?: Nullable<string>;
}

export interface StaffMemberResponse {
  id: string;
  userName: string;
  email?: Nullable<string>;
  fullName: string;
  roleCodes: string[];
  twoFactorEnabled: boolean;
  lastLoginAt?: Nullable<string>;
  lastLoginIp?: Nullable<string>;
  lastLoginLocation?: Nullable<string>;
  status: 'Active' | 'Locked' | 'Inactive';
}

export interface RoleSearchRequest {
  keyword?: string;
  isActive?: boolean;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateRoleRequest {
  code: string;
  name: string;
  description?: string;
  isDefault?: boolean;
  isActive?: boolean;
  permissionCodes: string[];
}

/** BE: UpdateRoleRequest - khong chua permissionCodes (chi dung cho POST /roles/{id}/permissions). */
export interface UpdateRoleRequest {
  name?: string;
  description?: string;
  isDefault?: boolean;
  isActive?: boolean;
}

/** BE: AssignPermissionsRequest - POST /roles/{id}/permissions. */
export interface AssignPermissionsRequest {
  permissions: string[];
  permissionCodes: string[];
}

export interface AssignRoleRequest {
  userId: string;
  roleId: string;
}

export type PermissionPagedResult = PagedResult<PermissionResponse>;
export type RolePagedResult = PagedResult<RoleListItemResponse>;
