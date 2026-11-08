import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { SearchFilterBarComponent, SearchFilterValue } from '../../../shared/ui/search-filter-bar/search-filter-bar.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { CustomerListItemResponse, CustomerStatus, VipTier } from './customers.models';
import { CustomersStore } from './customers.store';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-customers',
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
      title="Danh sách khách hàng"
      subtitle="Phân khúc VIP, lịch sử mua và phân bố địa lý khách hàng."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Bán hàng' }, { label: 'Khách hàng' }]"
    >
      <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.CustomerExportPii"><i class="pi pi-download"></i> Xuất CSV</button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.CustomerCreate" (click)="openCreate()"><i class="pi pi-user-plus"></i> Thêm khách hàng</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tổng khách hàng', value: (store.totalItems() | number) + '', icon: 'pi pi-users', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Khách VIP Platinum', value: store.platinumCount() + '', icon: 'pi pi-star-fill', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Khách VIP Gold', value: store.goldCount() + '', icon: 'pi pi-star', tone: 'warning' }" />
      <app-stat-card [config]="{ label: 'Tổng chi tiêu', value: (store.totalLifetimeSpend() | number) + '', icon: 'pi pi-wallet', tone: 'info' }" />
    </div>

    <app-search-filter-bar
      [fields]="filterFields"
      [value]="filterValue()"
      searchPlaceholder="Tìm theo họ tên, email hoặc số điện thoại..."
      (changed)="onFilterChanged($event)"
      (reset)="onReset()"
    />

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.CustomerEdit" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn" title="Gửi voucher" *hasPermission="PermissionConstants.CustomerSendVoucher" (click)="askVoucher(row)"><i class="pi pi-ticket"></i></button>
      <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.CustomerDeleteTemp" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo khách hàng' : 'Lưu thay đổi'"
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
        [tone]="confirmAction() === 'delete' ? 'danger' : 'info'"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmAction.set(null)"
      />
    }
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(13rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
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
export class AdminCustomersPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(CustomersStore);

  readonly rows = computed(() => this.store.items());
  readonly filterValue = signal<SearchFilterValue>({ keyword: '', filters: {} });

  readonly filterFields = [
    {
      key: 'vipTier',
      label: 'Hạng VIP',
      type: 'select' as const,
      placeholder: 'Tất cả hạng',
      options: [
        { label: 'Tất cả hạng', value: '' },
        { label: 'Standard', value: 'Standard' },
        { label: 'Silver', value: 'Silver' },
        { label: 'Gold', value: 'Gold' },
        { label: 'Platinum', value: 'Platinum' }
      ]
    },
    {
      key: 'status',
      label: 'Trạng thái',
      type: 'select' as const,
      placeholder: 'Tất cả trạng thái',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Đang hoạt động', value: 'Active' },
        { label: 'Tạm ngưng', value: 'Suspended' }
      ]
    }
  ];

  readonly columns: TableColumn[] = [
    { field: 'fullName', header: 'Khách hàng' },
    { field: 'phoneNumber', header: 'Điện thoại' },
    { field: 'provinceName', header: 'Khu vực' },
    { field: 'vipTier', header: 'Hạng VIP', type: 'status', statusMap: {
      Platinum: { label: 'Platinum', severity: 'success' },
      Gold: { label: 'Gold', severity: 'warning' },
      Silver: { label: 'Silver', severity: 'info' },
      Standard: { label: 'Standard', severity: 'info' }
    } },
    { field: 'orderCount', header: 'Số đơn', type: 'number' },
    { field: 'lifetimeSpend', header: 'Tổng chi tiêu', type: 'money' },
    { field: 'status', header: 'Trạng thái', type: 'status', statusMap: {
      Active: { label: 'Đang hoạt động', severity: 'success' },
      Suspended: { label: 'Tạm ngưng', severity: 'danger' }
    } }
  ];

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<CustomerListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'delete' | 'voucher' | null>(null);
  readonly confirmTarget = signal<CustomerListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.load();
  }

  onFilterChanged(value: SearchFilterValue): void {
    this.filterValue.set(value);
    void this.store.load({
      keyword: value.keyword || undefined,
      vipTier: (value.filters['vipTier'] as VipTier) || undefined,
      status: (value.filters['status'] as CustomerStatus) || undefined
    });
  }

  onReset(): void {
    this.filterValue.set({ keyword: '', filters: {} });
    void this.store.load({ keyword: undefined, vipTier: undefined, status: undefined });
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      fullName: '',
      userName: '',
      email: '',
      password: '',
      phoneNumber: '',
      address: '',
      birthday: '',
      vipTier: VipTier.Standard,
      note: ''
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: CustomerListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      fullName: row.fullName,
      phoneNumber: row.phoneNumber ?? '',
      address: row.address ?? '',
      vipTier: row.vipTier,
      status: row.status,
      note: ''
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: CustomerListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      fullName: row.fullName,
      userName: row.userName ?? '',
      email: row.email ?? '',
      phoneNumber: row.phoneNumber ?? '',
      address: row.address ?? '',
      provinceName: row.provinceName ?? '—',
      communeName: row.communeName ?? '—',
      vipTier: row.vipTier,
      orderCount: row.orderCount,
      lifetimeSpend: row.lifetimeSpend,
      preferredCategory: row.preferredCategory ?? '—',
      status: row.status,
      lastOrderDate: row.lastOrderDate ?? ''
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
      case 'create': return 'Thêm khách hàng';
      case 'edit': return 'Cập nhật khách hàng';
      case 'view': return 'Chi tiết khách hàng';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật thông tin của ${target.fullName}.`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã khách hàng: ${target.code}.`;
    }
    return 'Khách hàng đăng ký qua cửa hàng hoặc được quản trị tạo thủ công.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'fullName', label: 'Họ và tên', type: 'text', required: true, placeholder: 'vd: Nguyễn Thị B' },
          { name: 'userName', label: 'Tên đăng nhập', type: 'text', placeholder: 'vd: nguyenthib' },
          { name: 'email', label: 'Email', type: 'text', required: true, placeholder: 'vd: b@bookverse.vn' },
          { name: 'password', label: 'Mật khẩu', type: 'password', placeholder: 'Tối thiểu 8 ký tự' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text', placeholder: 'vd: 0901234567' },
          { name: 'address', label: 'Địa chỉ', type: 'text' },
          { name: 'birthday', label: 'Ngày sinh', type: 'date' },
          { name: 'vipTier', label: 'Hạng VIP', type: 'select', multiple: false, options: this.vipOptions() },
          { name: 'note', label: 'Ghi chú', type: 'textarea' }
        ];
      case 'edit':
        return [
          { name: 'fullName', label: 'Họ và tên', type: 'text', required: true },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'address', label: 'Địa chỉ', type: 'text' },
          { name: 'vipTier', label: 'Hạng VIP', type: 'select', multiple: false, options: this.vipOptions() },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, options: this.statusOptions() },
          { name: 'note', label: 'Ghi chú', type: 'textarea' }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã khách hàng', type: 'text' },
          { name: 'fullName', label: 'Họ và tên', type: 'text' },
          { name: 'userName', label: 'Tên đăng nhập', type: 'text' },
          { name: 'email', label: 'Email', type: 'text' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'address', label: 'Địa chỉ', type: 'text' },
          { name: 'provinceName', label: 'Tỉnh / thành phố', type: 'text' },
          { name: 'communeName', label: 'Xã / phường', type: 'text' },
          { name: 'vipTier', label: 'Hạng VIP', type: 'text' },
          { name: 'orderCount', label: 'Số đơn', type: 'number' },
          { name: 'lifetimeSpend', label: 'Tổng chi tiêu', type: 'money' },
          { name: 'preferredCategory', label: 'Danh mục yêu thích', type: 'text' },
          { name: 'status', label: 'Trạng thái', type: 'text' },
          { name: 'lastOrderDate', label: 'Đơn gần nhất', type: 'date' }
        ];
      default:
        return [];
    }
  }

  vipOptions(): { label: string; value: string }[] {
    return [
      { label: 'Standard', value: VipTier.Standard },
      { label: 'Silver', value: VipTier.Silver },
      { label: 'Gold', value: VipTier.Gold },
      { label: 'Platinum', value: VipTier.Platinum }
    ];
  }

  statusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Đang hoạt động', value: CustomerStatus.Active },
      { label: 'Tạm ngưng', value: CustomerStatus.Suspended }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      if (this.dialogMode() === 'create') {
        ok = await this.store.create({
          fullName: String(values['fullName'] ?? '').trim(),
          userName: String(values['userName'] ?? '').trim() || undefined,
          email: String(values['email'] ?? '').trim(),
          password: String(values['password'] ?? '') || undefined,
          phoneNumber: String(values['phoneNumber'] ?? '').trim() || undefined,
          address: String(values['address'] ?? '').trim() || undefined,
          birthday: String(values['birthday'] ?? '') || undefined,
          vipTier: (values['vipTier'] as VipTier) ?? VipTier.Standard,
          note: String(values['note'] ?? '').trim() || undefined
        });
      } else if (this.dialogMode() === 'edit' && target) {
        ok = await this.store.update(target.id, {
          fullName: String(values['fullName'] ?? '').trim(),
          phoneNumber: String(values['phoneNumber'] ?? '').trim(),
          address: String(values['address'] ?? '').trim(),
          vipTier: (values['vipTier'] as VipTier) ?? undefined,
          status: (values['status'] as CustomerStatus) ?? undefined,
          note: String(values['note'] ?? '').trim()
        });
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

  askDelete(row: CustomerListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('delete');
  }

  askVoucher(row: CustomerListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('voucher');
  }

  confirmTitle(): string {
    return this.confirmAction() === 'delete' ? 'Xóa mềm khách hàng' : 'Gửi voucher';
  }

  confirmMessage(): string {
    return this.confirmAction() === 'delete'
      ? 'Khách hàng sẽ bị ẩn khỏi danh sách nhưng vẫn có thể khôi phục.'
      : 'Voucher sẽ được gửi tới khách hàng qua kênh thông báo của hệ thống.';
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `Khách hàng: ${target.fullName} (${target.email ?? target.code})` : '';
  }

  confirmLabel(): string {
    return this.confirmAction() === 'delete' ? 'Xóa mềm' : 'Gửi voucher';
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    const action = this.confirmAction();
    if (!target || !action) {
      return;
    }

    this.submitting.set(true);
    try {
      if (action === 'delete') {
        await this.store.softDelete(target.id);
      } else {
        await this.store.sendVoucher(target.id, 'WELCOME10');
      }
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
