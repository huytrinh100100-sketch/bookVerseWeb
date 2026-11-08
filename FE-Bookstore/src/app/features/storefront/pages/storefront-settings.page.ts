import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { GeographyService } from '../../../shared/services/geography.service';
import { ProfileApiService, ProfileResponse } from '../../../shared/services/profile-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { StorefrontStore } from '../storefront.store';

type DialogMode = 'profile' | 'password' | null;

/** Trang "Cai dat tai khoan" cua khach hang - sua ho so + doi mat khau. */
@Component({
  selector: 'app-shop-settings',
  standalone: true,
  imports: [RouterLink, DatePipe, FormDialogComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      <header class="page__head">
        <div>
          <h1>Cài đặt tài khoản</h1>
          <p>Cập nhật thông tin cá nhân và bảo mật tài khoản.</p>
        </div>
      </header>

      @if (profile(); as p) {
        <section class="card">
          <h2>Thông tin cá nhân</h2>
          <dl class="info">
            <div><dt>Tên đăng nhập</dt><dd>{{ p.userName }}</dd></div>
            <div><dt>Họ và tên</dt><dd>{{ p.fullName }}</dd></div>
            <div><dt>Email</dt><dd>{{ p.email || '(chưa cập nhật)' }}</dd></div>
            <div><dt>Số điện thoại</dt><dd>{{ p.phoneNumber || '(chưa cập nhật)' }}</dd></div>
            <div><dt>Địa chỉ</dt><dd>{{ p.address || '(chưa cập nhật)' }}</dd></div>
            <div><dt>Hạng thành viên</dt><dd>{{ p.vipTier }} · {{ p.rewardPoints }} điểm</dd></div>
            <div><dt>Ngày tham gia</dt><dd>{{ p.createdAt | date }}</dd></div>
          </dl>
          <button type="button" class="btn btn--primary" (click)="openProfile()"><i class="pi pi-pencil"></i> Sửa hồ sơ</button>
        </section>
      }

      <section class="card">
        <h2>Bảo mật</h2>
        <p class="card__desc">Đổi mật khẩu thường xuyên để bảo vệ tài khoản của bạn.</p>
        <button type="button" class="btn btn--ghost" (click)="openPassword()"><i class="pi pi-key"></i> Đổi mật khẩu</button>
      </section>

      <a class="back-link" routerLink="/shop/account"><i class="pi pi-arrow-left"></i> Quay lại tài khoản</a>
    </div>

    @if (dialogMode() === 'profile') {
      <app-form-dialog
        title="Sửa hồ sơ"
        subtitle="Cập nhật thông tin cá nhân của bạn."
        [fields]="profileFields()"
        [values]="formValues()"
        mode="edit"
        submitLabel="Lưu thay đổi"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="saveProfile()"
        (cancelled)="closeDialog()"
      />
    }

    @if (dialogMode() === 'password') {
      <app-form-dialog
        title="Đổi mật khẩu"
        subtitle="Nhập mật khẩu hiện tại và mật khẩu mới."
        [fields]="passwordFields()"
        [values]="formValues()"
        mode="edit"
        submitLabel="Đổi mật khẩu"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="savePassword()"
        (cancelled)="closeDialog()"
      />
    }
  `,
  styles: [
    `
      .page { max-width: 44rem; margin: 0 auto; }
      .page__head { margin-bottom: 1.25rem; }
      .page__head h1 { margin: 0 0 0.25rem; font-size: 1.5rem; }
      .page__head p { margin: 0; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .card { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1.25rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); margin-bottom: 1rem; }
      .card h2 { margin: 0 0 0.75rem; font-size: 1.125rem; }
      .card__desc { margin: 0 0 0.875rem; font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .info { display: grid; grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr)); gap: 0.875rem; margin: 0 0 1rem; }
      .info dt { font-size: 0.6875rem; text-transform: uppercase; letter-spacing: 0.03em; color: var(--on-surface-variant, #45464d); margin-bottom: 0.125rem; }
      .info dd { margin: 0; font-size: 0.9375rem; font-weight: 600; }
      .back-link { display: inline-flex; align-items: center; gap: 0.375rem; margin-top: 0.5rem; color: var(--secondary, #006c49); font-size: 0.875rem; font-weight: 600; text-decoration: none; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.875rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
    `
  ]
})
export class StorefrontSettingsPageComponent implements OnInit {
  private readonly api = inject(ProfileApiService);
  private readonly geography = inject(GeographyService);
  private readonly toast = inject(ToastService);
  readonly store = inject(StorefrontStore);

  readonly profile = signal<ProfileResponse | null>(null);
  readonly dialogMode = signal<DialogMode>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly provinceOptions = computed(() => this.geography.provinceOptions());

  async ngOnInit(): Promise<void> {
    await this.geography.ensureLoaded();
    await this.loadProfile();
  }

  async loadProfile(): Promise<void> {
    const response = await firstValueFrom(this.api.get());
    this.profile.set(response.data ?? null);
  }

  openProfile(): void {
    const p = this.profile();
    this.formValues.set({
      fullName: p?.fullName ?? '',
      phoneNumber: p?.phoneNumber ?? '',
      address: p?.address ?? '',
      provinceId: p?.provinceId ?? '',
      communeId: p?.communeId ?? '',
      birthday: p?.birthday ? p.birthday.slice(0, 10) : ''
    });
    this.formError.set('');
    this.dialogMode.set('profile');
  }

  openPassword(): void {
    this.formValues.set({ currentPassword: '', newPassword: '', confirmPassword: '' });
    this.formError.set('');
    this.dialogMode.set('password');
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.formError.set('');
  }

  onFormChange(values: Record<string, unknown>): void {
    const previousProvince = this.formValues()['provinceId'];
    const next = { ...values };

    if (values['provinceId'] !== previousProvince) {
      next['communeId'] = '';
    }

    this.formValues.set(next);
  }

  profileFields(): DialogField[] {
    const provinceId = String(this.formValues()['provinceId'] ?? '');

    return [
      { name: 'fullName', label: 'Họ và tên', type: 'text', required: true },
      { name: 'phoneNumber', label: 'Số điện thoại', type: 'text', placeholder: 'vd: 0901234567' },
      { name: 'provinceId', label: 'Tỉnh / Thành phố', type: 'select', multiple: false, options: this.provinceOptions() },
      { name: 'communeId', label: 'Xã / Phường', type: 'select', multiple: false, options: this.geography.communeOptionsFor(provinceId) },
      { name: 'address', label: 'Địa chỉ', type: 'text', placeholder: 'vd: Số 12 Lý Nam Đế' },
      { name: 'birthday', label: 'Ngày sinh', type: 'date' }
    ];
  }

  passwordFields(): DialogField[] {
    return [
      { name: 'currentPassword', label: 'Mật khẩu hiện tại', type: 'password', required: true },
      { name: 'newPassword', label: 'Mật khẩu mới', type: 'password', required: true, hint: 'Tối thiểu 6 ký tự, có chữ hoa, chữ thường và số.' },
      { name: 'confirmPassword', label: 'Xác nhận mật khẩu mới', type: 'password', required: true }
    ];
  }

  async saveProfile(): Promise<void> {
    const values = this.formValues();
    this.formError.set('');
    this.submitting.set(true);

    try {
      const response = await firstValueFrom(
        this.api.update({
          fullName: String(values['fullName'] ?? '').trim(),
          phoneNumber: String(values['phoneNumber'] ?? '').trim(),
          address: String(values['address'] ?? '').trim(),
          provinceId: String(values['provinceId'] ?? '') || null,
          communeId: String(values['communeId'] ?? '') || null,
          birthday: String(values['birthday'] ?? '') || null
        })
      );

      if (response.isSuccess) {
        this.toast.success('Cập nhật hồ sơ thành công.');
        await this.loadProfile();
        await this.store.loadProfile();
        this.closeDialog();
      } else {
        this.formError.set(response.message || 'Không thể cập nhật hồ sơ.');
      }
    } catch {
      this.formError.set('Không thể cập nhật hồ sơ. Vui lòng thử lại.');
    } finally {
      this.submitting.set(false);
    }
  }

  async savePassword(): Promise<void> {
    const values = this.formValues();
    const currentPassword = String(values['currentPassword'] ?? '');
    const newPassword = String(values['newPassword'] ?? '');
    const confirmPassword = String(values['confirmPassword'] ?? '');

    this.formError.set('');

    if (!currentPassword || !newPassword) {
      this.formError.set('Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.');
      return;
    }

    if (newPassword !== confirmPassword) {
      this.formError.set('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.submitting.set(true);
    try {
      const response = await firstValueFrom(this.api.changePassword({ currentPassword, newPassword }));

      if (response.isSuccess) {
        this.toast.success('Đổi mật khẩu thành công.');
        this.closeDialog();
      } else {
        this.formError.set(response.message || 'Không thể đổi mật khẩu.');
      }
    } catch {
      this.formError.set('Không thể đổi mật khẩu. Vui lòng thử lại.');
    } finally {
      this.submitting.set(false);
    }
  }
}
