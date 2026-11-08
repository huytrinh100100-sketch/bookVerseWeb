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
import { OrderListItemResponse, OrderStatus, PaymentMethod } from './orders.models';
import { OrdersStore } from './orders.store';

type DialogMode = 'view' | 'status' | 'cancel' | 'refund' | null;

@Component({
  selector: 'app-admin-orders',
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
      title="Đơn hàng &amp; Xử lý"
      subtitle="Theo dõi đơn hàng từ khi tiếp nhận đến khi giao thành công."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Bán hàng' }, { label: 'Đơn hàng' }]"
    >
      <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.OrderExport"><i class="pi pi-download"></i> Xuất đơn hàng</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tất cả đơn hàng', value: (store.totalItems() | number) + '', icon: 'pi pi-receipt', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Chờ xử lý', value: store.pendingCount() + '', icon: 'pi pi-clock', tone: 'warning' }" />
      <app-stat-card [config]="{ label: 'Đang giao', value: store.processingCount() + '', icon: 'pi pi-truck', tone: 'info' }" />
      <app-stat-card [config]="{ label: 'Đã hoàn thành', value: store.completedCount() + '', icon: 'pi pi-check-circle', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Đã hủy', value: store.cancelledCount() + '', icon: 'pi pi-times', tone: 'danger' }" />
    </div>

    <app-search-filter-bar
      [fields]="filterFields"
      [value]="filterValue()"
      searchPlaceholder="Tìm theo mã đơn hoặc tên khách hàng..."
      (changed)="onFilterChanged($event)"
      (reset)="onReset()"
    />

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Cập nhật trạng thái" *hasPermission="PermissionConstants.OrderProcess" (click)="openStatus(row)"><i class="pi pi-forward"></i></button>
      <button type="button" class="icon-btn" title="Xác nhận đã thanh toán" *hasPermission="PermissionConstants.OrderProcess" (click)="askMarkPaid(row)"><i class="pi pi-wallet"></i></button>
      <button type="button" class="icon-btn icon-btn--danger" title="Hủy đơn hàng" *hasPermission="PermissionConstants.OrderCancel" (click)="openCancel(row)"><i class="pi pi-times"></i></button>
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
        [tone]="confirmAction() === 'cancel' ? 'danger' : 'info'"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmAction.set(null)"
      />
    }
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
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
    `
  ]
})
export class AdminOrdersPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(OrdersStore);

  readonly rows = computed(() => this.store.items());
  readonly filterValue = signal<SearchFilterValue>({ keyword: '', filters: {} });

  readonly filterFields = [
    {
      key: 'orderStatus',
      label: 'Trạng thái đơn',
      type: 'select' as const,
      placeholder: 'Tất cả trạng thái',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Chờ xử lý', value: 'Pending' },
        { label: 'Đã thanh toán', value: 'Paid' },
        { label: 'Đang xử lý', value: 'Processing' },
        { label: 'Đang giao', value: 'Shipped' },
        { label: 'Hoàn thành', value: 'Completed' },
        { label: 'Đã hủy', value: 'Cancelled' },
        { label: 'Đã hoàn tiền', value: 'Refunded' }
      ]
    },
    {
      key: 'paymentMethod',
      label: 'Thanh toán',
      type: 'select' as const,
      placeholder: 'Tất cả phương thức',
      options: [
        { label: 'Tất cả phương thức', value: '' },
        { label: 'COD', value: 'COD' },
        { label: 'MoMo', value: 'MoMo' },
        { label: 'VNPay', value: 'VNPay' },
        { label: 'OnePay', value: 'OnePay' },
        { label: 'Chuyển khoản', value: 'BankTransfer' }
      ]
    }
  ];

  readonly columns: TableColumn[] = [
    { field: 'code', header: 'Mã đơn' },
    { field: 'customerName', header: 'Khách hàng' },
    { field: 'orderDate', header: 'Thời gian', type: 'date' },
    { field: 'itemCount', header: 'Số lượng', type: 'number' },
    { field: 'finalAmount', header: 'Tổng tiền', type: 'money' },
    { field: 'paymentMethod', header: 'Thanh toán' },
    {
      field: 'orderStatus',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Pending: { label: 'Chờ xử lý', severity: 'warning' },
        Paid: { label: 'Đã thanh toán', severity: 'info' },
        Processing: { label: 'Đang xử lý', severity: 'info' },
        Shipped: { label: 'Đang giao', severity: 'info' },
        Completed: { label: 'Hoàn thành', severity: 'success' },
        Cancelled: { label: 'Đã hủy', severity: 'danger' },
        Refunded: { label: 'Đã hoàn tiền', severity: 'danger' }
      }
    }
  ];

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<OrderListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'markPaid' | 'cancel' | null>(null);
  readonly confirmTarget = signal<OrderListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.load();
  }

  onFilterChanged(value: SearchFilterValue): void {
    this.filterValue.set(value);
    void this.store.load({
      keyword: value.keyword || undefined,
      orderStatus: (value.filters['orderStatus'] as OrderStatus) || undefined,
      paymentMethod: (value.filters['paymentMethod'] as PaymentMethod) || undefined
    });
  }

  onReset(): void {
    this.filterValue.set({ keyword: '', filters: {} });
    void this.store.load({ keyword: undefined, orderStatus: undefined, paymentMethod: undefined });
  }

  openView(row: OrderListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      code: row.code,
      customerName: row.customerName,
      customerPhone: row.customerPhone ?? '',
      orderDate: row.orderDate,
      itemCount: row.itemCount,
      totalAmount: row.totalAmount,
      shippingAmount: row.shippingAmount,
      discountAmount: row.discountAmount,
      finalAmount: row.finalAmount,
      paymentMethod: row.paymentMethod,
      orderStatus: row.orderStatus,
      isPaid: row.isPaid,
      shippingAddress: row.shippingAddress ?? '',
      provinceName: row.provinceName ?? '—',
      communeName: row.communeName ?? '—',
      couponCode: row.couponCode ?? '—'
    });
    this.formError.set('');
    this.dialogMode.set('view');
  }

  openStatus(row: OrderListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({ orderStatus: row.orderStatus, staffNote: '' });
    this.formError.set('');
    this.dialogMode.set('status');
  }

  openCancel(row: OrderListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({ reason: '' });
    this.formError.set('');
    this.dialogMode.set('cancel');
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
      case 'view': return 'Chi tiết đơn hàng';
      case 'status': return 'Cập nhật trạng thái đơn';
      case 'cancel': return 'Hủy đơn hàng';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    return target ? `Mã đơn: ${target.code}.` : '';
  }

  dialogSubmitLabel(): string {
    switch (this.dialogMode()) {
      case 'status': return 'Cập nhật trạng thái';
      case 'cancel': return 'Hủy đơn hàng';
      default: return 'Lưu';
    }
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'view':
        return [
          { name: 'code', label: 'Mã đơn', type: 'text' },
          { name: 'customerName', label: 'Khách hàng', type: 'text' },
          { name: 'customerPhone', label: 'Điện thoại', type: 'text' },
          { name: 'orderDate', label: 'Thời gian đặt', type: 'date' },
          { name: 'itemCount', label: 'Số lượng sản phẩm', type: 'number' },
          { name: 'totalAmount', label: 'Tổng tiền hàng', type: 'money' },
          { name: 'shippingAmount', label: 'Phí vận chuyển', type: 'money' },
          { name: 'discountAmount', label: 'Giảm giá', type: 'money' },
          { name: 'finalAmount', label: 'Khách phải trả', type: 'money' },
          { name: 'paymentMethod', label: 'Phương thức thanh toán', type: 'text' },
          { name: 'orderStatus', label: 'Trạng thái', type: 'text' },
          { name: 'isPaid', label: 'Đã thanh toán', type: 'checkbox', placeholder: 'Đã nhận thanh toán' },
          { name: 'shippingAddress', label: 'Địa chỉ giao hàng', type: 'text' },
          { name: 'provinceName', label: 'Tỉnh / thành phố', type: 'text' },
          { name: 'communeName', label: 'Xã / phường', type: 'text' },
          { name: 'couponCode', label: 'Mã voucher', type: 'text' }
        ];
      case 'status':
        return [
          { name: 'orderStatus', label: 'Trạng thái mới', type: 'select', multiple: false, required: true, options: this.statusOptions() },
          { name: 'staffNote', label: 'Ghi chú xử lý', type: 'textarea', placeholder: 'Ghi chú nội bộ về đơn hàng...' }
        ];
      case 'cancel':
        return [
          { name: 'reason', label: 'Lý do hủy', type: 'textarea', required: true, placeholder: 'Nhập lý do hủy đơn hàng...' }
        ];
      default:
        return [];
    }
  }

  statusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Chờ xử lý', value: OrderStatus.Pending },
      { label: 'Đã thanh toán', value: OrderStatus.Paid },
      { label: 'Đang xử lý', value: OrderStatus.Processing },
      { label: 'Đang giao', value: OrderStatus.Shipped },
      { label: 'Hoàn thành', value: OrderStatus.Completed },
      { label: 'Đã hủy', value: OrderStatus.Cancelled },
      { label: 'Đã hoàn tiền', value: OrderStatus.Refunded }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      if (this.dialogMode() === 'status' && target) {
        ok = await this.store.updateStatus(
          target.id,
          (values['orderStatus'] as OrderStatus) ?? target.orderStatus,
          String(values['staffNote'] ?? '').trim() || undefined
        );
      } else if (this.dialogMode() === 'cancel' && target) {
        ok = await this.store.cancel(target.id, String(values['reason'] ?? '').trim());
      }

      if (ok) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể cập nhật đơn hàng. Vui lòng thử lại.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  askMarkPaid(row: OrderListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('markPaid');
  }

  confirmTitle(): string {
    return this.confirmAction() === 'markPaid' ? 'Xác nhận thanh toán' : 'Hủy đơn hàng';
  }

  confirmMessage(): string {
    return this.confirmAction() === 'markPaid'
      ? 'Đơn hàng sẽ được đánh dấu là đã thanh toán.'
      : 'Đơn hàng sẽ bị hủy và tồn kho được hoàn trả.';
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `Đơn ${target.code} - ${target.customerName}` : '';
  }

  confirmLabel(): string {
    return this.confirmAction() === 'markPaid' ? 'Xác nhận' : 'Hủy đơn';
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    const action = this.confirmAction();
    if (!target || !action) {
      return;
    }

    this.submitting.set(true);
    try {
      if (action === 'markPaid') {
        await this.store.markPaid(target.id, { paymentMethod: target.paymentMethod });
      }
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
