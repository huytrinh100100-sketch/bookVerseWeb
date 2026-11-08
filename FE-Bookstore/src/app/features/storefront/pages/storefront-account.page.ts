import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { ToastService } from '../../../core/services/toast.service';
import { OrderStatus } from '../../admin/orders/orders.models';
import { ShopOrderDetail, ShopOrderSummary } from '../storefront.models';
import { StorefrontStore } from '../storefront.store';

type DialogMode = 'detail' | 'review' | null;

/** Trang "Tai khoan" cua khach hang: lich su don hang + hanh dong day du. */
@Component({
  selector: 'app-shop-account',
  standalone: true,
  imports: [RouterLink, DecimalPipe, DatePipe, FormDialogComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="account">
      <aside class="profile">
        <div class="profile__avatar"><i class="pi pi-user"></i></div>
        <h2>{{ profile()?.fullName ?? 'Khách hàng' }}</h2>
        <p class="profile__email">{{ profile()?.email ?? '(chưa cập nhật)' }}</p>
        <span class="profile__vip">{{ profile()?.vipTier ?? 'Standard' }} VIP · {{ profile()?.rewardPoints ?? 0 }} điểm</span>

        <nav class="profile__nav">
          <a routerLink="/shop/account" class="profile__link--active"><i class="pi pi-receipt"></i> Lịch sử đơn hàng ({{ orders().length }})</a>
          <a routerLink="/shop/wishlist"><i class="pi pi-bookmark"></i> Tủ sách & Yêu thích ({{ profile()?.wishlistBookCount ?? 0 }})</a>
          <a routerLink="/shop/addresses"><i class="pi pi-map-marker"></i> Sổ địa chỉ ({{ profile()?.addressCount ?? 0 }})</a>
          <a routerLink="/shop/rewards"><i class="pi pi-star"></i> Sổ tay điểm thưởng & voucher</a>
          <a routerLink="/shop/settings"><i class="pi pi-cog"></i> Cài đặt tài khoản</a>
        </nav>
      </aside>

      <section class="orders">
        <div class="tabs">
          @for (tab of tabs; track tab.value) {
            <button type="button" [class.tabs__item--active]="activeTab() === tab.value" (click)="activeTab.set(tab.value)">
              {{ tab.label }} ({{ countFor(tab.value) }})
            </button>
          }
        </div>

        @for (order of filteredOrders(); track order.id) {
          <article class="order">
            <header class="order__head">
              <div>
                <strong>{{ order.code }}</strong>
                <span class="order__date">{{ order.orderDate | date }}</span>
              </div>
              <span class="order__status">{{ statusLabel(order.orderStatus) }}</span>
            </header>
            <p class="order__items">{{ order.previewTitles.join(' · ') }}@if (order.itemCount > 1) { (+{{ order.itemCount - 1 }}) }</p>
            <footer class="order__foot">
              <span class="order__total">{{ order.finalAmount | number }} d</span>
              <div class="order__actions">
                <button type="button" class="btn btn--ghost" (click)="openDetail(order)"><i class="pi pi-receipt"></i> Chi tiết hóa đơn</button>
                <button type="button" class="btn btn--ghost" (click)="track(order)"><i class="pi pi-location-searching"></i> Theo dõi đơn</button>
                @if (canCancel(order)) {
                  <button type="button" class="btn btn--danger" (click)="askCancel(order)"><i class="pi pi-times"></i> Hủy đơn</button>
                }
                @if (canReview(order)) {
                  <button type="button" class="btn btn--link" (click)="openReview(order)"><i class="pi pi-star"></i> Viết đánh giá</button>
                }
              </div>
            </footer>
          </article>
        } @empty {
          <div class="empty">
            <i class="pi pi-receipt"></i>
            <h3>Chưa có đơn hàng nào</h3>
            <p>Các đơn hàng của bạn sẽ hiển thị tại đây sau khi đặt hàng.</p>
            <a class="btn btn--primary" routerLink="/shop/catalog"><i class="pi pi-arrow-left"></i> Mua sách</a>
          </div>
        }
      </section>
    </div>

    @if (dialogMode() === 'detail' && detail(); as d) {
      <app-form-dialog
        [title]="'Chi tiết đơn ' + d.code"
        [subtitle]="'Đặt ngày ' + (d.orderDate | date) + ' · ' + statusLabel(d.orderStatus)"
        [fields]="detailFields()"
        [values]="detailValues()"
        mode="view"
        submitLabel="Đóng"
        (cancelled)="closeDialog()"
      />
    }

    @if (dialogMode() === 'review') {
      <app-form-dialog
        title="Viết đánh giá"
        [subtitle]="reviewTarget()?.code ? 'Đánh giá cho đơn ' + reviewTarget()?.code : ''"
        [fields]="reviewFields()"
        [values]="formValues()"
        mode="edit"
        submitLabel="Gửi đánh giá"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="submitReview()"
        (cancelled)="closeDialog()"
      />
    }

    @if (cancelTarget(); as target) {
      <app-confirm-dialog
        title="Hủy đơn hàng"
        [message]="'Bạn có chắc muốn hủy đơn ' + target.code + '?'"
        detail="Sau khi hủy, tồn kho sẽ được hoàn trả. Bạn không thể hủy sau khi đơn đã thanh toán."
        confirmLabel="Hủy đơn"
        tone="danger"
        [submitting]="submitting()"
        (confirmed)="runCancel()"
        (cancelled)="cancelTarget.set(null)"
      />
    }
  `,
  styles: [
    `
      .account { display: grid; grid-template-columns: 18rem 1fr; gap: 1.5rem; align-items: start; }
      .profile { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1.25rem; text-align: center; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .profile__avatar { width: 4rem; height: 4rem; border-radius: 9999px; background: var(--tertiary-fixed, #d8e2ff); display: grid; place-items: center; margin: 0 auto 0.75rem; font-size: 1.5rem; }
      .profile h2 { margin: 0 0 0.25rem; font-size: 1.125rem; }
      .profile__email { margin: 0 0 0.5rem; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .profile__vip { display: inline-block; font-size: 0.75rem; font-weight: 700; padding: 0.125rem 0.625rem; border-radius: 9999px; background: #ffe3b3; color: #7a4b00; }
      .profile__nav { display: flex; flex-direction: column; gap: 0.25rem; margin-top: 1.25rem; text-align: left; }
      .profile__nav a { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem; border-radius: 0.5rem; text-decoration: none; color: var(--on-surface, #191c1e); font-size: 0.875rem; }
      .profile__link--active { background: var(--secondary-container, #6cf8bb); font-weight: 700; }
      .tabs { display: flex; gap: 0.5rem; margin-bottom: 1rem; flex-wrap: wrap; }
      .tabs__item--active { font-weight: 700; color: var(--secondary, #006c49); border-bottom: 2px solid var(--secondary, #006c49); }
      .order { background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; padding: 1rem; margin-bottom: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .order__head { display: flex; justify-content: space-between; align-items: center; }
      .order__date { margin-left: 0.5rem; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .order__status { font-size: 0.75rem; font-weight: 700; padding: 0.125rem 0.5rem; border-radius: 9999px; background: var(--surface-container-high, #e6e8ea); }
      .order__items { font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .order__foot { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; }
      .order__total { font-weight: 800; }
      .order__actions { display: flex; gap: 0.375rem; flex-wrap: wrap; }
      .empty { text-align: center; padding: 3rem 1rem; background: var(--surface-container-lowest, #fff); border-radius: 0.75rem; box-shadow: 0 1px 3px rgb(0 0 0 / 0.08); }
      .empty i { font-size: 2.5rem; color: var(--outline, #76777d); }
      .empty h3 { margin: 0.75rem 0 0.375rem; font-size: 1.125rem; }
      .empty p { margin: 0 0 1rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 0.75rem; cursor: pointer; font-size: 0.8125rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--danger { background: var(--error-container, #ffdad6); color: var(--error, #ba1a1a); }
      .btn--link { background: transparent; color: var(--secondary, #006c49); }
    `
  ]
})
export class StorefrontAccountPageComponent implements OnInit {
  readonly store = inject(StorefrontStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly activeTab = signal('all');

  readonly tabs = [
    { value: 'all', label: 'Tất cả' },
    { value: 'processing', label: 'Đang xử lý & giao' },
    { value: 'completed', label: 'Hoàn thành' },
    { value: 'cancelled', label: 'Đã hủy' }
  ];

  readonly orders = computed(() => this.store.orders());
  readonly profile = computed(() => this.store.profile());

  readonly dialogMode = signal<DialogMode>(null);
  readonly detail = signal<ShopOrderDetail | null>(null);
  readonly reviewTarget = signal<ShopOrderSummary | null>(null);
  readonly cancelTarget = signal<ShopOrderSummary | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly filteredOrders = computed(() => {
    const tab = this.activeTab();
    const orders = this.orders();

    switch (tab) {
      case 'processing':
        return orders.filter((o) => o.orderStatus === OrderStatus.Processing || o.orderStatus === OrderStatus.Shipped);
      case 'completed':
        return orders.filter((o) => o.orderStatus === OrderStatus.Completed);
      case 'cancelled':
        return orders.filter((o) => o.orderStatus === OrderStatus.Cancelled);
      default:
        return orders;
    }
  });

  async ngOnInit(): Promise<void> {
    await this.store.loadMyOrders();
    await this.store.loadProfile();
  }

  countFor(tab: string): number {
    const orders = this.orders();
    switch (tab) {
      case 'processing':
        return orders.filter((o) => o.orderStatus === OrderStatus.Processing || o.orderStatus === OrderStatus.Shipped).length;
      case 'completed':
        return orders.filter((o) => o.orderStatus === OrderStatus.Completed).length;
      case 'cancelled':
        return orders.filter((o) => o.orderStatus === OrderStatus.Cancelled).length;
      default:
        return orders.length;
    }
  }

  statusLabel(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'Chờ xác nhận';
      case OrderStatus.Paid: return 'Đã thanh toán';
      case OrderStatus.Processing: return 'Đang xử lý';
      case OrderStatus.Shipped: return 'Đang giao';
      case OrderStatus.Completed: return 'Hoàn thành';
      case OrderStatus.Cancelled: return 'Đã hủy';
      case OrderStatus.Refunded: return 'Đã hoàn tiền';
      default: return status;
    }
  }

  canCancel(order: ShopOrderSummary): boolean {
    return order.orderStatus === OrderStatus.Pending || order.orderStatus === OrderStatus.Processing;
  }

  canReview(order: ShopOrderSummary): boolean {
    return order.orderStatus === OrderStatus.Completed;
  }

  async openDetail(order: ShopOrderSummary): Promise<void> {
    const detail = await this.store.loadOrderDetail(order.id);
    if (detail) {
      this.detail.set(detail);
      this.dialogMode.set('detail');
    } else {
      this.toast.error('Không tải được chi tiết đơn hàng.');
    }
  }

  detailFields(): DialogField[] {
    const d = this.detail();
    if (!d) {
      return [];
    }

    const fields: DialogField[] = [
      { name: 'code', label: 'Mã đơn hàng', type: 'text' },
      { name: 'status', label: 'Trạng thái', type: 'text' },
      { name: 'recipient', label: 'Người nhận', type: 'text' },
      { name: 'phone', label: 'Số điện thoại', type: 'text' },
      { name: 'address', label: 'Địa chỉ giao hàng', type: 'textarea' },
      { name: 'subtotal', label: 'Tạm tính', type: 'text' },
      { name: 'shipping', label: 'Vận chuyển', type: 'text' },
      { name: 'discount', label: 'Chiết khấu', type: 'text' },
      { name: 'total', label: 'Tổng cộng', type: 'text' },
      { name: 'tracking', label: 'Mã vận đơn', type: 'text' },
      { name: 'note', label: 'Ghi chú', type: 'textarea' }
    ];

    return fields;
  }

  detailValues(): Record<string, unknown> {
    const d = this.detail();
    if (!d) {
      return {};
    }

    return {
      code: d.code,
      status: this.statusLabel(d.orderStatus),
      recipient: d.recipientName ?? '',
      phone: d.phoneNumber ?? '',
      address: d.shippingAddress ?? '',
      subtotal: `${d.totalAmount.toLocaleString('vi-VN')} d`,
      shipping: `${d.shippingAmount.toLocaleString('vi-VN')} d`,
      discount: `${d.discountAmount.toLocaleString('vi-VN')} d`,
      total: `${d.finalAmount.toLocaleString('vi-VN')} d`,
      tracking: d.trackingCode ?? '(chưa có)',
      note: d.customerNote ?? ''
    };
  }

  openReview(order: ShopOrderSummary): void {
    this.reviewTarget.set(order);
    this.formValues.set({
      bookId: '',
      rating: 5,
      title: '',
      content: ''
    });
    this.formError.set('');
    this.dialogMode.set('review');
  }

  reviewFields(): DialogField[] {
    return [
      { name: 'bookId', label: 'Mã sách cần đánh giá', type: 'text', required: true, hint: 'Copy GUID sách từ trang chi tiết sách.' },
      { name: 'rating', label: 'Điểm đánh giá (1-5)', type: 'number', required: true, min: 1, max: 5 },
      { name: 'title', label: 'Tiêu đề', type: 'text', placeholder: 'vd: Rất đáng đọc' },
      { name: 'content', label: 'Nội dung đánh giá', type: 'textarea', required: true, placeholder: 'Chia sẻ cảm nhận của bạn về cuốn sách...' }
    ];
  }

  onFormChange(values: Record<string, unknown>): void {
    this.formValues.set(values);
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.detail.set(null);
    this.reviewTarget.set(null);
    this.formError.set('');
  }

  async submitReview(): Promise<void> {
    const values = this.formValues();
    this.formError.set('');

    const bookId = String(values['bookId'] ?? '').trim();
    const content = String(values['content'] ?? '').trim();

    if (!bookId || !content) {
      this.formError.set('Vui lòng nhập mã sách và nội dung đánh giá.');
      return;
    }

    this.submitting.set(true);
    try {
      const ok = await this.store.submitReview({
        bookId,
        rating: Number(values['rating'] ?? 5),
        title: String(values['title'] ?? '').trim() || undefined,
        content
      });

      if (ok) {
        this.closeDialog();
      } else {
        this.formError.set('Không gửi được đánh giá. Bạn có thể đã đánh giá cuốn sách này rồi.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  askCancel(order: ShopOrderSummary): void {
    this.cancelTarget.set(order);
  }

  async runCancel(): Promise<void> {
    const target = this.cancelTarget();
    if (!target) {
      return;
    }

    this.submitting.set(true);
    try {
      const ok = await this.store.cancelOrder(target.id);
      if (!ok) {
        this.toast.error('Không thể hủy đơn hàng này.');
      }
    } finally {
      this.submitting.set(false);
      this.cancelTarget.set(null);
    }
  }

  async track(order: ShopOrderSummary): Promise<void> {
    await this.router.navigate(['/shop/order-success', order.code]);
  }
}
