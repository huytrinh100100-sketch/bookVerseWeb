import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { GeographyService } from '../../../shared/services/geography.service';
import { CustomerAddress } from '../storefront.models';
import { StorefrontStore } from '../storefront.store';

type DialogMode = 'create' | 'edit' | null;

/** Trang "Sổ địa chỉ" của khách hàng - them / sua / xoa dia chi. */
@Component({
  selector: 'app-shop-addresses',
  standalone: true,
  imports: [RouterLink, FormDialogComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      <header class="page__head">
        <div>
          <h1>Sổ địa chỉ</h1>
          <p>Quản lý các địa chỉ nhận hàng của bạn.</p>
        </div>
        <button type="button" class="btn btn--primary" (click)="openCreate()"><i class="pi pi-plus"></i> Thêm địa chỉ</button>
      </header>

      <div class="addr-list">
        @for (address of addresses(); track address.id) {
          <article class="addr" [class.addr--default]="address.isDefault">
            <div class="addr__body">
              <div class="addr__top">
                <strong>{{ address.recipientName }}</strong>
                <span class="addr__phone">{{ address.phoneNumber }}</span>
                @if (address.isDefault) {
                  <span class="addr__badge">Mặc định</span>
                }
              </div>
              <p class="addr__text">{{ address.address }}, {{ address.communeName }}, {{ address.provinceName }}</p>
              @if (address.note) {
                <p class="addr__note"><i class="pi pi-comment"></i> {{ address.note }}</p>
              }
            </div>
            <div class="addr__actions">
              <button type="button" class="icon-btn" title="Sửa" (click)="openEdit(address)"><i class="pi pi-pencil"></i></button>
              <button type="button" class="icon-btn icon-btn--danger" title="Xóa" (click)="askDelete(address)"><i class="pi pi-trash"></i></button>
            </div>
          </article>
        } @empty {
          <div class="empty">
            <i class="pi pi-map-marker"></i>
            <h3>Chưa có địa chỉ nào</h3>
            <p>Thêm địa chỉ để đặt hàng nhanh hơn.</p>
            <button type="button" class="btn btn--primary" (click)="openCreate()"><i class="pi pi-plus"></i> Thêm địa chỉ</button>
          </div>
        }
      </div>

      <a class="back-link" routerLink="/shop/account"><i class="pi pi-arrow-left"></i> Quay lại tài khoản</a>
    </div>

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogMode() === 'create' ? 'Thêm địa chỉ' : 'Cập nhật địa chỉ'"
        [subtitle]="formError() || 'Điền đầy đủ thông tin người nhận và địa chỉ.'"
        [fields]="dialogFields()"
        [values]="formValues()"
        mode="edit"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo địa chỉ' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmTarget(); as target) {
      <app-confirm-dialog
        title="Xóa địa chỉ"
        [message]="'Bạn có chắc muốn xóa địa chỉ của ' + target.recipientName + '?'"
        [detail]="target.address + ', ' + target.communeName + ', ' + target.provinceName"
        confirmLabel="Xóa"
        tone="danger"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmTarget.set(null)"
      />
    }
  `,
  styles: [
    `
      .page { max-width: 52rem; margin: 0 auto; }
      .page__head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; margin-bottom: 1.25rem; }
      .page__head h1 { margin: 0 0 0.25rem; font-size: 1.5rem; }
      .page__head p { margin: 0; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .addr-list { display: flex; flex-direction: column; gap: 0.75rem; }
      .addr { display: flex; gap: 1rem; justify-content: space-between; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); border-left: 3px solid transparent; }
      .addr--default { border-left-color: var(--secondary, #006c49); }
      .addr__top { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
      .addr__phone { font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .addr__badge { font-size: 0.6875rem; font-weight: 700; padding: 0.125rem 0.5rem; border-radius: 9999px; background: var(--secondary-container, #6cf8bb); color: #001a42; }
      .addr__text { margin: 0.375rem 0 0; font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .addr__note { margin: 0.25rem 0 0; font-size: 0.8125rem; color: var(--outline, #76777d); }
      .addr__actions { display: flex; gap: 0.25rem; align-items: flex-start; }
      .icon-btn { border: none; background: transparent; cursor: pointer; color: var(--on-surface-variant, #45464d); padding: 0.375rem; border-radius: 0.375rem; }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); }
      .icon-btn--danger { color: var(--error, #ba1a1a); }
      .empty { text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .empty p { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .back-link { display: inline-flex; align-items: center; gap: 0.375rem; margin-top: 1.25rem; color: var(--secondary, #006c49); font-size: 0.875rem; font-weight: 600; text-decoration: none; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.875rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class StorefrontAddressesPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  private readonly geography = inject(GeographyService);

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<CustomerAddress | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);
  readonly confirmTarget = signal<CustomerAddress | null>(null);

  readonly addresses = computed(() => this.store.addresses());

  async ngOnInit(): Promise<void> {
    await this.geography.ensureLoaded();
    await this.store.loadAddresses();
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      recipientName: '',
      phoneNumber: '',
      address: '',
      provinceId: '',
      communeId: '',
      isDefault: false,
      note: ''
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(address: CustomerAddress): void {
    this.dialogTarget.set(address);
    this.formValues.set({
      recipientName: address.recipientName,
      phoneNumber: address.phoneNumber,
      address: address.address,
      provinceId: address.provinceId,
      communeId: address.communeId,
      isDefault: address.isDefault,
      note: address.note ?? ''
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.dialogTarget.set(null);
    this.formError.set('');
  }

  onFormChange(values: Record<string, unknown>): void {
    const previousProvince = this.formValues()['provinceId'];
    const next = { ...values };

    // Đổi tỉnh thì xoá xã/phường đang chọn vì không còn thuộc tỉnh đó.
    if (values['provinceId'] !== previousProvince) {
      next['communeId'] = '';
    }

    this.formValues.set(next);
  }

  dialogFields(): DialogField[] {
    const provinceId = String(this.formValues()['provinceId'] ?? '');

    return [
      { name: 'recipientName', label: 'Người nhận', type: 'text', required: true, placeholder: 'vd: Nguyễn Văn A' },
      { name: 'phoneNumber', label: 'Số điện thoại', type: 'text', required: true, placeholder: 'vd: 0901234567' },
      { name: 'provinceId', label: 'Tỉnh / Thành phố', type: 'select', multiple: false, required: true, options: this.geography.provinceOptions() },
      { name: 'communeId', label: 'Xã / Phường', type: 'select', multiple: false, required: true, options: this.geography.communeOptionsFor(provinceId) },
      { name: 'address', label: 'Số nhà, đường', type: 'text', required: true, placeholder: 'vd: Số 12 Lý Nam Đế, P. Lý Nam Đế' },
      { name: 'note', label: 'Ghi chú', type: 'textarea', placeholder: 'vd: Giao sau 18h' },
      { name: 'isDefault', label: 'Đặt làm địa chỉ mặc định', type: 'checkbox', placeholder: 'Dùng địa chỉ này cho đơn hàng sau' }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      const payload = {
        recipientName: String(values['recipientName'] ?? '').trim(),
        phoneNumber: String(values['phoneNumber'] ?? '').trim(),
        address: String(values['address'] ?? '').trim(),
        provinceId: String(values['provinceId'] ?? ''),
        communeId: String(values['communeId'] ?? ''),
        isDefault: Boolean(values['isDefault']),
        note: String(values['note'] ?? '').trim() || undefined
      };

      if (!payload.recipientName || !payload.phoneNumber || !payload.address || !payload.provinceId || !payload.communeId) {
        this.formError.set('Vui lòng nhập đầy đủ các trường bắt buộc.');
        return;
      }

      const result = this.dialogMode() === 'create'
        ? await this.store.createAddress(payload)
        : target
          ? await this.store.updateAddress(target.id, payload)
          : null;

      if (result) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể lưu địa chỉ. Vui lòng thử lại.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  askDelete(address: CustomerAddress): void {
    this.confirmTarget.set(address);
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    if (!target) {
      return;
    }

    this.submitting.set(true);
    try {
      await this.store.deleteAddress(target.id);
    } finally {
      this.submitting.set(false);
      this.confirmTarget.set(null);
    }
  }
}
