import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StorefrontStore } from '../storefront.store';

@Component({
  selector: 'app-shop-cart',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>Giỏ sách của bạn ({{ items().length }} cuốn)</h1>

    <div class="cart">
      <section class="cart__items">
        @for (item of items(); track item.id) {
          <article class="cart-item">
            <div class="cart-item__cover"><i class="pi pi-book"></i></div>
            <div class="cart-item__info">
              <h3>{{ item.title }}</h3>
              <p>{{ item.unitType === 'Rent' ? 'Cho mượn' : 'Bán' }} · {{ item.unitPrice | number }} d</p>
            </div>
            <div class="cart-item__qty">
              <button type="button" (click)="update(item.id, item.quantity - 1)"><i class="pi pi-minus"></i></button>
              <span>{{ item.quantity }}</span>
              <button type="button" (click)="update(item.id, item.quantity + 1)"><i class="pi pi-plus"></i></button>
            </div>
            <strong class="cart-item__total">{{ item.lineTotal | number }} d</strong>
            <button type="button" class="cart-item__remove" (click)="remove(item.id)"><i class="pi pi-trash"></i></button>
          </article>
        } @empty {
          <div class="cart-empty">
            <i class="pi pi-shopping-bag"></i>
            <h3>Giỏ sách đang trống</h3>
            <p>Chưa có cuốn sách nào trong giỏ. Hãy khám phá danh mục để tìm cuốn sách bạn thích.</p>
            <a class="btn btn--primary" routerLink="/shop/catalog"><i class="pi pi-arrow-left"></i> Xem danh mục</a>
          </div>
        }
      </section>

      <aside class="summary">
        <h2>Tóm tắt đơn hàng</h2>
        <div class="summary__row"><span>Tạm tính</span><span>{{ subtotal() | number }} d</span></div>
        <div class="summary__row"><span>Vận chuyển</span><span>{{ shipping() | number }} d</span></div>
        <div class="summary__row summary__row--discount"><span>Mã giảm giá</span><span>-{{ discount() | number }} d</span></div>
        <div class="summary__row summary__row--total"><span>Tổng cộng</span><span>{{ total() | number }} d</span></div>

        <div class="coupon">
          <input type="text" placeholder="Nhập mã giảm giá..." [value]="couponCode()" (input)="couponCode.set($any($event.target).value)" />
          <button type="button" (click)="apply()">Áp dụng</button>
        </div>

        @if (!freeShipping()) {
          <p class="summary__hint">Mua thêm {{ shortfall() | number }} d để được miễn phí vận chuyển.</p>
        }

        <a class="btn btn--primary btn--block" routerLink="/shop/checkout"><i class="pi pi-lock"></i> Thanh toán</a>
        <button type="button" class="btn btn--ghost btn--block" (click)="clear()"><i class="pi pi-trash"></i> Xóa giỏ sách</button>
      </aside>
    </div>
  `,
  styles: [
    `
      .cart { display: grid; grid-template-columns: 1fr 22rem; gap: 1.5rem; margin-top: 1rem; }
      .cart__items { display: flex; flex-direction: column; gap: 0.75rem; }
      .cart-item { display: grid; grid-template-columns: 4rem 1fr auto auto auto; align-items: center; gap: 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .cart-item__cover { width: 4rem; height: 5.5rem; display: grid; place-items: center; background: var(--surface-container-low, #f2f4f6); border-radius: 0.375rem; }
      .cart-item__info h3 { margin: 0 0 0.25rem; font-size: 0.9375rem; }
      .cart-item__info p { margin: 0; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .cart-item__qty { display: inline-flex; align-items: center; gap: 0.5rem; }
      .cart-item__qty button { border: 1px solid var(--outline-variant, #c6c6cd); background: transparent; width: 1.75rem; height: 1.75rem; border-radius: 0.375rem; cursor: pointer; }
      .cart-item__remove { border: none; background: transparent; cursor: pointer; color: var(--error, #ba1a1a); }
      .summary { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1.25rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); align-self: start; }
      .summary h2 { margin: 0 0 1rem; font-size: 1.125rem; }
      .summary__row { display: flex; justify-content: space-between; padding: 0.375rem 0; font-size: 0.875rem; }
      .summary__row--discount { color: var(--secondary, #006c49); }
      .summary__row--total { font-size: 1.125rem; font-weight: 800; border-top: 1px solid var(--outline-variant, #c6c6cd); margin-top: 0.5rem; padding-top: 0.75rem; }
      .coupon { display: flex; gap: 0.375rem; margin: 1rem 0; }
      .coupon input { flex: 1; padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: 1px solid var(--outline-variant, #c6c6cd); }
      .coupon button { border: none; background: var(--tertiary-container, #001a42); color: #fff; padding: 0.5rem 0.875rem; border-radius: 0.5rem; cursor: pointer; font-weight: 600; }
      .summary__hint { font-size: 0.8125rem; color: #a4600a; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.625rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; justify-content: center; gap: 0.375rem; }
      .btn--block { width: 100%; margin-bottom: 0.5rem; }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .cart-empty { text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .cart-empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .cart-empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .cart-empty p { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
    `
  ]
})
export class StorefrontCartPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  readonly couponCode = signal('');

  readonly items = computed(() => this.store.cart().items);
  readonly subtotal = computed(() => this.items().reduce((sum, i) => sum + i.lineTotal, 0));
  readonly discount = computed(() => this.store.cart().discountAmount);
  readonly shipping = computed(() => this.store.cart().shippingAmount);
  readonly total = computed(() => this.subtotal() - this.discount() + this.shipping());
  readonly freeShipping = computed(() => this.subtotal() - this.discount() >= this.store.cart().freeShippingThreshold);
  readonly shortfall = computed(() => Math.max(0, this.store.cart().freeShippingThreshold - (this.subtotal() - this.discount())));

  async ngOnInit(): Promise<void> {
    await this.store.loadCart();
  }

  async update(itemId: string, quantity: number): Promise<void> {
    if (quantity < 1) {
      await this.remove(itemId);
      return;
    }
    await this.store.updateQuantity(itemId, quantity);
  }

  async remove(itemId: string): Promise<void> {
    await this.store.removeItem(itemId);
  }

  async apply(): Promise<void> {
    if (this.couponCode().trim()) {
      await this.store.applyCoupon(this.couponCode().trim().toUpperCase());
    }
  }

  async clear(): Promise<void> {
    for (const item of this.items()) {
      await this.store.removeItem(item.id);
    }
  }
}
