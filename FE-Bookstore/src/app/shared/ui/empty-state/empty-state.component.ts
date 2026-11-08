import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="empty-state">
      <i class="pi" [ngClass]="icon()"></i>
      <p class="empty-state__title">{{ title() }}</p>
      @if (description()) {
        <p class="empty-state__desc">{{ description() }}</p>
      }
      <ng-content />
    </div>
  `,
  styles: [
    `
      .empty-state { text-align: center; padding: 3rem 1rem; color: var(--outline, #76777d); }
      .empty-state i { font-size: 2.5rem; }
      .empty-state__title { margin: 0.75rem 0 0.25rem; font-weight: 600; color: var(--on-surface, #191c1e); }
      .empty-state__desc { margin: 0; font-size: 0.875rem; }
    `
  ]
})
export class EmptyStateComponent {
  readonly icon = input('pi pi-inbox');
  readonly title = input('Chua co du lieu');
  readonly description = input('');
}
