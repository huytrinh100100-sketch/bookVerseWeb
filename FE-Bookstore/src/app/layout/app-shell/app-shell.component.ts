import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../shared/layout/sidebar/sidebar.component';
import { TopbarComponent } from '../../shared/layout/topbar/topbar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="shell">
      <app-sidebar class="shell__sidebar" />
      <div class="shell__body">
        <app-topbar />
        <main class="shell__main">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [
    `
      .shell {
        display: flex;
        min-height: 100vh;
        background: var(--surface, #f7f9fb);
      }
      .shell__sidebar {
        flex-shrink: 0;
        position: sticky;
        top: 0;
        height: 100vh;
        align-self: flex-start;
        overflow: visible;
      }
      .shell__body { flex: 1; min-width: 0; }
      .shell__main { padding: 1.5rem 2rem; max-width: 96rem; }
    `
  ]
})
export class AppShellComponent {}
