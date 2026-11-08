import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { PartnerListItemResponse, PartnerType } from './partners.models';
import { PartnersStore } from './partners.store';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-partners',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    PageHeaderComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Tác giả &amp; Nhà xuất bản"
      subtitle="Quản lý đối tác và hợp đồng bản quyền cho từng mục sách."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Danh mục' }, { label: 'Tác giả & NXB' }]"
    >
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.AuthorCreate" (click)="openCreate()">
        <i class="pi pi-user-plus"></i> Thêm tác giả / NXB
      </button>
    </app-page-header>

    <div class="tabs">
      <button type="button" [class.tabs__item--active]="tab() === 'all'" (click)="tab.set('all')">Tất cả ({{ rows().length }})</button>
      <button type="button" [class.tabs__item--active]="tab() === PartnerType.Author" (click)="tab.set(PartnerType.Author)">Tác giả ({{ authorCount() }})</button>
      <button type="button" [class.tabs__item--active]="tab() === PartnerType.Publisher" (click)="tab.set(PartnerType.Publisher)">Nhà xuất bản ({{ publisherCount() }})</button>
    </div>

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.AuthorEdit" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.AuthorDeleteTemp" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
    </ng-template>

    <app-data-table [columns]="columns" [rows]="filtered()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo đối tác' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmAction()) {
      <app-confirm-dialog
        title="Xóa mềm đối tác"
        message="Đối tác sẽ bị ẩn khỏi danh sách. Các sách đã xuất bản vẫn giữ nguyên."
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
      .tabs {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 1rem;
        background: var(--surface-container-lowest, #fff);
        border: 1px solid var(--outline-variant, #c6c6cd);
        border-radius: 0.75rem;
        padding: 0.25rem;
        width: fit-content;
      }
      .tabs button {
        border: none;
        background: transparent;
        cursor: pointer;
        padding: 0.5rem 0.875rem;
        border-radius: 0.5rem;
        font-size: 0.875rem;
        font-weight: 600;
        color: var(--on-surface-variant, #45464d);
      }
      .tabs__item--active { background: var(--secondary-container, #6cf8bb); color: var(--on-secondary-container, #002113) !important; }
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
        background: var(--secondary, #006c49);
        color: #fff;
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
    `
  ]
})
export class AdminPartnersPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;
  /** Enum loại đối tác dùng cho template (tab filter). */
  readonly PartnerType = PartnerType;

  readonly store = inject(PartnersStore);
  readonly tab = signal<'all' | PartnerType>('all');

  readonly rows = computed(() => this.store.items());
  readonly authorCount = computed(() => this.rows().filter((p) => p.type === PartnerType.Author).length);
  readonly publisherCount = computed(() => this.rows().filter((p) => p.type === PartnerType.Publisher).length);
  readonly filtered = computed(() => {
    const current = this.tab();
    return current === 'all' ? this.rows() : this.rows().filter((p) => p.type === current);
  });

  readonly columns: TableColumn[] = [
    { field: 'name', header: 'Đối tác' },
    {
      field: 'type',
      header: 'Phân loại',
      type: 'status',
      statusMap: {
        Author: { label: 'Tác giả', severity: 'info' },
        Publisher: { label: 'Nhà xuất bản', severity: 'success' }
      }
    },
    { field: 'country', header: 'Quốc gia' },
    { field: 'email', header: 'Email' },
    { field: 'bookCount', header: 'Số sách', type: 'number' },
    {
      field: 'agreementStatus',
      header: 'Trạng thái hợp đồng',
      type: 'status',
      statusMap: {
        Draft: { label: 'Bản nháp', severity: 'warning' },
        Active: { label: 'Đang hiệu lực', severity: 'success' },
        Expired: { label: 'Đã hết hạn', severity: 'danger' },
        Terminated: { label: 'Đã chấm dứt', severity: 'danger' }
      }
    }
  ];

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<PartnerListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'delete' | null>(null);
  readonly confirmTarget = signal<PartnerListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await this.store.load();
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      name: '',
      type: PartnerType.Author,
      country: '',
      website: '',
      email: '',
      phoneNumber: '',
      biography: '',
      description: '',
      agreementStatus: 'Draft',
      agreementExpiresAt: '',
      isActive: true
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: PartnerListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      name: row.name,
      type: row.type,
      country: row.country ?? '',
      website: row.website ?? '',
      email: row.email ?? '',
      phoneNumber: row.phoneNumber ?? '',
      biography: row.biography ?? '',
      description: row.description ?? '',
      agreementStatus: row.agreementStatus,
      agreementExpiresAt: row.agreementExpiresAt ?? '',
      isActive: row.isActive
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: PartnerListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      code: row.code,
      name: row.name,
      type: row.type,
      country: row.country ?? '',
      website: row.website ?? '',
      email: row.email ?? '',
      phoneNumber: row.phoneNumber ?? '',
      biography: row.biography ?? '',
      description: row.description ?? '',
      bookCount: row.bookCount,
      agreementStatus: row.agreementStatus,
      agreementExpiresAt: row.agreementExpiresAt ?? '',
      isActive: row.isActive
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
      case 'create': return 'Thêm tác giả / Nhà xuất bản';
      case 'edit': return 'Cập nhật đối tác';
      case 'view': return 'Chi tiết đối tác';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật thông tin của ${target.name}.`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã đối tác: ${target.code}.`;
    }
    return 'Tác giả và nhà xuất bản dùng chung một khung hồ sơ.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'name', label: 'Tên', type: 'text', required: true, placeholder: 'vd: Nhà Nam' },
          { name: 'type', label: 'Phân loại', type: 'select', multiple: false, required: true, options: this.typeOptions() },
          { name: 'country', label: 'Quốc gia', type: 'text', placeholder: 'vd: Việt Nam (VN)' },
          { name: 'email', label: 'Email', type: 'text', placeholder: 'vd: info@nhanam.vn' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'website', label: 'Website', type: 'text', placeholder: 'https://...' },
          { name: 'agreementStatus', label: 'Trạng thái hợp đồng', type: 'select', multiple: false, options: this.agreementOptions() },
          { name: 'agreementExpiresAt', label: 'Ngày hết hạn', type: 'date' },
          { name: 'biography', label: 'Tiểu sử', type: 'textarea', placeholder: 'Giới thiệu ngắn về tác giả...' },
          { name: 'description', label: 'Mô tả', type: 'textarea', placeholder: 'Mô tả nhà xuất bản...' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      case 'edit':
        return [
          { name: 'name', label: 'Tên', type: 'text', required: true },
          { name: 'type', label: 'Phân loại', type: 'select', multiple: false, options: this.typeOptions() },
          { name: 'country', label: 'Quốc gia', type: 'text' },
          { name: 'email', label: 'Email', type: 'text' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'website', label: 'Website', type: 'text' },
          { name: 'agreementStatus', label: 'Trạng thái hợp đồng', type: 'select', multiple: false, options: this.agreementOptions() },
          { name: 'agreementExpiresAt', label: 'Ngày hết hạn', type: 'date' },
          { name: 'biography', label: 'Tiểu sử', type: 'textarea' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã đối tác', type: 'text' },
          { name: 'name', label: 'Tên', type: 'text' },
          { name: 'type', label: 'Phân loại', type: 'text' },
          { name: 'country', label: 'Quốc gia', type: 'text' },
          { name: 'email', label: 'Email', type: 'text' },
          { name: 'phoneNumber', label: 'Điện thoại', type: 'text' },
          { name: 'website', label: 'Website', type: 'text' },
          { name: 'bookCount', label: 'Số sách', type: 'number' },
          { name: 'agreementStatus', label: 'Trạng thái hợp đồng', type: 'text' },
          { name: 'agreementExpiresAt', label: 'Ngày hết hạn', type: 'date' },
          { name: 'biography', label: 'Tiểu sử', type: 'textarea' },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'isActive', label: 'Kích hoạt', type: 'checkbox', placeholder: 'Đang hoạt động' }
        ];
      default:
        return [];
    }
  }

  typeOptions(): { label: string; value: string }[] {
    return [
      { label: 'Tác giả', value: PartnerType.Author },
      { label: 'Nhà xuất bản', value: PartnerType.Publisher }
    ];
  }

  agreementOptions(): { label: string; value: string }[] {
    return [
      { label: 'Bản nháp', value: 'Draft' },
      { label: 'Đang hiệu lực', value: 'Active' },
      { label: 'Đã hết hạn', value: 'Expired' },
      { label: 'Đã chấm dứt', value: 'Terminated' }
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
          name: String(values['name'] ?? '').trim(),
          type: (values['type'] as PartnerType) ?? PartnerType.Author,
          country: String(values['country'] ?? '').trim() || undefined,
          email: String(values['email'] ?? '').trim() || undefined,
          phoneNumber: String(values['phoneNumber'] ?? '').trim() || undefined,
          website: String(values['website'] ?? '').trim() || undefined,
          biography: String(values['biography'] ?? '').trim() || undefined,
          description: String(values['description'] ?? '').trim() || undefined,
          agreementStatus: (values['agreementStatus'] as never) ?? 'Draft',
          agreementExpiresAt: String(values['agreementExpiresAt'] ?? '') || undefined,
          isActive: Boolean(values['isActive'])
        });
      } else if (this.dialogMode() === 'edit' && target) {
        ok = await this.store.update(target.id, {
          name: String(values['name'] ?? '').trim(),
          country: String(values['country'] ?? '').trim() || undefined,
          email: String(values['email'] ?? '').trim() || undefined,
          phoneNumber: String(values['phoneNumber'] ?? '').trim() || undefined,
          website: String(values['website'] ?? '').trim() || undefined,
          biography: String(values['biography'] ?? '').trim() || undefined,
          description: String(values['description'] ?? '').trim() || undefined,
          agreementStatus: values['agreementStatus'] as never,
          agreementExpiresAt: String(values['agreementExpiresAt'] ?? '') || undefined,
          isActive: Boolean(values['isActive'])
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

  askDelete(row: PartnerListItemResponse): void {
    this.confirmTarget.set(row);
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
