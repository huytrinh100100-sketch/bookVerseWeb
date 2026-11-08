import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export type ConfirmTone = 'danger' | 'warning' | 'info';

/**
 * Hop xac nhan dung chung cho cac thao tac xoa / khoa / phuc hoi.
 * Moi thay the confirm-delete-dialog cu (khong dung duoc nua).
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="overlay" (click)="onOverlayClick($event)">
      <div class="dialog" role="dialog" aria-modal="true" [class.dialog--danger]="tone() === 'danger'">
        <div class="dialog__icon">
          <i class="pi" [class.pi-exclamation-triangle]="tone() === 'danger'" [class.pi-info-circle]="tone() !== 'danger'"></i>
        </div>
        <h2 class="dialog__title">{{ title() }}</h2>
        <p class="dialog__message">{{ message() }}</p>
        @if (detail()) {
          <p class="dialog__detail">{{ detail() }}</p>
        }
        <div class="dialog__actions">
          <button type="button" class="btn btn--ghost" (click)="cancelled.emit()">{{ cancelLabel() }}</button>
          <button type="button" class="btn" [class.btn--danger]="tone() === 'danger'" [class.btn--primary]="tone() !== 'danger'"
                  [disabled]="submitting()" (click)="confirmed.emit()">
            @if (submitting()) {
              <i class="pi pi-spin pi-spinner"></i> Đang xử lý...
            } @else {
              {{ confirmLabel() }}
            }
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .overlay {
        position: fixed;
        inset: 0;
        background: rgb(0 0 0 / 0.45);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 1rem;
        z-index: 1100;
        backdrop-filter: blur(2px);
      }
      .dialog {
        width: 100%;
        max-width: 26rem;
        background: var(--surface-container-lowest, #fff);
        border-radius: 1rem;
        box-shadow: 0 12px 40px rgb(0 0 0 / 0.25);
        padding: 1.5rem;
        text-align: center;
      }
      .dialog__icon {
        width: 3rem;
        height: 3rem;
        border-radius: 9999px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-size: 1.25rem;
        background: var(--surface-container-high, #e6e8ea);
        color: var(--on-surface-variant, #45464d);
        margin-bottom: 0.75rem;
      }
      .dialog--danger .dialog__icon { background: var(--error-container, #ffdad6); color: var(--error, #ba1a1a); }
      .dialog__title { margin: 0 0 0.5rem; font-size: 1.0625rem; }
      .dialog__message { margin: 0 0 0.5rem; color: var(--on-surface-variant, #45464d); font-size: 0.875rem; }
      .dialog__detail {
        margin: 0 0 0.5rem;
        font-size: 0.8125rem;
        color: var(--outline, #76777d);
        background: var(--surface-container-low, #f2f4f6);
        padding: 0.5rem 0.75rem;
        border-radius: 0.5rem;
        text-align: left;
      }
      .dialog__actions { display: flex; gap: 0.5rem; justify-content: flex-end; margin-top: 1.25rem; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; }
      .btn--ghost { background: var(--surface-container-high, #e6e8ea); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--danger { background: var(--error, #ba1a1a); color: #fff; }
      .btn:disabled { opacity: 0.6; cursor: not-allowed; }
    `
  ]
})
export class ConfirmDialogComponent {
  readonly title = input('Xác nhận');
  readonly message = input('Bạn có chắc không?');
  readonly detail = input('');
  readonly confirmLabel = input('Xác nhận');
  readonly cancelLabel = input('Hủy');
  readonly tone = input<ConfirmTone>('danger');
  readonly submitting = input(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.cancelled.emit();
    }
  }
}
