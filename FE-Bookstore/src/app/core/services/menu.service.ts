import { computed, inject, Injectable } from '@angular/core';
import { AuthStore } from '../auth/auth.store';

export interface AppMenuItem {
  label: string;
  icon: string;
  routerLink: string;
  permission?: string;
  children?: AppMenuItem[];
}

/**
 * Menu quản trị bám theo sidebar Stitch (bookverse_*) và kiểm tra quyền.
 *
 * Mỗi mục khai báo `permission` tương ứng policy code của BE
 * (xem core/permissions/permission.constants.ts). Người bán hàng chỉ thấy
 * những mục mà tài khoản của họ được cấp quyền.
 */
@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly authStore = inject(AuthStore);

  private readonly items: AppMenuItem[] = [
    {
      label: 'Tổng quan',
      icon: 'pi pi-chart-line',
      routerLink: '/admin/dashboard',
      permission: 'dashboard.view'
    },
    {
      label: 'Danh mục',
      icon: 'pi pi-book',
      routerLink: '/admin/books',
      permission: 'book.list',
      children: [
        { label: 'Quản lý sách', icon: 'pi pi-list', routerLink: '/admin/books', permission: 'book.list' },
        { label: 'Quản lý danh mục', icon: 'pi pi-sitemap', routerLink: '/admin/categories', permission: 'category.list' },
        { label: 'Tác giả & NXB', icon: 'pi pi-users', routerLink: '/admin/partners', permission: 'author.list' }
      ]
    },
    {
      label: 'Bán hàng',
      icon: 'pi pi-receipt',
      routerLink: '/admin/orders',
      permission: 'order.list',
      children: [
        { label: 'Đơn hàng', icon: 'pi pi-receipt', routerLink: '/admin/orders', permission: 'order.list' },
        { label: 'Khách hàng', icon: 'pi pi-users', routerLink: '/admin/customers', permission: 'customer.list' }
      ]
    },
    {
      label: 'Vận hành',
      icon: 'pi pi-box',
      routerLink: '/admin/inventory',
      permission: 'bookcopy.list',
      children: [
        { label: 'Tồn kho & bản sao', icon: 'pi pi-box', routerLink: '/admin/inventory', permission: 'bookcopy.list' },
        { label: 'Duyệt đánh giá', icon: 'pi pi-star', routerLink: '/admin/reviews', permission: 'review.list' }
      ]
    },
    {
      label: 'Tiếp thị',
      icon: 'pi pi-bolt',
      routerLink: '/admin/promotions',
      permission: 'coupon.list',
      children: [
        { label: 'Khuyến mại & voucher', icon: 'pi pi-ticket', routerLink: '/admin/promotions', permission: 'coupon.list' },
        { label: 'Blog & bài viết', icon: 'pi pi-file-edit', routerLink: '/admin/articles', permission: 'article.list' }
      ]
    },
    {
      label: 'Quản trị',
      icon: 'pi pi-users',
      routerLink: '/admin/accounts',
      permission: 'account.list',
      children: [
        { label: 'Quản lý tài khoản', icon: 'pi pi-id-card', routerLink: '/admin/accounts', permission: 'account.list' },
        { label: 'Phân quyền', icon: 'pi pi-shield', routerLink: '/admin/permissions', permission: 'permission.view' },
        { label: 'Cấu hình', icon: 'pi pi-sliders-h', routerLink: '/admin/settings', permission: 'setting.view' }
      ]
    }
  ];

  readonly visibleItems = computed(() =>
    this.items
      .map((item) => ({
        ...item,
        children: item.children?.filter((child) => this.authStore.hasPermission(child.permission ?? ''))
      }))
      .filter((item) => {
        if (item.children?.length) {
          // Nhom co menu con: chi hien khi it nhat 1 muc con duoc phep.
          return true;
        }
        return !item.permission || this.authStore.hasPermission(item.permission);
      })
  );
}
