import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-delete-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="confirm">
      <h3>{{ title() }}</h3>
      <p>{{ message() }}</p>
      <div class="confirm__actions">
        <button type="button" (click)="cancelled.emit()">Hủy</button>
        <button type="button" class="confirm__danger" (click)="confirmed.emit()">{{ confirmLabel() }}</button>
      </div>
    </div>
  `,
  styles: [
    `
      .confirm { padding: 1.5rem; }
      .confirm h3 { margin: 0 0 0.5rem; }
      .confirm p { margin: 0 0 1.25rem; color: var(--on-surface-variant, #45464d); }
      .confirm__actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
      .confirm__danger { background: var(--error, #ba1a1a); color: #fff; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer; }
    `
  ]
})
export class ConfirmDeleteDialogComponent {
  readonly title = input('Xóa bản ghi');
  readonly message = input('Bạn có chắc muốn xóa? Thao tác này không thể hoàn tác.');
  readonly confirmLabel = input('Xóa');
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
