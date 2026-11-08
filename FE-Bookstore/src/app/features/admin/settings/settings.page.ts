import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { SettingsStore } from './settings.store';
import { StoreSettingResponse } from './settings.models';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [FormsModule, DecimalPipe, NgClass, PageHeaderComponent, HasPermissionDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Cấu hình cửa hàng &amp; Hệ thống"
      subtitle="Thông tin thương hiệu, cổng thanh toán, đơn vị vận chuyển và cảnh báo."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Quản trị' }, { label: 'Cấu hình' }]"
    >
      <button type="button" class="btn btn--ghost" (click)="reload()"><i class="pi pi-refresh"></i> Tải lại</button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.SettingEdit" (click)="save()">
        <i class="pi pi-check"></i> Lưu thay đổi
      </button>
    </app-page-header>

    <div class="tabs">
      @for (tab of tabs; track tab.id) {
        <button type="button" [class.tabs__item--active]="activeTab() === tab.id" (click)="activeTab.set(tab.id)">
          <i class="pi" [ngClass]="tab.icon"></i> {{ tab.label }}
        </button>
      }
    </div>

    <section class="panel">
      <h2 class="panel__title">Thông tin thương hiệu &amp; địa lý</h2>
      <div class="form-grid">
        <label><span>Tên thương hiệu *</span><input [ngModel]="form().storeBrandName" (ngModelChange)="patch('storeBrandName', $event)" /></label>
        <label><span>Email hỗ trợ</span><input [ngModel]="form().supportEmail" (ngModelChange)="patch('supportEmail', $event)" /></label>
        <label><span>Tổng đài</span><input [ngModel]="form().hotline" (ngModelChange)="patch('hotline', $event)" /></label>
        <label><span>Trụ sở &amp; kho xuất phát</span><input [ngModel]="form().headquartersAddress" (ngModelChange)="patch('headquartersAddress', $event)" /></label>
        <label><span>Đơn vị tiền tệ</span><input [ngModel]="form().defaultCurrency" (ngModelChange)="patch('defaultCurrency', $event)" /></label>
        <label><span>Múi giờ vận hành</span><input [ngModel]="form().operatingTimezone" (ngModelChange)="patch('operatingTimezone', $event)" /></label>
        <label><span>Ngưỡng miễn phí vận chuyển</span><input type="number" [ngModel]="form().freeShippingThreshold" (ngModelChange)="patch('freeShippingThreshold', $event)" /></label>
        <label><span>Thời gian nhàn trạng (phút)</span><input type="number" [ngModel]="form().staffInactivityTimeoutMinutes" (ngModelChange)="patch('staffInactivityTimeoutMinutes', $event)" /></label>
        <label class="form-grid__full"><span>Thông báo bảo trì</span><textarea rows="3" [ngModel]="form().maintenanceNotice" (ngModelChange)="patch('maintenanceNotice', $event)"></textarea></label>
      </div>
    </section>

    <section class="panel">
      <h2 class="panel__title">Cổng thanh toán</h2>
      <div class="list">
        @for (gateway of store.paymentGateways(); track gateway.id) {
          <div class="list__row">
            <span>{{ gateway.name }}</span>
            <span class="list__meta">{{ gateway.provider }}</span>
            <span class="list__meta">{{ gateway.isEnabled ? 'Đang bật' : 'Đang tắt' }}</span>
          </div>
        } @empty {
          <p class="list__empty">Chưa cấu hình cổng thanh toán nào.</p>
        }
      </div>
    </section>

    <section class="panel">
      <h2 class="panel__title">Đơn vị vận chuyển</h2>
      <div class="list">
        @for (carrier of store.carriers(); track carrier.id) {
          <div class="list__row">
            <span>{{ carrier.name }}</span>
            <span class="list__meta">{{ carrier.price | number }} đ</span>
            <span class="list__meta">{{ carrier.estimatedDays }} ngày</span>
          </div>
        } @empty {
          <p class="list__empty">Chưa cấu hình đơn vị vận chuyển nào.</p>
        }
      </div>
    </section>
  `,
  styles: [
    `
      .tabs {
        display: flex;
        gap: 0.25rem;
        margin-bottom: 1rem;
        flex-wrap: wrap;
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
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
      .tabs__item--active { background: var(--secondary-container, #6cf8bb); color: var(--on-secondary-container, #002113) !important; }
      .panel {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        padding: 1.25rem;
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
        margin-bottom: 1.5rem;
      }
      .panel__title { margin: 0 0 1rem; font-size: 1.0625rem; font-weight: 600; }
      .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(18rem, 1fr)); gap: 1rem; }
      .form-grid label { display: flex; flex-direction: column; gap: 0.25rem; }
      .form-grid label span { font-size: 0.6875rem; text-transform: uppercase; letter-spacing: 0.03em; font-weight: 600; color: var(--on-surface-variant, #45464d); }
      .form-grid input, .form-grid textarea { padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: 1px solid var(--outline-variant, #c6c6cd); font-family: inherit; }
      .form-grid__full { grid-column: 1 / -1; }
      .list__row { display: flex; justify-content: space-between; gap: 1rem; padding: 0.75rem 0; border-bottom: 1px solid var(--surface-container-high, #e6e8ea); font-size: 0.875rem; }
      .list__meta { color: var(--on-surface-variant, #45464d); font-size: 0.8125rem; }
      .list__empty { color: var(--outline, #76777d); font-size: 0.875rem; padding: 1rem; text-align: center; margin: 0; }
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
export class AdminSettingsPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(SettingsStore);
  readonly activeTab = signal('general');
  readonly form = signal<StoreSettingResponse>(this.store.store() ?? ({} as StoreSettingResponse));

  readonly tabs = [
    { id: 'general', label: 'Tổng quan & Thương hiệu', icon: 'pi pi-shopping-bag' },
    { id: 'payment', label: 'Cổng thanh toán', icon: 'pi pi-wallet' },
    { id: 'shipping', label: 'Đơn vị vận chuyển', icon: 'pi pi-truck' },
    { id: 'alerts', label: 'Cảnh báo & Thông báo', icon: 'pi pi-bell' },
    { id: 'security', label: 'Bảo mật & API', icon: 'pi pi-shield' }
  ];

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  async reload(): Promise<void> {
    await this.store.loadAll();
    if (this.store.store()) {
      this.form.set(this.store.store()!);
    }
  }

  async save(): Promise<void> {
    await this.store.save(this.form());
  }

  patch<K extends keyof StoreSettingResponse>(key: K, value: StoreSettingResponse[K]): void {
    this.form.update((current) => ({ ...current, [key]: value }));
  }
}
