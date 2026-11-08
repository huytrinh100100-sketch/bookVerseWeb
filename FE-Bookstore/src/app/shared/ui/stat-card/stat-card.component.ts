import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface StatCardConfig {
  label: string;
  value: string;
  note?: string;
  icon: string;
  tone: 'primary' | 'success' | 'warning' | 'danger' | 'info';
}

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stat-card" [attr.data-tone]="config().tone">
      <div class="stat-card__head">
        <span class="stat-card__label">{{ config().label }}</span>
        <i class="pi" [ngClass]="config().icon"></i>
      </div>
      <div class="stat-card__value">{{ config().value }}</div>
      @if (config().note) {
        <div class="stat-card__note">{{ config().note }}</div>
      }
    </div>
  `,
  styles: [
    `
      .stat-card {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.75rem;
        padding: 1.25rem;
        box-shadow: 0 1px 3px rgb(0 0 0 / 0.08);
      }
      .stat-card__head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        color: var(--on-surface-variant, #45464d);
      }
      .stat-card__label {
        font-size: 0.75rem;
        text-transform: uppercase;
        letter-spacing: 0.025em;
        font-weight: 600;
      }
      .stat-card__value {
        margin-top: 0.5rem;
        font-size: 1.75rem;
        font-weight: 700;
        line-height: 1.75rem;
      }
      .stat-card__note {
        margin-top: 0.25rem;
        font-size: 0.8125rem;
        color: var(--on-surface-variant, #45464d);
      }
      .stat-card[data-tone='success'] i { color: var(--secondary, #006c49); }
      .stat-card[data-tone='danger'] i { color: var(--error, #ba1a1a); }
      .stat-card[data-tone='warning'] i { color: #a4600a; }
      .stat-card[data-tone='info'] i { color: var(--tertiary-container, #001a42); }
    `
  ]
})
export class StatCardComponent {
  readonly config = input.required<StatCardConfig>();
}
