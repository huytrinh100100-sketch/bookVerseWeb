import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { ReviewListItemResponse } from './reviews.models';
import { ReviewsStore } from './reviews.store';

type DialogMode = 'approve' | 'reject' | null;

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [
    FormsModule,
    DecimalPipe,
    PageHeaderComponent,
    StatCardComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Duyệt đánh giá"
      subtitle="Kiểm duyệt đánh giá trước khi hiển thị trên storefront."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Vận hành' }, { label: 'Đánh giá' }]"
    >
      <button type="button" class="btn btn--ghost"><i class="pi pi-filter"></i> Bộ lọc nâng cao</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Chờ duyệt', value: store.pendingCount() + '', icon: 'pi pi-clock', tone: 'warning' }" />
      <app-stat-card [config]="{ label: 'Đã duyệt', value: store.approvedCount() + '', icon: 'pi pi-check-circle', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Bị đánh dấu', value: store.flaggedCount() + '', icon: 'pi pi-flag', tone: 'danger' }" />
      <app-stat-card [config]="{ label: 'Tổng số đánh giá', value: (store.totalItems() | number) + '', icon: 'pi pi-comments', tone: 'primary' }" />
    </div>

    <div class="review-list">
      @for (review of rows(); track review.id) {
        <article class="review">
          <header class="review__head">
            <div class="review__title">
              <strong>{{ review.bookTitle }}</strong>
              <span class="review__customer">
                {{ review.customerName }}
                @if (review.isVerifiedPurchase) {
                  <span class="review__verified"><i class="pi pi-verified"></i> Đã mua hàng</span>
                }
              </span>
            </div>
            <div class="review__meta">
              <span class="review__rating">
                @for (star of [1, 2, 3, 4, 5]; track star) {
                  <i class="pi pi-star-fill" [class.review__star--off]="star > review.rating"></i>
                }
                {{ review.rating }}/5
              </span>
              <span class="badge" [attr.data-severity]="severityOf(review)">{{ statusLabel(review) }}</span>
            </div>
          </header>

          @if (review.title) {
            <h3 class="review__title-text">{{ review.title }}</h3>
          }
          <p class="review__content">{{ review.content }}</p>

          @if (review.replyContent) {
            <div class="review__reply">
              <span class="review__reply-label">Phản hồi của cửa hàng</span>
              <p>{{ review.replyContent }}</p>
            </div>
          }

          <footer class="review__actions">
            <button type="button" class="btn btn--success" *hasPermission="PermissionConstants.ReviewApprove" (click)="openApprove(review)">
              <i class="pi pi-check"></i> Duyệt &amp; xuất bản
            </button>
            <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.ReviewReject" (click)="openReject(review)">
              <i class="pi pi-ban"></i> Từ chối
            </button>
            <span class="review__spacer"></span>
            <span class="review__helpful"><i class="pi pi-thumbs-up"></i> {{ review.helpfulCount }} hữu ích</span>
          </footer>
        </article>
      } @empty {
        <div class="review__empty"><i class="pi pi-inbox"></i><span>Không có đánh giá nào cần duyệt.</span></div>
      }
    </div>

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        mode="edit"
        [submitLabel]="dialogMode() === 'approve' ? 'Duyệt đánh giá' : 'Từ chối đánh giá'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(13rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
      .review-list { display: flex; flex-direction: column; gap: 1rem; }
      .review {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        padding: 1.25rem;
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
      }
      .review__head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
      .review__title { display: flex; flex-direction: column; gap: 0.25rem; }
      .review__customer { display: block; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .review__verified { color: var(--secondary, #006c49); font-weight: 600; margin-left: 0.375rem; }
      .review__meta { display: flex; align-items: center; gap: 0.75rem; }
      .review__rating { color: #a4600a; font-size: 0.875rem; font-weight: 700; white-space: nowrap; }
      .review__star--off { color: var(--outline-variant, #c6c6cd); }
      .review__title-text { margin: 0.75rem 0 0.25rem; font-size: 0.9375rem; }
      .review__content { color: var(--on-surface, #191c1e); line-height: 1.5rem; margin: 0; }
      .review__reply {
        margin-top: 0.75rem;
        padding: 0.75rem 1rem;
        background: var(--surface-container-low, #f2f4f6);
        border-left: 3px solid var(--secondary, #006c49);
        border-radius: 0.5rem;
      }
      .review__reply-label { font-size: 0.6875rem; font-weight: 700; text-transform: uppercase; color: var(--on-surface-variant, #45464d); }
      .review__reply p { margin: 0.25rem 0 0; font-size: 0.875rem; }
      .review__actions { display: flex; gap: 0.5rem; flex-wrap: wrap; align-items: center; margin-top: 1rem; }
      .review__spacer { flex: 1; }
      .review__helpful { font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .review__empty {
        text-align: center;
        padding: 3rem 1rem;
        color: var(--outline, #76777d);
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
      }
      .review__empty i { display: block; font-size: 2rem; margin-bottom: 0.5rem; opacity: 0.5; }
      .badge {
        display: inline-flex;
        padding: 0.1875rem 0.5625rem;
        border-radius: 9999px;
        font-size: 0.75rem;
        font-weight: 600;
        white-space: nowrap;
        background: var(--surface-container-high, #e6e8ea);
      }
      .badge[data-severity='success'] { background: #c8f0dc; color: #005236; }
      .badge[data-severity='warning'] { background: #ffe3b3; color: #7a4b00; }
      .badge[data-severity='danger'] { background: #ffdad6; color: #93000a; }
      .badge[data-severity='info'] { background: var(--tertiary-fixed, #d8e2ff); color: #001a42; }
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
      .btn--success { background: var(--secondary, #006c49); color: #fff; }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
    `
  ]
})
export class AdminReviewsPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(ReviewsStore);

  readonly rows = computed(() => this.store.items());

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<ReviewListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  async ngOnInit(): Promise<void> {
    await this.store.load();
  }

  statusLabel(review: ReviewListItemResponse): string {
    switch (review.status) {
      case 'Approved': return 'Đã duyệt';
      case 'Rejected': return 'Đã từ chối';
      case 'Flagged': return 'Bị đánh dấu';
      case 'Spam': return 'Spam';
      default: return 'Chờ duyệt';
    }
  }

  severityOf(review: ReviewListItemResponse): string {
    switch (review.status) {
      case 'Approved': return 'success';
      case 'Rejected': return 'danger';
      case 'Flagged': return 'warning';
      case 'Spam': return 'danger';
      default: return 'info';
    }
  }

  openApprove(review: ReviewListItemResponse): void {
    this.dialogTarget.set(review);
    this.formValues.set({ replyContent: review.replyContent ?? '' });
    this.formError.set('');
    this.dialogMode.set('approve');
  }

  openReject(review: ReviewListItemResponse): void {
    this.dialogTarget.set(review);
    this.formValues.set({ reason: '' });
    this.formError.set('');
    this.dialogMode.set('reject');
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
    return this.dialogMode() === 'approve' ? 'Duyệt đánh giá' : 'Từ chối đánh giá';
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    return target ? `${target.bookTitle} - ${target.customerName}` : '';
  }

  dialogFields(): DialogField[] {
    if (this.dialogMode() === 'approve') {
      return [
        { name: 'replyContent', label: 'Phản hồi của cửa hàng', type: 'textarea', placeholder: 'Cảm ơn khách hàng đã đánh giá...' }
      ];
    }
    return [
      { name: 'reason', label: 'Lý do từ chối', type: 'textarea', required: true, placeholder: 'Nhập lý do từ chối đánh giá...' }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      if (this.dialogMode() === 'approve' && target) {
        ok = await this.store.approve(target.id, String(values['replyContent'] ?? '').trim() || undefined);
      } else if (this.dialogMode() === 'reject' && target) {
        ok = await this.store.reject(target.id, String(values['reason'] ?? '').trim());
      }

      if (ok) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể xử lý đánh giá. Vui lòng thử lại.');
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
