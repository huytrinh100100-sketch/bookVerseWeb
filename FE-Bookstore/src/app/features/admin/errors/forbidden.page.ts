import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="forbidden">
      <i class="pi pi-lock"></i>
      <h1>403 - Không có quyền truy cập</h1>
      <p>Bạn không có quyền thực hiện thao tác này. Liên hệ quản trị viên nếu cần cập nhật.</p>
      <a class="btn" routerLink="/shop">Về cửa hàng</a>
    </div>
  `,
  styles: [
    `
      .forbidden { text-align: center; padding: 4rem 1rem; }
      .forbidden i { font-size: 3rem; color: var(--error, #ba1a1a); }
      .btn { display: inline-block; margin-top: 1rem; background: var(--secondary, #006c49); color: #fff; padding: 0.5rem 1rem; border-radius: 0.5rem; text-decoration: none; font-weight: 600; }
    `
  ]
})
export class ForbiddenPageComponent {}
