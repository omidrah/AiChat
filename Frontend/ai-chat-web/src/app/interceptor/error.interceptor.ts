import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'یک خطای ناشناخته رخ داد';

      if (error.error?.detail) {
        message = error.error.detail;
      } else if (error.status === 0) {
        message = 'ارتباط با سرور برقرار نشد';
      } else if (error.status === 404) {
        message = 'منبع موردنظر پیدا نشد';
      } else if (error.status === 503) {
        message = 'سرویس هوش مصنوعی در دسترس نیست';
      }

      snackBar.open(message, 'بستن', {
        duration: 4000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        direction: 'rtl'
      });

      return throwError(() => error);
    })
  );
};
