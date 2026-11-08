import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

/**
 * BookVerse - Quản lý sách.
 * 2 khu vực tách biệt:
 *   /admin/...      -> Back-office (quản trị)  : yêu cầu quyền (permission guard)
 *   /shop/...       -> Storefront (khách mua)   : chỉ cần đăng nhập cho các bước thanh toán
 */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'shop' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.page').then((m) => m.LoginPageComponent)
  },
  {
    path: 'shop',
    loadChildren: () => import('./features/storefront/storefront.routes').then((m) => m.STOREFRONT_ROUTES)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES)
  },
  { path: '**', redirectTo: 'shop' }
];
