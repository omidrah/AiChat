// auth.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/AuthService';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router); 
  const token = auth.getToken();
  const mode = auth.getAuthMode();
     
  const isAuthRequest =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/refresh') ||
    req.url.includes('/auth/mode');

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  } else if (mode === 'windows') {
    req = req.clone({
      withCredentials: true
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (mode === 'windows') {
        return throwError(() => error);
      }
      
      // اگر خطا ۴۰۱ بود و درخواست لاگین/رفرش نبود
      if (error.status === 401 && !isAuthRequest) {
        const refreshToken = auth.getRefreshToken();
        
        if (refreshToken) {
          // تلاش برای رفرش کردن توکن
          return auth.refresh().pipe(
            switchMap((res) => {
              const newToken = res.access_token;
              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(retryReq);
            }),
            catchError((refreshError) => {
              // اگر خود رفرش توکن هم به هر دلیل خطا خورد (مثلا اکسپایر شدن رفرش توکن)
              auth.logout();
              router.navigate(['/login']); // 👈 هدایت به صفحه لاگین
              return throwError(() => refreshError);
            })
          );
        } else {
          // اگر رفرش توکن نداشتیم، مستقیم کاربر را خارج کن
          auth.logout();
          router.navigate(['/login']);
        }
      }

      return throwError(() => error);
    })
  );
};
