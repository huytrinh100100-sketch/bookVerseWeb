import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="login">
      <form class="login__card" (ngSubmit)="submit()">
        <div class="login__brand"><i class="pi pi-book"></i> BookVerse</div>
        <h1>Đăng nhập</h1>
        <p class="login__sub">Đăng nhập để quản lý cửa hàng hoặc mua sách.</p>

        <label>
          <span>Tên đăng nhập hoặc email</span>
          <input type="text" name="userNameOrEmail" [(ngModel)]="userNameOrEmail" required />
        </label>
        <label>
          <span>Mật khẩu</span>
          <input type="password" name="password" [(ngModel)]="password" required />
        </label>

        @if (error()) {
          <p class="login__error">{{ error() }}</p>
        }

        <button type="submit" class="login__submit" [disabled]="loading()">
          {{ loading() ? 'Đang xử lý...' : 'Đăng nhập' }}
        </button>
      </form>
    </div>
  `,
  styles: [
    `
      .login { min-height: 100vh; display: grid; place-items: center; background: var(--primary-container, #131b2e); }
      .login__card { width: 24rem; background: var(--surface-container-lowest, #fff); border-radius: 1rem; padding: 2rem; display: flex; flex-direction: column; gap: 0.75rem; }
      .login__brand { display: flex; align-items: center; gap: 0.5rem; font-weight: 800; font-size: 1.25rem; margin-bottom: 0.5rem; }
      .login__card h1 { margin: 0; font-size: 1.5rem; }
      .login__sub { margin: 0 0 0.5rem; font-size: 0.875rem; color: var(--on-surface-variant, #45464d); }
      .login__card label { display: flex; flex-direction: column; gap: 0.25rem; }
      .login__card label span { font-size: 0.75rem; text-transform: uppercase; color: var(--on-surface-variant, #45464d); }
      .login__card input { padding: 0.625rem 0.75rem; border-radius: 0.5rem; border: 1px solid var(--outline-variant, #c6c6cd); }
      .login__error { color: var(--error, #ba1a1a); font-size: 0.875rem; margin: 0; }
      .login__submit { margin-top: 0.5rem; border: none; border-radius: 0.5rem; padding: 0.75rem; background: var(--secondary, #006c49); color: #fff; font-weight: 700; cursor: pointer; }
    `
  ]
})
export class LoginPageComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  userNameOrEmail = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal('');

  async submit(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const profile = await this.auth.login(this.userNameOrEmail, this.password);
      if (profile) {
        // Khach hang (Customer) ve storefront; nhan vien/quan tri ve back-office.
        const isCustomer = profile.roles.includes('Customer') && !profile.roles.includes('Admin');
        await this.router.navigate(isCustomer ? ['/shop'] : ['/admin/dashboard']);
      } else {
        this.error.set('Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.');
      }
    } catch {
      this.error.set('Không kết nối được máy chủ. Vui lòng thử lại.');
    } finally {
      this.loading.set(false);
    }
  }
}
