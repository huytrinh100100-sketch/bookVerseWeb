import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { CategoriesStore } from './categories.store';
import { CategoryTreeNodeResponse } from './categories.models';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [
    FormsModule,
    PageHeaderComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Quản lý danh mục"
      subtitle="Cây danh mục đóng vai trò điều hướng mega-menu của storefront."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Danh mục' }, { label: 'Quản lý danh mục' }]"
    >
      <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.CategoryReorder"><i class="pi pi-sort"></i> Sắp xếp lại cây</button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.CategoryCreate" (click)="openCreate()"><i class="pi pi-plus"></i> Thêm danh mục</button>
    </app-page-header>

    <div class="tree">
      @for (node of tree(); track node.id) {
        <div class="tree__node tree__node--root">
          <div class="tree__row">
            <i class="pi pi-folder-open"></i>
            <span class="tree__name">{{ node.name }}</span>
            <span class="tree__code">{{ node.code }}</span>
            <span class="tree__count">{{ node.bookCount }} sách</span>
            @if (node.isMegaMenu) {
              <span class="tree__flag">Mega menu</span>
            }
            @if (!node.isActive) {
              <span class="tree__flag tree__flag--muted">Đang ẩn</span>
            }
            <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(node)"><i class="pi pi-eye"></i></button>
            <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.CategoryUpdate" (click)="openEdit(node)"><i class="pi pi-pencil"></i></button>
            <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.CategoryDeleteTemp" (click)="askDelete(node)"><i class="pi pi-trash"></i></button>
          </div>
          @for (child of node.children; track child.id) {
            <div class="tree__row tree__row--child">
              <i class="pi pi-folder"></i>
              <span class="tree__name">{{ child.name }}</span>
              <span class="tree__code">{{ child.code }}</span>
              <span class="tree__count">{{ child.bookCount }} sách</span>
              <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(child)"><i class="pi pi-eye"></i></button>
              <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.CategoryUpdate" (click)="openEdit(child)"><i class="pi pi-pencil"></i></button>
              <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.CategoryDeleteTemp" (click)="askDelete(child)"><i class="pi pi-trash"></i></button>
            </div>
          }
        </div>
      } @empty {
        <p class="tree__empty">Chưa có danh mục nào.</p>
      }
    </div>

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo danh mục' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmAction()) {
      <app-confirm-dialog
        title="Xóa mềm danh mục"
        message="Danh mục sẽ bị ẩn khỏi cây điều hướng. Các sách vẫn giữ nguyên liên kết."
        [detail]="confirmDetail()"
        confirmLabel="Xóa mềm"
        tone="danger"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmAction.set(null)"
      />
    }
  `,
  styles: [
    `
      .tree {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        padding: 1rem 1.25rem;
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
      }
      .tree__node { border-left: 2px solid var(--outline-variant, #c6c6cd); margin-left: 0.5rem; padding-left: 1rem; }
      .tree__row {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem 0.5rem;
        border-bottom: 1px solid var(--surface-container-high, #e6e8ea);
        border-radius: 0.5rem;
      }
      .tree__row:hover { background: var(--surface-container-low, #f2f4f6); }
      .tree__row--child { padding-left: 1.75rem; }
      .tree__name { font-weight: 600; }
      .tree__code { font-family: monospace; font-size: 0.75rem; color: var(--outline, #76777d); }
      .tree__count { font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); margin-left: auto; }
      .tree__flag { font-size: 0.6875rem; padding: 0.125rem 0.5rem; border-radius: 9999px; background: var(--tertiary-fixed, #d8e2ff); color: #001a42; font-weight: 600; }
      .tree__flag--muted { background: var(--surface-container-high, #e6e8ea); color: var(--on-surface-variant, #45464d); }
      .tree__empty { text-align: center; color: var(--outline, #76777d); padding: 2rem; margin: 0; }
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
      }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); color: var(--on-surface, #191c1e); }
      .icon-btn--danger { color: var(--error, #ba1a1a); }
      .btn {
        border: none;
        border-radius: 0.5rem;
        padding: 0.5rem 1rem;
        cursor: pointer;
        font-size: 0.875rem;
        font-weight: 600;
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class AdminCategoriesPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(CategoriesStore);

  readonly tree = computed(() => this.store.tree());

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<CategoryTreeNodeResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'delete' | null>(null);
  readonly confirmTarget = signal<CategoryTreeNodeResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.loadTree();
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      code: '',
      name: '',
      slug: '',
      rootCode: 'BOOK',
      parentId: '',
      sortOrder: 0,
      description: '',
      iconUrl: '',
      isMegaMenu: false,
      isActive: true
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(node: CategoryTreeNodeResponse): void {
    this.dialogTarget.set(node);
    this.formValues.set({
      code: node.code,
      name: node.name,
      slug: node.slug ?? '',
      rootCode: node.rootCode,
      parentId: node.parentId ?? '',
      sortOrder: node.sortOrder,
      description: '',
      iconUrl: '',
      isMegaMenu: node.isMegaMenu,
      isActive: node.isActive
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(node: CategoryTreeNodeResponse): void {
    this.dialogTarget.set(node);
    this.formValues.set({
      code: node.code,
      name: node.name,
      slug: node.slug ?? '',
      rootCode: node.rootCode,
      rootName: node.rootName,
      parentId: node.parentId ?? '',
      sortOrder: node.sortOrder,
      bookCount: node.bookCount,
      isMegaMenu: node.isMegaMenu,
      isActive: node.isActive
    });
    this.formError.set('');
    this.dialogMode.set('view');
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.dialogTarget.set(null);
    this.formError.set('');
  }

  onFormChange(values: Record<string, unknown>): void {
    this.formValues.set(values);
  }

  dialogTitle(): string {
    switch (this.dialogMode()) {
      case 'create': return 'Thêm danh mục';
      case 'edit': return 'Cập nhật danh mục';
      case 'view': return 'Chi tiết danh mục';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật danh mục "${target.name}".`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã danh mục: ${target.code}.`;
    }
    return 'Chọn cây gốc và danh mục cha để đặt đúng vị trí.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'code', label: 'Mã danh mục', type: 'text', required: true, placeholder: 'vd: VN-LIT-01' },
          { name: 'name', label: 'Tên danh mục', type: 'text', required: true, placeholder: 'vd: Văn học Việt Nam' },
          { name: 'slug', label: 'Slug (URL)', type: 'text', placeholder: 'vd: van-hoc-viet-nam' },
          { name: 'rootCode', label: 'Cây gốc', type: 'select', multiple: false, required: true, options: this.rootOptions() },
          { name: 'parentId', label: 'Danh mục cha', type: 'select', multiple: false, options: this.parentOptions() },
          { name: 'sortOrder', label: 'Thứ tự hiển thị', type: 'number', min: 0 },
          { name: 'iconUrl', label: 'Biểu tượng (URL)', type: 'text' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'isMegaMenu', label: 'Hiện trên mega-menu', type: 'checkbox', placeholder: 'Hiển thị trong mega-menu storefront' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      case 'edit':
        return [
          { name: 'name', label: 'Tên danh mục', type: 'text', required: true },
          { name: 'slug', label: 'Slug (URL)', type: 'text' },
          { name: 'sortOrder', label: 'Thứ tự hiển thị', type: 'number', min: 0 },
          { name: 'iconUrl', label: 'Biểu tượng (URL)', type: 'text' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'isMegaMenu', label: 'Hiện trên mega-menu', type: 'checkbox', placeholder: 'Hiển thị trong mega-menu storefront' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã danh mục', type: 'text' },
          { name: 'name', label: 'Tên danh mục', type: 'text' },
          { name: 'slug', label: 'Slug (URL)', type: 'text' },
          { name: 'rootName', label: 'Cây gốc', type: 'text' },
          { name: 'sortOrder', label: 'Thứ tự hiển thị', type: 'number' },
          { name: 'bookCount', label: 'Số sách', type: 'number' },
          { name: 'isMegaMenu', label: 'Hiện trên mega-menu', type: 'checkbox', placeholder: 'Có trong mega-menu' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      default:
        return [];
    }
  }

  rootOptions(): { label: string; value: string }[] {
    return [
      { label: 'Sách', value: 'BOOK' },
      { label: 'Nhà xuất bản', value: 'PUBLISHER' },
      { label: 'Định dạng', value: 'FORMAT' },
      { label: 'Ngôn ngữ', value: 'LANGUAGE' },
      { label: 'Đối tượng', value: 'AUDIENCE' },
      { label: 'Ấn phẩm', value: 'EDITION' }
    ];
  }

  parentOptions(): { label: string; value: string }[] {
    const options: { label: string; value: string }[] = [{ label: '-- Danh mục gốc --', value: '' }];
    this.tree().forEach((node) => {
      options.push({ label: node.name, value: node.id });
      node.children?.forEach((child) => options.push({ label: `　${child.name}`, value: child.id }));
    });
    return options;
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      if (this.dialogMode() === 'create') {
        const result = await this.store.create({
          code: String(values['code'] ?? '').trim(),
          name: String(values['name'] ?? '').trim(),
          slug: String(values['slug'] ?? '').trim() || undefined,
          rootCode: String(values['rootCode'] ?? 'BOOK'),
          parentId: String(values['parentId'] ?? '') || null,
          sortOrder: Number(values['sortOrder'] ?? 0),
          description: String(values['description'] ?? '').trim() || undefined,
          iconUrl: String(values['iconUrl'] ?? '').trim() || undefined,
          isMegaMenu: Boolean(values['isMegaMenu']),
          isActive: Boolean(values['isActive'])
        });
        ok = result !== null;
      } else if (this.dialogMode() === 'edit' && target) {
        const result = await this.store.update(target.id, {
          name: String(values['name'] ?? '').trim(),
          slug: String(values['slug'] ?? '').trim(),
          sortOrder: Number(values['sortOrder'] ?? 0),
          description: String(values['description'] ?? '').trim() || undefined,
          iconUrl: String(values['iconUrl'] ?? '').trim() || undefined,
          isMegaMenu: Boolean(values['isMegaMenu']),
          isActive: Boolean(values['isActive'])
        });
        ok = result !== null;
      }

      if (ok) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  askDelete(node: CategoryTreeNodeResponse): void {
    this.confirmTarget.set(node);
    this.confirmAction.set('delete');
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `${target.name} (${target.code})` : '';
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    if (!target) {
      return;
    }

    this.submitting.set(true);
    try {
      await this.store.softDelete(target.id);
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
