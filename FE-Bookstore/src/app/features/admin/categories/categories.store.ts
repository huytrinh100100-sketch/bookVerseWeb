import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { CategoriesApiService } from './categories-api.service';
import {
  CategoryDetailResponse,
  CategoryTreeNodeResponse,
  CreateCategoryRequest,
  MoveCategoryRequest,
  UpdateCategoryRequest
} from './categories.models';

@Injectable({ providedIn: 'root' })
export class CategoriesStore {
  private readonly api = inject(CategoriesApiService);
  private readonly toast = inject(ToastService);

  readonly tree = signal<CategoryTreeNodeResponse[]>([]);
  readonly detail = signal<CategoryDetailResponse | null>(null);
  readonly loading = signal(false);
  readonly treeLoading = signal(false);

  async loadTree(rootCode?: string): Promise<void> {
    this.treeLoading.set(true);
    try {
      const response = await firstValueFrom(this.api.getTree(rootCode));
      this.tree.set(response.data ?? []);
    } finally {
      this.treeLoading.set(false);
    }
  }

  async loadDetail(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.getById(id));
    this.detail.set(response.data ?? null);
  }

  async create(payload: CreateCategoryRequest): Promise<CategoryDetailResponse | null> {
    const response = await firstValueFrom(this.api.create(payload));
    if (response.isSuccess) {
      this.toast.success('Tạo danh mục thành công.');
      this.detail.set(response.data ?? null);
      await this.loadTree(payload.rootCode);
      return response.data ?? null;
    }
    return null;
  }

  async update(id: string, payload: UpdateCategoryRequest): Promise<CategoryDetailResponse | null> {
    const response = await firstValueFrom(this.api.update(id, payload));
    if (response.isSuccess) {
      this.toast.success('Cập nhật danh mục thành công.');
      this.detail.set(response.data ?? null);
      // BE UpdateCategoryRequest khong co rootCode -> tai lai toan bo cay.
      await this.loadTree();
      return response.data ?? null;
    }
    return null;
  }

  async move(id: string, payload: MoveCategoryRequest): Promise<boolean> {
    const response = await firstValueFrom(this.api.move(id, payload));
    if (response.isSuccess) {
      this.toast.success('Di chuyển danh mục thành công.');
      await this.loadTree();
      return true;
    }
    return false;
  }

  async softDelete(id: string): Promise<void> {
    const response = await firstValueFrom(this.api.softDelete(id));
    if (response.isSuccess) {
      this.toast.success('Đã xóa mềm danh mục.');
      await this.loadTree();
    }
  }
}
