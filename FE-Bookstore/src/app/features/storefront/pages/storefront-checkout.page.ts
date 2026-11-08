import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { StorefrontStore } from '../storefront.store';
import { CustomerAddress } from '../storefront.models';
import { PaymentMethod } from '../../admin/orders/orders.models';
import { StorefrontLookupsService } from '../storefront-lookups.service';

@Component({
  selector: 'app-shop-checkout',
  standalone: true,
  imports: [DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="checkout">
      <section class="block">
        <h2>Địa chỉ nhận hàng</h2>
        @for (address of addresses(); track address.id) {
          <label class="address" [class.address--selected]="selectedAddressId() === address.id">
            <input type="radio" name="address" [value]="address.id" [checked]="selectedAddressId() === address.id" (change)="selectAddress(address)" />
            <div>
              <strong>{{ address.recipientName }}</strong> · {{ address.phoneNumber }}
              <p>{{ address.address }}, {{ address.communeName }}, {{ address.provinceName }}</p>
              @if (address.note) {
                <small>{{ address.note }}</small>
              }
            </div>
          </label>
        } @empty {
          <p class="block__empty">
            Chưa có địa chỉ nào. Vui lòng thêm địa chỉ nhận hàng tại
            <a routerLink="/shop/addresses">Sổ địa chỉ</a> trước khi thanh toán.
          </p>
        }
        <a class="btn btn--ghost" routerLink="/shop/addresses"><i class="pi pi-plus"></i> Địa chỉ mới</a>
      </section>

      <section class="block">
        <h2>Đơn vị vận chuyển</h2>
        @for (carrier of carriers(); track carrier.code) {
          <label class="option" [class.option--selected]="carrierCode() === carrier.code">
            <input type="radio" name="carrier" [value]="carrier.code" [checked]="carrierCode() === carrier.code" (change)="carrierCode.set(carrier.code)" />
            <div>
              <strong>{{ carrier.name }}</strong>
              <p>Giao hàng trong khoảng {{ carrier.estimatedDays }} ngày làm việc</p>
            </div>
            <span class="option__price">{{ carrier.price | number }} d</span>
          </label>
        }
      </section>

      <section class="block">
        <h2>Phương thức thanh toán</h2>
        @for (method of paymentMethods(); track method.value) {
          <label class="option" [class.option--selected]="paymentMethod() === method.value">
            <input type="radio" name="payment" [value]="method.value" [checked]="paymentMethod() === method.value" (change)="paymentMethod.set(method.value)" />
            <div>
              <strong>{{ method.label }}</strong>
              <p>{{ method.note }}</p>
            </div>
          </label>
        }
      </section>

      <section class="block">
        <h2>Ghi chú cho đơn hàng</h2>
        <textarea rows="3" placeholder="Ghi chú giao hàng, yêu cầu bảo bì..." [value]="customerNote()" (input)="customerNote.set($any($event.target).value)"></textarea>
      </section>

      <aside class="total">
        <h2>Thanh toán</h2>
        <div class="total__row"><span>Tạm tính</span><span>{{ subtotal() | number }} d</span></div>
        <div class="total__row"><span>Vận chuyển</span><span>{{ shippingAmount() | number }} d</span></div>
        <div class="total__row total__row--discount"><span>Chiết khấu</span><span>-{{ discount() | number }} d</span></div>
        <div class="total__row total__row--grand"><span>Tổng cộng</span><span>{{ grandTotal() | number }} d</span></div>
        <button type="button" class="btn btn--primary btn--block" [disabled]="submitting()" (click)="placeOrder()">
          <i class="pi pi-lock"></i>
          @if (submitting()) {
            Đang xử lý...
          } @else {
            Xác nhận đặt hàng ({{ grandTotal() | number }} d)
          }
        </button>
      </aside>
    </div>
  `,
  styles: [
    `
      .checkout { display: grid; grid-template-columns: 1fr 20rem; gap: 1.5rem; align-items: start; }
      .block { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1.25rem; margin-bottom: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .block h2 { margin: 0 0 0.75rem; font-size: 1.125rem; }
      .address, .option { display: flex; gap: 0.75rem; padding: 0.75rem; border: 1px solid var(--outline-variant, #c6c6cd); border-radius: 0.5rem; margin-bottom: 0.5rem; cursor: pointer; }
      .address--selected, .option--selected { border-color: var(--secondary, #006c49); background: rgb(0 108 73 / 0.06); }
      .address p, .option p { margin: 0.25rem 0 0; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .option__price { margin-left: auto; font-weight: 700; }
      .block textarea { width: 100%; padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: 1px solid var(--outline-variant, #c6c6cd); }
      .block__empty { margin: 0 0 0.75rem; font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .block__empty a { color: var(--secondary, #006c49); font-weight: 600; }
      .total { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1.25rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); position: sticky; top: 5rem; }
      .total h2 { margin: 0 0 0.75rem; font-size: 1.125rem; }
      .total__row { display: flex; justify-content: space-between; padding: 0.375rem 0; font-size: 0.875rem; }
      .total__row--discount { color: var(--secondary, #006c49); }
      .total__row--grand { font-size: 1.125rem; font-weight: 800; border-top: 1px solid var(--outline-variant, #c6c6cd); margin-top: 0.5rem; padding-top: 0.75rem; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.625rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; justify-content: center; gap: 0.375rem; }
      .btn--block { width: 100%; margin-top: 0.75rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
    `
  ]
})
export class StorefrontCheckoutPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  private readonly router = inject(Router);
  private readonly lookups = inject(StorefrontLookupsService);

  readonly selectedAddressId = signal('');
  readonly carrierCode = signal('');
  readonly paymentMethod = signal<PaymentMethod>(PaymentMethod.COD);
  readonly customerNote = signal('');
  readonly submitting = signal(false);

  /** Don vi van chuyen lay tu BE; fallback mac dinh khi khong goi duoc (quyen setting.view). */
  readonly carriers = computed(() => {
    const fromBe = this.lookups.carriers();
    if (fromBe.length > 0) {
      return fromBe;
    }
    return [
      { id: 'GHN', code: 'GHN', name: 'Giao Hàng Nhanh', price: 30000, estimatedDays: 2, isEnabled: true },
      { id: 'GHTK', code: 'GHTK', name: 'Giao Hàng Tiết Kiệm', price: 25000, estimatedDays: 4, isEnabled: true },
      { id: 'VNPOST', code: 'VNPOST', name: 'Bưu điện Việt Nam', price: 20000, estimatedDays: 6, isEnabled: true }
    ];
  });

  readonly paymentMethods = computed(() => {
    const fromBe = this.lookups.gateways();
    const notes: Record<string, string> = {
      MOMO: 'Quét QR hoặc áp dụng tự động. Khuyến nghị cho Việt Nam.',
      VNPAY: 'Quét từ Vietcombank, Techcombank, BIDV và 30+ ứng dụng.',
      ONEPAY: 'Visa, Mastercard, JCB với xác thực EMV 3D Secure 2.0.',
      COD: 'Kiểm tra bản sao lưu trang trước khi thanh toán cho đơn vận.'
    };
    const labels: Record<string, string> = {
      MOMO: 'MoMo E-Wallet',
      VNPAY: 'VNPay QR Gateway',
      ONEPAY: 'Thẻ tín dụng (OnePay)',
      COD: 'Thanh toán khi nhận hàng'
    };

    const fromApi = fromBe.map((g) => ({
      value: g.code as PaymentMethod,
      label: g.name || labels[g.code] || g.code,
      note: notes[g.code] ?? ''
    }));

    return fromApi.length > 0
      ? fromApi
      : [
          { value: PaymentMethod.COD, label: 'Thanh toán khi nhận hàng', note: notes['COD'] },
          { value: PaymentMethod.MoMo, label: 'MoMo E-Wallet', note: notes['MOMO'] },
          { value: PaymentMethod.VNPay, label: 'VNPay QR Gateway', note: notes['VNPAY'] },
          { value: PaymentMethod.OnePay, label: 'Thẻ tín dụng (OnePay)', note: notes['ONEPAY'] }
        ];
  });

  readonly addresses = computed(() => this.store.addresses());
  readonly subtotal = computed(() => this.store.cart().items.reduce((sum, i) => sum + i.lineTotal, 0));
  readonly discount = computed(() => this.store.cart().discountAmount);
  readonly shippingAmount = computed(() => this.carriers().find((c) => c.code === this.carrierCode())?.price ?? 0);
  readonly grandTotal = computed(() => this.subtotal() - this.discount() + this.shippingAmount());

  async ngOnInit(): Promise<void> {
    await this.store.loadAddresses();
    await this.store.loadCart();
    await this.lookups.ensureLoaded();

    const first = this.addresses()[0];
    if (first) {
      this.selectedAddressId.set(first.id);
    }

    if (!this.carrierCode()) {
      this.carrierCode.set(this.carriers()[0]?.code ?? '');
    }

    if (!this.paymentMethod()) {
      this.paymentMethod.set(this.paymentMethods()[0]?.value ?? PaymentMethod.COD);
    }
  }

  selectAddress(address: CustomerAddress): void {
    this.selectedAddressId.set(address.id);
  }

  async placeOrder(): Promise<void> {
    if (this.submitting()) {
      return;
    }

    if (this.store.cart().items.length === 0) {
      return;
    }

    this.submitting.set(true);
    try {
      const order = await this.store.checkout({
        addressId: this.selectedAddressId(),
        carrierCode: this.carrierCode(),
        shippingAmount: this.shippingAmount(),
        paymentMethod: this.paymentMethod(),
        couponCode: this.store.cart().couponCode ?? undefined,
        customerNote: this.customerNote(),
        details: this.store.cart().items.map((i) => ({ bookId: i.bookId, unitType: i.unitType, quantity: i.quantity }))
      });
      if (order) {
        await this.router.navigate(['/shop/order-success', order.code]);
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
