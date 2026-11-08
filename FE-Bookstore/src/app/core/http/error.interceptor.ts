import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { ToastService } from '../services/toast.service';

/** 401 -> dang xuat va ve trang login. 403 -> thong bao khong du quyen. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authStore.clearSession();
        router.navigate(['/login'], { queryParams: { returnUrl: req.url } });
      } else if (error.status === 403) {
        toast.error(error.error?.message ?? 'Bạn không có quyền thực hiện thao tác này.');
      }

      return throwError(() => error);
    })
  );
};
