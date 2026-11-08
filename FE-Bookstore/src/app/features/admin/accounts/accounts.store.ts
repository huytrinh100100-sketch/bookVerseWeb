import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { PermissionsApiService } from '../permissions/permissions-api.service';
import { RoleListItemResponse } from '../permissions/permissions.models';
import { AccountsApiService } from './accounts-api.service';
import {
  AccountListItemResponse,
  AccountSearchRequest,
  AssignRolesRequest,
  CreateAccountRequest,
  UpdateAccountRequest
} from './accounts.models';

@Injectable({ providedIn: 'root' })
export class AccountsStore {
  private readonly api = inject(AccountsApiService);
  private readonly roleApi = inject(PermissionsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<AccountListItemResponse[]>([]);
  readonly roles = signal<RoleListItemResponse[]>([]);
  readonly totalItems = signal(0);
  readonly loading = signal(false);
  readonly filters = signal<AccountSearchRequest>({ page: 1, pageSize: 20, isDeleted: false });

  readonly activeCount = computed(() => this.items().filter((a) => a.isActive && !a.isDeleted).length);
  readonly lockedCount = computed(() => this.items().filter((a) => a.isLocked).length);
  readonly deletedCount = computed(() => this.items().filter((a) => a.isDeleted).length);

  async load(params?: Partial<AccountSearchRequest>): Promise<void> {
    this.loading.set(true);
    try {
      this.filters.update((current) => ({ ...current, ...params }));
      const response = await firstValueFrom(this.api.search(this.filters()));
      this.items.set(response.data?.items ?? []);
      this.totalItems.set(response.data?.totalItems ?? 0);
    } finally {
      this.loading.set(false);
    }
  }

  /** Tải danh sách vai trò dùng cho dropdown gán quyền. */
  async loadRoles(): Promise<void> {
    const response = await firstValueFrom(this.roleApi.searchRoles({ page: 1, pageSize: 100 }));
    this.roles.set(response.data?.items ?? []);
  }

  async create(payload: CreateAccountRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo tài khoản thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể tạo tài khoản.');
    return false;
  }

  async update(id: string, payload: UpdateAccountRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật tài khoản thành công.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể cập nhật tài khoản.');
    return false;
  }

  async assignRoles(id: string, roles: string[]): Promise<boolean> {
    const payload: AssignRolesRequest = { roles };
    const response = await firstValueFrom(this.api.assignRoles(id, payload));
    if (response.isSuccess) {
      this.toast.success('Đã gán vai trò cho tài khoản.');
      await this.load();
      return true;
    }
    this.toast.error(response.message || 'Không thể gán vai trò.');
    return false;
  }

  async lock(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.lock(id));
    if (response.isSuccess) {
      this.toast.success('Đã khóa tài khoản.');
      await this.load();
    }
  }

  async unlock(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.unlock(id));
    if (response.isSuccess) {
      this.toast.success('Đã mở khóa tài khoản.');
      await this.load();
    }
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm tài khoản.');
      await this.load();
    }
  }

  async restore(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.restore(id));
    if (response.isSuccess) {
      this.toast.success('Đã khôi phục tài khoản.');
      await this.load();
    }
  }

  async hardDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.hardDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa vĩnh viễn tài khoản.');
      await this.load();
    }
  }
}
