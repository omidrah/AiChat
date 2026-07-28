import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'یک خطای ناشناخته رخ داد';

      if (error.error?.detail) {
        // پیامی که از بک‌بند (ProblemDetails) فرستادیم
        errorMessage = error.error.detail;
      } else if (error.status === 0) {
        errorMessage = 'ارتباط با سرور برقرار نیست';
      }

      // اینجا می‌توانی از یک کتابخانه Toast مثل HotToast یا MatSnackBar استفاده کنی
      alert(errorMessage); // برای سادگی فعلاً alert، اما بهتر است کامپوننت UI باشد

      return throwError(() => error);
    })
  );
};
