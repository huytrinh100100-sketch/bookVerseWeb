import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { PermissionsApiService } from './permissions-api.service';
import {
  CreateRoleRequest,
  PermissionResponse,
  RoleListItemResponse,
  StaffMemberResponse
} from './permissions.models';

@Injectable({ providedIn: 'root' })
export class PermissionsStore {
  private readonly api = inject(PermissionsApiService);
  private readonly toast = inject(ToastService);

  readonly permissions = signal<PermissionResponse[]>([]);
  readonly roles = signal<RoleListItemResponse[]>([]);
  readonly staff = signal<StaffMemberResponse[]>([]);
  readonly loading = signal(false);

  /** Ma quyen duoc tick theo role hien tai. */
  readonly selectedRolePermissions = signal<Record<string, string[]>>({});

  readonly permissionGroups = computed(() => {
    const groups = new Map<string, PermissionResponse[]>();
    this.permissions().forEach((p) => {
      const list = groups.get(p.groupName) ?? [];
      list.push(p);
      groups.set(p.groupName, list);
    });
    return Array.from(groups.entries()).map(([name, items]) => ({ name, items }));
  });

  async loadAll(): Promise<void> {
    this.loading.set(true);
    try {
      const [permissions, roles, staff] = await Promise.all([
        firstValueFrom(this.api.listPermissions()),
        firstValueFrom(this.api.searchRoles({ page: 1, pageSize: 100 })),
        firstValueFrom(this.api.listStaff())
      ]);
      this.permissions.set(permissions.data ?? []);
      this.roles.set(roles.data?.items ?? []);
      this.staff.set(staff.data ?? []);
      this.selectedRolePermissions.set(
        this.roles().reduce<Record<string, string[]>>((acc, role) => {
          acc[role.id] = [...role.permissionCodes];
          return acc;
        }, {})
      );
    } finally {
      this.loading.set(false);
    }
  }

  togglePermission(roleId: string, code: string, checked: boolean): void {
    this.selectedRolePermissions.update((map) => {
      const current = map[roleId] ?? [];
      const next = checked ? [...new Set([...current, code])] : current.filter((c) => c !== code);
      return { ...map, [roleId]: next };
    });
  }

  async saveRolePermissions(roleId: string): Promise<boolean> {
    const codes = this.selectedRolePermissions()[roleId] ?? [];
    if (!this.roles().some((r) => r.id === roleId)) {
      return false;
    }

    // BE khong chap nhan permissionCodes trong PUT /roles/{id};
    // phai dung POST /roles/{id}/permissions voi AssignPermissionsRequest.
    const response = await firstValueFrom(
      this.api.assignPermissions(roleId, { permissions: codes, permissionCodes: codes })
    );

    if (response.isSuccess) {
      this.toast.success('Đã lưu ma trận quyền.');
      await this.loadAll();
      return true;
    }
    return false;
  }

  async createRole(payload: CreateRoleRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.createRole(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo vai trò thành công.');
      await this.loadAll();
      return true;
    }
    return false;
  }
}
