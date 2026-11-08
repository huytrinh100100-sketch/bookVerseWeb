import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { SearchFilterBarComponent, SearchFilterValue } from '../../../shared/ui/search-filter-bar/search-filter-bar.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { AccountListItemResponse } from './accounts.models';
import { AccountsStore } from './accounts.store';

type DialogMode = 'create' | 'edit' | 'view' | 'roles' | null;

@Component({
  selector: 'app-admin-accounts',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    DataTableComponent,
    PageHeaderComponent,
    SearchFilterBarComponent,
    StatCardComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Quản lý tài khoản"
      subtitle="Tạo tài khoản cho nhân viên và người bán hàng, gán vai trò để kiểm soát phạm vi truy cập."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Quản trị' }, { label: 'Tài khoản' }]"
    >
      <button type="button" class="btn btn--ghost" (click)="openRoles()">
        <i class="pi pi-shield"></i> Vai trò &amp; quyền
      </button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.AccountCreate" (click)="openCreate()">
        <i class="pi pi-user-plus"></i> Thêm tài khoản
      </button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tổng tài khoản', value: (store.totalItems() | number) + '', icon: 'pi pi-id-card', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Đang hoạt động', value: store.activeCount() + '', icon: 'pi pi-check-circle', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Đang bị khóa', value: store.lockedCount() + '', icon: 'pi pi-lock', tone: 'warning' }" />
      <app-stat-card [config]="{ label: 'Đã xóa mềm', value: store.deletedCount() + '', icon: 'pi pi-trash', tone: 'danger' }" />
    </div>

    <app-search-filter-bar
      [fields]="filterFields"
      [value]="filterValue()"
      searchPlaceholder="Tìm theo tên đăng nhập, họ tên hoặc email..."
      (changed)="onFilterChanged($event)"
      (reset)="onReset()"
    />

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.AccountUpdate" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn" title="Gán vai trò" *hasPermission="PermissionConstants.AccountAssignRoles" (click)="openRoles(row)"><i class="pi pi-id-badge"></i></button>
      @if (row.isLocked) {
        <button type="button" class="icon-btn" title="Mở khóa" *hasPermission="PermissionConstants.AccountUnlock" (click)="askUnlock(row)"><i class="pi pi-lock-open"></i></button>
      } @else {
        <button type="button" class="icon-btn" title="Khóa tài khoản" *hasPermission="PermissionConstants.AccountLock" (click)="askLock(row)"><i class="pi pi-lock"></i></button>
      }
      @if (row.isDeleted) {
        <button type="button" class="icon-btn" title="Khôi phục" *hasPermission="PermissionConstants.AccountRestore" (click)="askRestore(row)"><i class="pi pi-refresh"></i></button>
      } @else {
        <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.AccountDeleteTemp" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
      }
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogSubmitLabel()"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmAction()) {
      <app-confirm-dialog
        [title]="confirmTitle()"
        [message]="confirmMessage()"
        [detail]="confirmDetail()"
        [confirmLabel]="confirmLabel()"
        [tone]="confirmTone()"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmAction.set(null)"
      />
    }
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
        transition: background 0.15s ease;
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
export class AdminAccountsPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(AccountsStore);

  readonly rows = computed(() =>
    this.store.items().map((account) => ({ ...account, status: this.statusOf(account) }))
  );

  /** Chuỗi trạng thái hiển thị: Đã xóa / Đang khóa / Đang hoạt động / Không hoạt động. */
  statusOf(account: AccountListItemResponse): string {
    if (account.isDeleted) {
      return 'Deleted';
    }
    if (account.isLocked) {
      return 'Locked';
    }
    return account.isActive ? 'Active' : 'Inactive';
  }

  readonly filterValue = signal<SearchFilterValue>({ keyword: '', filters: {} });

  readonly filterFields = [
    {
      key: 'status',
      label: 'Trạng thái',
      type: 'select' as const,
      placeholder: 'Tất cả trạng thái',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Đang hoạt động', value: 'active' },
        { label: 'Đã khóa', value: 'locked' },
        { label: 'Đã xóa mềm', value: 'deleted' }
      ]
    }
  ];

  readonly columns: TableColumn[] = [
    { field: 'fullName', header: 'Họ và tên' },
    { field: 'userName', header: 'Tên đăng nhập' },
    { field: 'email', header: 'Email' },
    { field: 'phoneNumber', header: 'Điện thoại' },
    { field: 'roles', header: 'Vai trò', type: 'chips' },
    {
      field: 'status',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Active: { label: 'Đang hoạt động', severity: 'success' },
        Inactive: { label: 'Không hoạt động', severity: 'info' },
        Locked: { label: 'Đang khóa', severity: 'warning' },
        Deleted: { label: 'Đã xóa mềm', severity: 'danger' }
      }
    }
  ];

  // ---- Dialog state
  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<AccountListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  // ---- Confirm state
  readonly confirmAction = signal<'lock' | 'unlock' | 'delete' | 'restore' | null>(null);
  readonly confirmTarget = signal<AccountListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.load();
    await this.store.loadRoles();
  }

  // ================= Lọc & tải dữ liệu =================

  onFilterChanged(value: SearchFilterValue): void {
    this.filterValue.set(value);
    const status = value.filters['status'];

    const params = {
      keyword: value.keyword || undefined,
      isDeleted: status === 'deleted' ? true : status === '' ? false : undefined,
      isActive: status === 'active' ? true : undefined
    };

    void this.store.load(params);
  }

  onReset(): void {
    this.filterValue.set({ keyword: '', filters: {} });
    void this.store.load({ keyword: undefined, isDeleted: false, isActive: undefined });
  }

  // ================= Dialog: tạo / sửa / xem =================

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      userName: '',
      fullName: '',
      email: '',
      password: '',
      phoneNumber: '',
      isActive: true,
      roles: []
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: AccountListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      userName: row.userName,
      fullName: row.fullName,
      email: row.email ?? '',
      phoneNumber: row.phoneNumber ?? '',
      isActive: row.isActive
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: AccountListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      userName: row.userName,
      fullName: row.fullName,
      email: row.email ?? '',
      phoneNumber: row.phoneNumber ?? '',
      isActive: row.isActive,
      roles: row.roles
    });
    this.formError.set('');
    this.dialogMode.set('view');
  }

  openRoles(row?: AccountListItemResponse): void {
    if (!row) {
      // Mở từ nút "Vai trò & quyền" -> điều hướng sang trang phân quyền.
      window.location.href = '/admin/permissions';
      return;
    }
    this.dialogTarget.set(row);
    this.formValues.set({ roles: [...(row.roles ?? [])] });
    this.formError.set('');
    this.dialogMode.set('roles');
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
      case 'create': return 'Thêm tài khoản mới';
      case 'edit': return 'Cập nhật tài khoản';
      case 'view': return 'Chi tiết tài khoản';
      case 'roles': return 'Gán vai trò';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    switch (this.dialogMode()) {
      case 'create': return 'Tài khoản sẽ dùng để đăng nhập vào hệ thống quản trị.';
      case 'edit': return target ? `Cập nhật thông tin của ${target.userName}.` : '';
      case 'view': return target ? `Thông tin chi tiết của ${target.userName}.` : '';
      case 'roles': return target ? `Chọn vai trò để quyết định quyền truy cập của ${target.userName}.` : '';
      default: return '';
    }
  }

  dialogSubmitLabel(): string {
    return this.dialogMode() === 'roles' ? 'Lưu vai trò' : 'Lưu thay đổi';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'userName', label: 'Tên đăng nhập', type: 'text', required: true, placeholder: 'vd: nhanvien.banhang', hint: 'Duy nhất, không chứa khoảng trắng.' },
          { name: 'fullName', label: 'Họ và tên', type: 'text', required: true, placeholder: 'vd: Nguyễn Văn A' },
          { name: 'email', label: 'Email', type: 'text', required: true, placeholder: 'vd: a@bookverse.vn' },
          { name: 'password', label: 'Mật khẩu', type: 'password', required: true, placeholder: 'Tối thiểu 8 ký tự' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text', placeholder: 'vd: 0901234567', half: true },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Tài khoản được phép đăng nhập ngay' },
          { name: 'roles', label: 'Vai trò', type: 'select', multiple: true, required: true, options: this.roleOptions(), hint: 'Người bán hàng chỉ thấy các mục menu thuộc vai trò được gán.' }
        ];
      case 'edit':
        return [
          { name: 'fullName', label: 'Họ và tên', type: 'text', required: true },
          { name: 'email', label: 'Email', type: 'text', required: true },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Tài khoản được phép đăng nhập' }
        ];
      case 'view':
        return [
          { name: 'userName', label: 'Tên đăng nhập', type: 'text' },
          { name: 'fullName', label: 'Họ và tên', type: 'text' },
          { name: 'email', label: 'Email', type: 'text' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' },
          { name: 'roles', label: 'Vai trò', type: 'select', multiple: true, options: this.roleOptions() }
        ];
      case 'roles':
        return [
          {
            name: 'roles',
            label: 'Vai trò được cấp',
            type: 'select',
            multiple: true,
            required: true,
            options: this.roleOptions(),
            hint: 'Mỗi vai trò gắn một nhóm quyền; người dùng chỉ thấy menu và trang tương ứng.'
          }
        ];
      default:
        return [];
    }
  }

  roleOptions(): { label: string; value: string }[] {
    // BE so sanh theo Role.Name (khong phai Code) trong AssignRolesAsync.
    return this.store.roles().map((role) => ({
      label: `${role.name}${role.description ? ' - ' + role.description : ''}`,
      value: role.name
    }));
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      switch (this.dialogMode()) {
        case 'create': {
          const roles = (values['roles'] as string[]) ?? [];
          ok = await this.store.create({
            userName: String(values['userName'] ?? '').trim(),
            fullName: String(values['fullName'] ?? '').trim(),
            email: String(values['email'] ?? '').trim(),
            password: String(values['password'] ?? ''),
            phoneNumber: String(values['phoneNumber'] ?? '').trim() || undefined,
            isActive: Boolean(values['isActive']),
            roles
          });
          break;
        }
        case 'edit': {
          if (!target) {
            break;
          }
          ok = await this.store.update(target.id, {
            fullName: String(values['fullName'] ?? '').trim(),
            email: String(values['email'] ?? '').trim(),
            phoneNumber: String(values['phoneNumber'] ?? '').trim(),
            isActive: Boolean(values['isActive'])
          });
          break;
        }
        case 'roles': {
          if (!target) {
            break;
          }
          ok = await this.store.assignRoles(target.id, (values['roles'] as string[]) ?? []);
          break;
        }
        default:
          break;
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

  // ================= Xác nhận hành động =================

  askLock(row: AccountListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('lock');
  }

  askUnlock(row: AccountListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('unlock');
  }

  askDelete(row: AccountListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('delete');
  }

  askRestore(row: AccountListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('restore');
  }

  confirmTitle(): string {
    switch (this.confirmAction()) {
      case 'lock': return 'Khóa tài khoản';
      case 'unlock': return 'Mở khóa tài khoản';
      case 'delete': return 'Xóa mềm tài khoản';
      case 'restore': return 'Khôi phục tài khoản';
      default: return '';
    }
  }

  confirmMessage(): string {
    switch (this.confirmAction()) {
      case 'lock': return 'Tài khoản sẽ không thể đăng nhập cho đến khi được mở khóa.';
      case 'unlock': return 'Tài khoản sẽ đăng nhập được trở lại ngay.';
      case 'delete': return 'Tài khoản sẽ bị ẩn khỏi danh sách nhưng vẫn có thể khôi phục.';
      case 'restore': return 'Tài khoản sẽ hiển thị lại trong danh sách.';
      default: return '';
    }
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `Tài khoản: ${target.fullName} (${target.userName})` : '';
  }

  confirmLabel(): string {
    switch (this.confirmAction()) {
      case 'lock': return 'Khóa';
      case 'unlock': return 'Mở khóa';
      case 'delete': return 'Xóa mềm';
      case 'restore': return 'Khôi phục';
      default: return 'Xác nhận';
    }
  }

  confirmTone(): 'danger' | 'warning' | 'info' {
    switch (this.confirmAction()) {
      case 'lock': return 'warning';
      case 'unlock': return 'info';
      case 'delete': return 'danger';
      default: return 'info';
    }
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    const action = this.confirmAction();
    if (!target || !action) {
      return;
    }

    this.submitting.set(true);
    try {
      switch (action) {
        case 'lock': await this.store.lock(target.id); break;
        case 'unlock': await this.store.unlock(target.id); break;
        case 'delete': await this.store.softDelete(target.id); break;
        case 'restore': await this.store.restore(target.id); break;
      }
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
