import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { CouponAudience, CouponListItemResponse, CouponStatus, DiscountType } from './promotions.models';
import { PromotionsStore } from './promotions.store';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-promotions',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    DataTableComponent,
    PageHeaderComponent,
    StatCardComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Khuyến mại &amp; Voucher"
      subtitle="Mã giảm giá, chương trình tự động và flash deal."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Tiếp thị' }, { label: 'Voucher' }]"
    >
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.CouponCreate" (click)="openCreate()"><i class="pi pi-plus"></i> Tạo voucher</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tất cả khuyến mại', value: (store.totalItems() | number) + '', icon: 'pi pi-bolt', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Đang hoạt động', value: store.activeCount() + '', icon: 'pi pi-check-circle', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Đã lên lịch', value: store.scheduledCount() + '', icon: 'pi pi-clock', tone: 'info' }" />
      <app-stat-card [config]="{ label: 'Đã hết hạn', value: store.expiredCount() + '', icon: 'pi pi-ban', tone: 'warning' }" />
    </div>

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sao chép mã" (click)="copyCode(row)"><i class="pi pi-copy"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.CouponEdit" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.CouponDeleteTemp" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo voucher' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmAction()) {
      <app-confirm-dialog
        title="Xóa mềm voucher"
        message="Voucher sẽ bị ẩn khỏi danh sách và không còn áp dụng cho đơn mới."
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
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(13rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
      }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); }
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
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class AdminPromotionsPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(PromotionsStore);

  readonly rows = computed(() => this.store.items());

  readonly columns: TableColumn[] = [
    { field: 'code', header: 'Mã voucher' },
    { field: 'name', header: 'Tên chương trình' },
    {
      field: 'discountType',
      header: 'Loại giảm giá',
      type: 'status',
      statusMap: {
        Percent: { label: 'Phần trăm', severity: 'success' },
        FixedAmount: { label: 'Số tiền cố định', severity: 'info' },
        FreeShipping: { label: 'Miễn phí vận chuyển', severity: 'info' },
        BuyXGetY: { label: 'Mua X tặng Y', severity: 'warning' }
      }
    },
    { field: 'discountValue', header: 'Mức giảm', type: 'number' },
    { field: 'usageLimit', header: 'Đã dùng / Hạn', type: 'number' },
    { field: 'endDate', header: 'Kết thúc', type: 'date' },
    {
      field: 'status',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Draft: { label: 'Bản nháp', severity: 'warning' },
        Active: { label: 'Đang hoạt động', severity: 'success' },
        Scheduled: { label: 'Đã lên lịch', severity: 'info' },
        Expired: { label: 'Đã hết hạn', severity: 'danger' },
        Paused: { label: 'Tạm dừng', severity: 'info' }
      }
    }
  ];

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<CouponListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'delete' | null>(null);
  readonly confirmTarget = signal<CouponListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.load();
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      code: '',
      name: '',
      description: '',
      discountType: DiscountType.Percent,
      discountValue: 10,
      minOrderValue: 0,
      maxDiscountAmount: 0,
      usageLimit: 100,
      perUserUsageLimit: 1,
      startDate: '',
      endDate: '',
      audience: CouponAudience.Public,
      status: CouponStatus.Draft
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: CouponListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      code: row.code,
      name: row.name,
      description: row.description ?? '',
      discountType: row.discountType,
      discountValue: row.discountValue,
      minOrderValue: row.minOrderValue,
      maxDiscountAmount: row.maxDiscountAmount ?? 0,
      usageLimit: row.usageLimit,
      perUserUsageLimit: row.perUserUsageLimit,
      startDate: row.startDate,
      endDate: row.endDate,
      audience: row.audience,
      status: row.status
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: CouponListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      code: row.code,
      name: row.name,
      description: row.description ?? '',
      discountType: row.discountType,
      discountValue: row.discountValue,
      minOrderValue: row.minOrderValue,
      maxDiscountAmount: row.maxDiscountAmount ?? 0,
      usageLimit: row.usageLimit,
      usedCount: row.usedCount,
      perUserUsageLimit: row.perUserUsageLimit,
      startDate: row.startDate,
      endDate: row.endDate,
      audience: row.audience,
      status: row.status
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
      case 'create': return 'Tạo voucher';
      case 'edit': return 'Cập nhật voucher';
      case 'view': return 'Chi tiết voucher';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật voucher "${target.code}".`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã voucher: ${target.code}.`;
    }
    return 'Thiết lập điều kiện áp dụng cho voucher.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'code', label: 'Mã voucher', type: 'text', required: true, placeholder: 'vd: WELCOME10' },
          { name: 'name', label: 'Tên chương trình', type: 'text', required: true, placeholder: 'vd: Giảm 10% đơn đầu tiên' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'discountType', label: 'Loại giảm giá', type: 'select', multiple: false, required: true, options: this.discountTypeOptions() },
          { name: 'discountValue', label: 'Mức giảm', type: 'number', required: true, min: 0, hint: 'Phần trăm hoặc số tiền tùy loại giảm giá.' },
          { name: 'minOrderValue', label: 'Đơn tối thiểu (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'maxDiscountAmount', label: 'Giảm tối đa (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'usageLimit', label: 'Số lượt sử dụng', type: 'number', required: true, min: 1 },
          { name: 'perUserUsageLimit', label: 'Số lượt mỗi khách', type: 'number', min: 1 },
          { name: 'startDate', label: 'Bắt đầu', type: 'date' },
          { name: 'endDate', label: 'Kết thúc', type: 'date' },
          { name: 'audience', label: 'Đối tượng', type: 'select', multiple: false, options: this.audienceOptions() },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, required: true, options: this.statusOptions() }
        ];
      case 'edit':
        return [
          { name: 'name', label: 'Tên chương trình', type: 'text', required: true },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'discountType', label: 'Loại giảm giá', type: 'select', multiple: false, options: this.discountTypeOptions() },
          { name: 'discountValue', label: 'Mức giảm', type: 'number', min: 0 },
          { name: 'minOrderValue', label: 'Đơn tối thiểu (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'maxDiscountAmount', label: 'Giảm tối đa (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'usageLimit', label: 'Số lượt sử dụng', type: 'number', min: 1 },
          { name: 'perUserUsageLimit', label: 'Số lượt mỗi khách', type: 'number', min: 1 },
          { name: 'startDate', label: 'Bắt đầu', type: 'date' },
          { name: 'endDate', label: 'Kết thúc', type: 'date' },
          { name: 'audience', label: 'Đối tượng', type: 'select', multiple: false, options: this.audienceOptions() },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, options: this.statusOptions() }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã voucher', type: 'text' },
          { name: 'name', label: 'Tên chương trình', type: 'text' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'discountType', label: 'Loại giảm giá', type: 'text' },
          { name: 'discountValue', label: 'Mức giảm', type: 'number' },
          { name: 'minOrderValue', label: 'Đơn tối thiểu', type: 'money' },
          { name: 'maxDiscountAmount', label: 'Giảm tối đa', type: 'money' },
          { name: 'usageLimit', label: 'Số lượt sử dụng', type: 'number' },
          { name: 'usedCount', label: 'Đã sử dụng', type: 'number' },
          { name: 'perUserUsageLimit', label: 'Số lượt mỗi khách', type: 'number' },
          { name: 'startDate', label: 'Bắt đầu', type: 'date' },
          { name: 'endDate', label: 'Kết thúc', type: 'date' },
          { name: 'audience', label: 'Đối tượng', type: 'text' },
          { name: 'status', label: 'Trạng thái', type: 'text' }
        ];
      default:
        return [];
    }
  }

  discountTypeOptions(): { label: string; value: string }[] {
    return [
      { label: 'Phần trăm', value: DiscountType.Percent },
      { label: 'Số tiền cố định', value: DiscountType.FixedAmount },
      { label: 'Miễn phí vận chuyển', value: DiscountType.FreeShipping },
      { label: 'Mua X tặng Y', value: DiscountType.BuyXGetY }
    ];
  }

  audienceOptions(): { label: string; value: string }[] {
    return [
      { label: 'Tất cả khách hàng', value: CouponAudience.Public },
      { label: 'Khách VIP', value: CouponAudience.VipOnly },
      { label: 'Khách mới', value: CouponAudience.NewCustomer }
    ];
  }

  statusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Bản nháp', value: CouponStatus.Draft },
      { label: 'Đang hoạt động', value: CouponStatus.Active },
      { label: 'Đã lên lịch', value: CouponStatus.Scheduled },
      { label: 'Tạm dừng', value: CouponStatus.Paused },
      { label: 'Đã hết hạn', value: CouponStatus.Expired }
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
          code: String(values['code'] ?? '').trim(),
          name: String(values['name'] ?? '').trim(),
          description: String(values['description'] ?? '').trim() || undefined,
          discountType: (values['discountType'] as DiscountType) ?? DiscountType.Percent,
          discountValue: Number(values['discountValue'] ?? 0),
          minOrderValue: Number(values['minOrderValue'] ?? 0),
          maxDiscountAmount: Number(values['maxDiscountAmount'] ?? 0),
          usageLimit: Number(values['usageLimit'] ?? 0),
          perUserUsageLimit: Number(values['perUserUsageLimit'] ?? 1),
          startDate: String(values['startDate'] ?? ''),
          endDate: String(values['endDate'] ?? ''),
          audience: (values['audience'] as CouponAudience) ?? CouponAudience.Public,
          status: (values['status'] as CouponStatus) ?? CouponStatus.Draft
        });
      } else if (this.dialogMode() === 'edit' && target) {
        ok = await this.store.update(target.id, {
          name: String(values['name'] ?? '').trim(),
          description: String(values['description'] ?? '').trim(),
          discountType: values['discountType'] as DiscountType,
          discountValue: Number(values['discountValue'] ?? 0),
          minOrderValue: Number(values['minOrderValue'] ?? 0),
          maxDiscountAmount: Number(values['maxDiscountAmount'] ?? 0),
          usageLimit: Number(values['usageLimit'] ?? 0),
          perUserUsageLimit: Number(values['perUserUsageLimit'] ?? 1),
          startDate: String(values['startDate'] ?? ''),
          endDate: String(values['endDate'] ?? ''),
          audience: values['audience'] as CouponAudience,
          status: values['status'] as CouponStatus
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

  copyCode(row: CouponListItemResponse): void {
    void navigator.clipboard?.writeText(row.code);
  }

  askDelete(row: CouponListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('delete');
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `${target.code} - ${target.name}` : '';
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
