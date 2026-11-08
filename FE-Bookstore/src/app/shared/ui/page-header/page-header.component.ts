import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface BreadcrumbItem {
  label: string;
  route?: string;
  icon?: string;
}

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-header">
      <nav class="page-header__crumbs">
        @for (item of breadcrumbs(); track item.label) {
          <span class="page-header__crumb">
            @if (item.icon) {
              <i class="pi" [ngClass]="item.icon"></i>
            }
            {{ item.label }}
          </span>
          @if (!$last) {
            <i class="pi pi-angle-right page-header__sep"></i>
          }
        }
      </nav>
      <div class="page-header__main">
        <div>
          <h1 class="page-header__title">{{ title() }}</h1>
          @if (subtitle()) {
            <p class="page-header__subtitle">{{ subtitle() }}</p>
          }
        </div>
        <div class="page-header__actions">
          <ng-content />
        </div>
      </div>
    </header>
  `,
  styles: [
    `
      .page-header { margin-bottom: 1.5rem; }
      .page-header__crumbs { display: flex; align-items: center; gap: 0.375rem; font-size: 0.75rem; color: var(--outline, #76777d); }
      .page-header__crumb { display: inline-flex; align-items: center; gap: 0.25rem; }
      .page-header__sep { font-size: 0.625rem; }
      .page-header__main { display: flex; align-items: flex-end; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
      .page-header__title { font-size: 2.25rem; font-weight: 700; letter-spacing: -0.025em; margin: 0.25rem 0 0; }
      .page-header__subtitle { margin: 0.25rem 0 0; color: var(--on-surface-variant, #45464d); font-size: 0.9375rem; }
      .page-header__actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }
    `
  ]
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly breadcrumbs = input<BreadcrumbItem[]>([]);
}
