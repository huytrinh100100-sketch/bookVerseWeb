import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../auth/auth.store';

/** Chặn route khi user thiếu quyền (đọc route.data.permission). */
export const permissionGuard: CanActivateFn = (route) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);
  const required = route.data['permission'] as string | undefined;

  if (!required || authStore.hasPermission(required)) {
    return true;
  }

  return router.createUrlTree(['/admin/forbidden']);
};
