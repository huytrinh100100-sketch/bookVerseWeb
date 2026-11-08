import { DecimalPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { ShopCoupon, StorefrontApiService } from '../storefront-api.service';
import { StorefrontStore } from '../storefront.store';

/** Trang "So tay diem thuong & voucher" cua khach hang. */
@Component({
  selector: 'app-shop-rewards',
  standalone: true,
  imports: [RouterLink, DecimalPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      <header class="page__head">
        <div>
          <h1>Sổ tay điểm thưởng & voucher</h1>
          <p>Sử dụng điểm thưởng và mã giảm giá khi thanh toán.</p>
        </div>
      </header>

      <section class="points">
        <div class="points__box">
          <span class="points__label">Điểm thưởng hiện có</span>
          <strong class="points__value">{{ profile()?.rewardPoints ?? 0 }}</strong>
          <span class="points__tier">Hạng: {{ profile()?.vipTier ?? 'Standard' }}</span>
        </div>
        <div class="points__box">
          <span class="points__label">Số địa chỉ đã lưu</span>
          <strong class="points__value">{{ profile()?.addressCount ?? 0 }}</strong>
          <a routerLink="/shop/addresses" class="points__link">Quản lý địa chỉ</a>
        </div>
      </section>

      <h2 class="section-title">Voucher đang hiệu lực</h2>
      <div class="coupons">
        @for (coupon of coupons(); track coupon.id) {
          <article class="coupon">
            <div class="coupon__head">
              <span class="coupon__code">{{ coupon.code }}</span>
              <span class="coupon__value">
                @if (coupon.discountType === 'Percent') {
                  {{ coupon.discountValue }}%
                } @else {
                  {{ coupon.discountValue | number }} d
                }
              </span>
            </div>
            <h3 class="coupon__name">{{ coupon.name }}</h3>
            <p class="coupon__min">Đơn tối thiểu {{ coupon.minOrderValue | number }} d</p>
            @if (coupon.maxDiscountAmount) {
              <p class="coupon__min">Giảm tối đa {{ coupon.maxDiscountAmount | number }} d</p>
            }
            @if (coupon.endDate) {
              <p class="coupon__min">Hết hạn {{ coupon.endDate | date }}</p>
            }
            <button type="button" class="btn btn--primary btn--block" (click)="copyCode(coupon.code)">
              <i class="pi pi-copy"></i> Sao chép mã
            </button>
          </article>
        } @empty {
          <div class="empty">
            <i class="pi pi-ticket"></i>
            <h3>Chưa có voucher nào</h3>
            <p>Hiện chưa có voucher đang hiệu lực. Hãy theo dõi các chương trình khuyến mại sắp tới.</p>
            <a class="btn btn--primary" routerLink="/shop/catalog"><i class="pi pi-arrow-left"></i> Xem danh mục</a>
          </div>
        }
      </div>

      <a class="back-link" routerLink="/shop/account"><i class="pi pi-arrow-left"></i> Quay lại tài khoản</a>
    </div>
  `,
  styles: [
    `
      .page { max-width: 60rem; margin: 0 auto; }
      .page__head { margin-bottom: 1.25rem; }
      .page__head h1 { margin: 0 0 0.25rem; font-size: 1.5rem; }
      .page__head p { margin: 0; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .points { display: grid; grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr)); gap: 0.75rem; margin-bottom: 1.5rem; }
      .points__box { background: var(--secondary-container, #6cf8bb); border-radius: 0.75rem; padding: 1.25rem; display: flex; flex-direction: column; gap: 0.25rem; }
      .points__label { font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.04em; color: #001a42; }
      .points__value { font-size: 2rem; line-height: 1.1; color: #001a42; }
      .points__tier { font-size: 0.8125rem; color: #001a42; }
      .points__link { font-size: 0.8125rem; font-weight: 600; color: #001a42; }
      .section-title { font-size: 1.125rem; margin: 0 0 0.75rem; }
      .coupons { display: grid; grid-template-columns: repeat(auto-fill, minmax(15rem, 1fr)); gap: 0.75rem; }
      .coupon { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); border-top: 3px solid var(--secondary, #006c49); display: flex; flex-direction: column; gap: 0.25rem; }
      .coupon__head { display: flex; align-items: center; justify-content: space-between; gap: 0.5rem; }
      .coupon__code { font-family: monospace; font-weight: 700; color: var(--secondary, #006c49); }
      .coupon__value { font-size: 1.125rem; font-weight: 800; }
      .coupon__name { margin: 0.25rem 0 0; font-size: 0.9375rem; }
      .coupon__min { margin: 0; font-size: 0.75rem; color: var(--on-surface-variant, #45464d); }
      .coupon .btn { margin-top: auto; }
      .empty { grid-column: 1 / -1; text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .empty p { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .back-link { display: inline-flex; align-items: center; gap: 0.375rem; margin-top: 1.25rem; color: var(--secondary, #006c49); font-size: 0.875rem; font-weight: 600; text-decoration: none; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.75rem; cursor: pointer; font-size: 0.8125rem; font-weight: 600; display: inline-flex; align-items: center; justify-content: center; gap: 0.375rem; }
      .btn--block { width: 100%; margin-top: 0.75rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class StorefrontRewardsPageComponent implements OnInit {
  private readonly api = inject(StorefrontApiService);
  private readonly toast = inject(ToastService);
  readonly store = inject(StorefrontStore);

  readonly coupons = signal<ShopCoupon[]>([]);
  readonly profile = computed(() => this.store.profile());

  async ngOnInit(): Promise<void> {
    await this.store.loadProfile();
    await this.loadCoupons();
  }

  async loadCoupons(): Promise<void> {
    try {
      const response = await firstValueFrom(this.api.getCoupons());
      this.coupons.set(response.data ?? []);
    } catch {
      this.coupons.set([]);
    }
  }

  async copyCode(code: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(code);
      this.toast.success(`Đã sao chép mã ${code}.`);
    } catch {
      this.toast.info(`Mã voucher: ${code}`);
    }
  }
}
