import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MenuService } from '../../../core/services/menu.service';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, NgClass],
  host: {
    '[class.is-collapsed]': 'collapsed()'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="sidebar" [class.sidebar--collapsed]="collapsed()">
      <div class="sidebar__brand">
        <img
          class="sidebar__logo"
          [src]="collapsed() ? 'assets/logo-bookverse-mark.png' : 'assets/logo-bookverse-light.png'"
          alt="BookVerse"
        />
        @if (!collapsed()) {
          <span class="sidebar__brand-text">BookVerse</span>
        }
        <button type="button" class="sidebar__toggle" (click)="collapsed.set(!collapsed())" [title]="collapsed() ? 'Mở rộng menu' : 'Thu gọn menu'">
          <i class="pi" [class.pi-angle-left]="!collapsed()" [class.pi-angle-right]="collapsed()"></i>
        </button>
      </div>

      <nav class="sidebar__nav">
        @for (item of visibleMenu(); track item.routerLink) {
          <div class="sidebar__group">
            @if (!collapsed()) {
              <span class="sidebar__group-label">{{ item.label }}</span>
            }
            @if (item.children?.length) {
              @for (child of item.children; track child.routerLink) {
                <a class="sidebar__link" [routerLink]="child.routerLink" routerLinkActive="sidebar__link--active" [title]="child.label">
                  <i class="pi" [ngClass]="child.icon"></i>
                  @if (!collapsed()) {
                    <span>{{ child.label }}</span>
                  }
                </a>
              }
            } @else {
              <a class="sidebar__link" [routerLink]="item.routerLink" routerLinkActive="sidebar__link--active" [title]="item.label">
                <i class="pi" [ngClass]="item.icon"></i>
                @if (!collapsed()) {
                  <span>{{ item.label }}</span>
                }
              </a>
            }
          </div>
        }
      </nav>

      <div class="sidebar__footer">
        <div class="sidebar__user">
          <i class="pi pi-user-circle sidebar__avatar"></i>
          @if (!collapsed()) {
            <span class="sidebar__user-info">
              <span class="sidebar__user-name">{{ auth.displayName() }}</span>
              <span class="sidebar__user-role">{{ roleLabel() }}</span>
            </span>
          }
        </div>
        <button type="button" class="sidebar__logout" (click)="logout()" title="Đăng xuất">
          <i class="pi pi-sign-out"></i>
          @if (!collapsed()) {
            <span>Đăng xuất</span>
          }
        </button>
      </div>
    </aside>
  `,
  styles: [
    `
      :host {
        display: block;
        width: 16rem;
        height: 100vh;
        position: sticky;
        top: 0;
        transition: width 0.2s ease;
      }
      :host(.is-collapsed) { width: 4rem; }

      .sidebar {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        width: 100%;
        height: 100%;
        overflow-y: auto;
        overflow-x: hidden;
        background: var(--primary-container, #131b2e);
        color: var(--on-primary-container, #d8e2ff);
        padding-bottom: 0.5rem;
      }

      .sidebar__brand {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 1.125rem 1rem;
        font-weight: 700;
        font-size: 1.125rem;
        border-bottom: 1px solid rgb(255 255 255 / 0.08);
        position: sticky;
        top: 0;
        background: var(--primary-container, #131b2e);
        z-index: 2;
      }
      .sidebar__brand > i { font-size: 1.375rem; flex-shrink: 0; }
      .sidebar__logo {
        width: 2.25rem;
        height: 2.25rem;
        object-fit: contain;
        flex-shrink: 0;
      }
      .sidebar__brand-text { white-space: nowrap; overflow: hidden; }
      .sidebar__toggle {
        margin-left: auto;
        border: none;
        background: rgb(255 255 255 / 0.08);
        color: inherit;
        cursor: pointer;
        width: 1.75rem;
        height: 1.75rem;
        border-radius: 0.375rem;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
      }
      .sidebar__toggle:hover { background: rgb(255 255 255 / 0.18); }

      .sidebar__nav { padding: 1rem 0.75rem; display: flex; flex-direction: column; gap: 1rem; flex: 1; }
      .sidebar__group { display: flex; flex-direction: column; gap: 0.125rem; }
      .sidebar__group-label {
        font-size: 0.6875rem;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        color: var(--outline-variant, #c6c6cd);
        padding: 0 0.5rem 0.25rem;
      }

      .sidebar__link {
        display: flex;
        align-items: center;
        gap: 0.625rem;
        padding: 0.5rem 0.625rem;
        border-radius: 0.5rem;
        color: inherit;
        text-decoration: none;
        font-size: 0.875rem;
        white-space: nowrap;
      }
      .sidebar__link i { font-size: 1rem; width: 1.25rem; text-align: center; flex-shrink: 0; }
      .sidebar__link:hover { background: rgb(255 255 255 / 0.08); }
      .sidebar__link--active {
        background: var(--secondary-container, #6cf8bb);
        color: var(--on-secondary-container, #002113);
        font-weight: 600;
      }

      .sidebar__footer {
        padding: 0.75rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        border-top: 1px solid rgb(255 255 255 / 0.08);
      }
      .sidebar__user { display: flex; align-items: center; gap: 0.5rem; padding: 0.25rem 0.375rem; }
      .sidebar__avatar { font-size: 1.75rem; color: var(--secondary-container, #6cf8bb); }
      .sidebar__user-info { display: flex; flex-direction: column; min-width: 0; }
      .sidebar__user-name { font-size: 0.8125rem; font-weight: 600; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
      .sidebar__user-role { font-size: 0.6875rem; opacity: 0.75; }
      .sidebar__logout {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        border: none;
        background: transparent;
        color: inherit;
        cursor: pointer;
        padding: 0.5rem 0.625rem;
        border-radius: 0.5rem;
        font-size: 0.8125rem;
        font-weight: 600;
      }
      .sidebar__logout:hover { background: rgb(186 26 26 / 0.2); color: #ffdad6; }

      .sidebar--collapsed { width: 100%; }
      .sidebar--collapsed .sidebar__brand { padding: 1.125rem 0.625rem; justify-content: center; }
      .sidebar--collapsed .sidebar__brand > i { margin: 0; }
      .sidebar--collapsed .sidebar__toggle {
        position: absolute;
        top: 1.375rem;
        right: -0.75rem;
        width: 1.5rem;
        height: 1.5rem;
        background: var(--secondary-container, #6cf8bb);
        color: var(--on-secondary-container, #002113);
      }
      .sidebar--collapsed .sidebar__link { justify-content: center; padding: 0.5rem; }
      .sidebar--collapsed .sidebar__nav { padding: 1rem 0.5rem; }
      .sidebar--collapsed .sidebar__user { justify-content: center; }
    `
  ]
})
export class SidebarComponent {
  readonly menu = inject(MenuService);
  readonly auth = inject(AuthStore);

  readonly collapsed = signal(false);

  /** Chi hien nhom menu con khi it nhat 1 muc con duoc phep truy cap. */
  readonly visibleMenu = computed(() =>
    this.menu.visibleItems().filter((item) => !item.children || item.children.length > 0)
  );

  readonly roleLabel = computed(() => {
    const roles = this.auth.roles();
    if (!roles.length) {
      return 'Nhân viên';
    }
    return roles.join(', ');
  });

  logout(): void {
    this.auth.clearSession();
    window.location.href = '/login';
  }
}
