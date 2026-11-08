import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-shop-order-success',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="success">
      <i class="pi pi-check-circle"></i>
      <h1>Cam on ban, {{ customerName() }}!</h1>
      <p>Đơn hàng của bạn đã được tiếp nhận.</p>
      <p class="success__code">Mã đơn hàng: <strong>{{ orderCode() }}</strong></p>

      <div class="success__actions">
        <button type="button" class="btn btn--ghost"><i class="pi pi-copy"></i> Sao chép mã AWB</button>
        <button type="button" class="btn btn--ghost"><i class="pi pi-receipt"></i> Tải hóa đơn điện tử</button>
        <button type="button" class="btn btn--ghost"><i class="pi pi-description"></i> Xuất phiếu thủ viện</button>
      </div>

      <a class="btn btn--primary" routerLink="/shop/catalog">Tiếp tục khám phá</a>
    </div>
  `,
  styles: [
    `
      .success { text-align: center; padding: 3rem 1rem; }
      .success i { font-size: 3rem; color: var(--secondary, #006c49); }
      .success__code { font-size: 1.125rem; }
      .success__actions { display: flex; gap: 0.5rem; justify-content: center; flex-wrap: wrap; margin: 1.5rem 0; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.625rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; text-decoration: none; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class StorefrontOrderSuccessPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthStore);

  readonly orderCode = signal('');
  readonly customerName = signal('Trần Bảo Linh');

  ngOnInit(): void {
    this.orderCode.set(this.route.snapshot.paramMap.get('code') ?? 'BK-1041');
    const displayName = this.auth.displayName();
    if (displayName) {
      this.customerName.set(displayName);
    }
  }
}
