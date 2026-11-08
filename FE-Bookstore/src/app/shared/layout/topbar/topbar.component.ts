import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="topbar">
      <div class="topbar__search">
        <i class="pi pi-search"></i>
        <input type="search" placeholder="Tìm đơn hàng, sách, khách hàng..." />
      </div>
      <div class="topbar__right">
        <span class="topbar__user">
          <i class="pi pi-user-circle"></i>
          {{ auth.displayName() }}
        </span>
        <button type="button" class="topbar__icon" title="Thông báo"><i class="pi pi-bell"></i></button>
        <a class="topbar__link" routerLink="/shop">Xem storefront</a>
      </div>
    </header>
  `,
  styles: [
    `
      .topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        padding: 0.75rem 2rem;
        background: var(--surface-container-lowest, #fff);
        border-bottom: 1px solid var(--outline-variant, #c6c6cd);
        position: sticky;
        top: 0;
        z-index: 10;
      }
      .topbar__search { position: relative; }
      .topbar__search i { position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%); color: var(--outline, #76777d); }
      .topbar__search input {
        padding: 0.5rem 0.75rem 0.5rem 2.5rem;
        border-radius: 0.5rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        background: var(--surface-container-low, #f2f4f6);
        width: 22rem;
        font-family: inherit;
        font-size: 0.875rem;
      }
      .topbar__right { display: flex; align-items: center; gap: 1rem; }
      .topbar__user {
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
        font-size: 0.8125rem;
        font-weight: 600;
        color: var(--on-surface-variant, #45464d);
      }
      .topbar__user i { font-size: 1.125rem; color: var(--secondary, #006c49); }
      .topbar__icon {
        border: none;
        background: transparent;
        cursor: pointer;
        font-size: 1.125rem;
        padding: 0.375rem;
        border-radius: 0.375rem;
        color: var(--on-surface-variant, #45464d);
      }
      .topbar__icon:hover { background: var(--surface-container-low, #f2f4f6); }
      .topbar__link { font-size: 0.8125rem; color: var(--secondary, #006c49); text-decoration: none; font-weight: 600; }
    `
  ]
})
export class TopbarComponent {
  readonly auth = inject(AuthStore);
}
