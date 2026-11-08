import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastService } from './core/services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <router-outlet />
    <div class="toasts">
      @for (message of toast.messages(); track message.id) {
        <div class="toast" [attr.data-severity]="message.severity" (click)="toast.dismiss(message.id)">
          <strong>{{ message.summary }}</strong>
          @if (message.detail) {
            <span>{{ message.detail }}</span>
          }
        </div>
      }
    </div>
  `,
  styles: [
    `
      .toasts { position: fixed; right: 1.5rem; bottom: 1.5rem; display: flex; flex-direction: column; gap: 0.5rem; z-index: 999; }
      .toast { background: var(--surface-container-highest, #e0e3e5); padding: 0.75rem 1rem; border-radius: 0.5rem; min-width: 16rem; box-shadow: 0 4px 12px rgb(0 0 0 / 0.16); display: flex; flex-direction: column; cursor: pointer; }
      .toast[data-severity='success'] { background: #c8f0dc; color: #005236; }
      .toast[data-severity='warn'] { background: #ffe3b3; color: #7a4b00; }
      .toast[data-severity='error'] { background: #ffdad6; color: #93000a; }
      .toast span { font-size: 0.8125rem; }
    `
  ]
})
export class AppComponent {
  readonly toast = new ToastService();
}
