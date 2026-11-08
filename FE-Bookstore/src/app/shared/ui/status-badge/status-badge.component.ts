import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface StatusBadgeConfig {
  label: string;
  severity: 'success' | 'info' | 'warning' | 'danger';
}

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-severity]="config().severity">{{ config().label }}</span>`,
  styles: [
    `
      .badge { display: inline-flex; align-items: center; padding: 0.125rem 0.5rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; background: #e6e8ea; }
      .badge[data-severity='success'] { background: #c8f0dc; color: #005236; }
      .badge[data-severity='warning'] { background: #ffe3b3; color: #7a4b00; }
      .badge[data-severity='danger'] { background: #ffdad6; color: #93000a; }
      .badge[data-severity='info'] { background: #d8e2ff; color: #001a42; }
    `
  ]
})
export class StatusBadgeComponent {
  readonly config = input.required<StatusBadgeConfig>();
}
