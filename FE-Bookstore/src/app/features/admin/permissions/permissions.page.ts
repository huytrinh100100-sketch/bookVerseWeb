import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { PermissionsStore } from './permissions.store';

type DialogMode = 'create' | 'view' | null;

@Component({
  selector: 'app-admin-permissions',
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
      title="Phân quyền &amp; Truy cập"
      subtitle="Ma trận quyền theo vai trò, dùng cho menu, route và directive."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Quản trị' }, { label: 'Phân quyền' }]"
    >
      <button type="button" class="btn btn--ghost"><i class="pi pi-history"></i> Nhật ký hệ thống</button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.RoleCreate" (click)="openCreate()">
        <i class="pi pi-plus"></i> Tạo vai trò
      </button>
    </app-page-header>

    <div class="layout">
      <aside class="roles">
        <h3>Vai trò</h3>
        @for (role of store.roles(); track role.id) {
          <button type="button" class="roles__item" [class.roles__item--active]="selectedRoleId() === role.id" (click)="selectedRoleId.set(role.id)">
            <span class="roles__name">
              {{ role.name }}
              <span class="roles__code">{{ role.code }}</span>
            </span>
            <span class="roles__count">{{ role.userCount }} tài khoản</span>
          </button>
        } @empty {
          <p class="roles__empty">Chưa có vai trò nào.</p>
        }
      </aside>

      <section class="matrix">
        <header class="matrix__head">
          <div>
            <h3>Ma trận quyền theo vai trò</h3>
            <p class="matrix__hint">Tích chọn các quyền sẽ áp dụng cho vai trò đang chọn.</p>
          </div>
          <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.RoleAssignPermissions" (click)="save()">
            <i class="pi pi-save"></i> Lưu ma trận
          </button>
        </header>
        @for (group of store.permissionGroups(); track group.name) {
          <div class="matrix__group">
            <h4>{{ group.name }}</h4>
            @for (permission of group.items; track permission.code) {
              <label class="matrix__row">
                <input type="checkbox" [checked]="isChecked(permission.code)" (change)="onToggle(permission.code, $event)" />
                <span class="matrix__code">{{ permission.code }}</span>
                <span class="matrix__name">{{ permission.name }}</span>
              </label>
            }
          </div>
        }
      </section>
    </div>

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo vai trò' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }
  `,
  styles: [
    `
      .layout { display: grid; grid-template-columns: 17rem 1fr; gap: 1rem; align-items: start; }
      @media (max-width: 60rem) { .layout { grid-template-columns: 1fr; } }
      .roles, .matrix {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        padding: 1.25rem;
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
      }
      .roles h3, .matrix h3 { margin: 0 0 0.75rem; font-size: 1rem; }
      .matrix__hint { margin: -0.5rem 0 1rem; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .roles__item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 0.5rem;
        width: 100%;
        border: none;
        background: transparent;
        padding: 0.625rem 0.75rem;
        border-radius: 0.5rem;
        cursor: pointer;
        text-align: left;
      }
      .roles__item:hover { background: var(--surface-container-low, #f2f4f6); }
      .roles__item--active { background: var(--secondary-container, #6cf8bb); font-weight: 700; }
      .roles__name { display: flex; flex-direction: column; font-size: 0.875rem; }
      .roles__code { font-family: monospace; font-size: 0.6875rem; color: var(--on-surface-variant, #45464d); font-weight: 400; }
      .roles__count { font-size: 0.6875rem; color: var(--on-surface-variant, #45464d); white-space: nowrap; }
      .roles__empty { color: var(--outline, #76777d); font-size: 0.875rem; padding: 1rem 0; margin: 0; text-align: center; }
      .matrix__head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
      .matrix__group { margin-top: 1.25rem; }
      .matrix__group h4 {
        margin: 0 0 0.5rem;
        font-size: 0.6875rem;
        text-transform: uppercase;
        letter-spacing: 0.06em;
        color: var(--on-surface-variant, #45464d);
        border-bottom: 1px solid var(--outline-variant, #c6c6cd);
        padding-bottom: 0.375rem;
      }
      .matrix__row {
        display: grid;
        grid-template-columns: 1.25rem 13rem 1fr;
        align-items: center;
        gap: 0.5rem;
        padding: 0.3125rem 0;
        font-size: 0.875rem;
        cursor: pointer;
      }
      .matrix__row:hover { background: var(--surface-container-low, #f2f4f6); }
      .matrix__code { font-family: monospace; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .btn {
        border: none;
        border-radius: 0.5rem;
        padding: 0.5rem 1rem;
        cursor: pointer;
        font-size: 0.875rem;
        font-weight: 600;
        display: inline-flex;
        gap: 0.375rem;
        align-items: center;
      }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class AdminPermissionsPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(PermissionsStore);
  readonly selectedRoleId = signal('');

  readonly dialogMode = signal<DialogMode>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  async ngOnInit(): Promise<void> {
    await this.store.loadAll();
    const first = this.store.roles()[0];
    if (first) {
      this.selectedRoleId.set(first.id);
    }
  }

  isChecked(code: string): boolean {
    return (this.store.selectedRolePermissions()[this.selectedRoleId()] ?? []).includes(code);
  }

  onToggle(code: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.store.togglePermission(this.selectedRoleId(), code, checked);
  }

  async save(): Promise<void> {
    this.submitting.set(true);
    try {
      const ok = await this.store.saveRolePermissions(this.selectedRoleId());
      if (!ok) {
        this.formError.set('Không thể lưu ma trận quyền.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  openCreate(): void {
    this.formValues.set({
      code: '',
      name: '',
      description: '',
      isDefault: false,
      isActive: true
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.formError.set('');
  }

  onFormChange(values: Record<string, unknown>): void {
    this.formValues.set(values);
  }

  dialogTitle(): string {
    return 'Tạo vai trò mới';
  }

  dialogSubtitle(): string {
    return 'Đặt mã và tên vai trò, sau đó chọn quyền trong ma trận bên trái.';
  }

  dialogFields(): DialogField[] {
    return [
      { name: 'code', label: 'Mã vai trò', type: 'text', required: true, placeholder: 'vd: WAREHOUSE_STAFF' },
      { name: 'name', label: 'Tên vai trò', type: 'text', required: true, placeholder: 'vd: Nhân viên kho' },
      { name: 'description', label: 'Mô tả', type: 'textarea', placeholder: 'Mô tả phạm vi công việc...' },
      { name: 'isDefault', label: 'Vai trò mặc định', type: 'checkbox', placeholder: 'Gán cho tài khoản mới' },
      { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    this.formError.set('');
    this.submitting.set(true);

    try {
      const ok = await this.store.createRole({
        code: String(values['code'] ?? '').trim(),
        name: String(values['name'] ?? '').trim(),
        description: String(values['description'] ?? '').trim() || undefined,
        isDefault: Boolean(values['isDefault']),
        isActive: Boolean(values['isActive']),
        permissionCodes: []
      });

      if (ok) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể tạo vai trò. Kiểm tra lại mã và tên vai trò.');
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
