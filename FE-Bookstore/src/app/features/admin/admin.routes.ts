import { Routes } from '@angular/router';
import { AppShellComponent } from '../../layout/app-shell/app-shell.component';
import { permissionGuard } from '../../core/guards/permission.guard';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.page').then((m) => m.AdminDashboardPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'dashboard.view' }
      },
      {
        path: 'books',
        loadComponent: () => import('./books/books.page').then((m) => m.AdminBooksPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'book.list' }
      },
      {
        path: 'categories',
        loadComponent: () => import('./categories/categories.page').then((m) => m.AdminCategoriesPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'category.list' }
      },
      {
        path: 'partners',
        loadComponent: () => import('./partners/partners.page').then((m) => m.AdminPartnersPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'author.list' }
      },
      {
        path: 'orders',
        loadComponent: () => import('./orders/orders.page').then((m) => m.AdminOrdersPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'order.list' }
      },
      {
        path: 'customers',
        loadComponent: () => import('./customers/customers.page').then((m) => m.AdminCustomersPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'customer.list' }
      },
      {
        path: 'inventory',
        loadComponent: () => import('./inventory/inventory.page').then((m) => m.AdminInventoryPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'bookcopy.list' }
      },
      {
        path: 'promotions',
        loadComponent: () => import('./promotions/promotions.page').then((m) => m.AdminPromotionsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'coupon.list' }
      },
      {
        path: 'reviews',
        loadComponent: () => import('./reviews/reviews.page').then((m) => m.AdminReviewsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'review.list' }
      },
      {
        path: 'articles',
        loadComponent: () => import('./articles/articles.page').then((m) => m.AdminArticlesPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'article.list' }
      },
      {
        path: 'accounts',
        loadComponent: () => import('./accounts/accounts.page').then((m) => m.AdminAccountsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'account.list' }
      },
      {
        path: 'permissions',
        loadComponent: () => import('./permissions/permissions.page').then((m) => m.AdminPermissionsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'permission.view' }
      },
      {
        path: 'settings',
        loadComponent: () => import('./settings/settings.page').then((m) => m.AdminSettingsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'setting.view' }
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./errors/forbidden.page').then((m) => m.ForbiddenPageComponent)
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
    ]
  }
];
