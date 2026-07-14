import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/AuthService';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  //console.log("INTERCEPTOR");
  const auth = inject(AuthService);
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
  }
  else if (mode === 'windows') {
    req = req.clone({
      withCredentials: true
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
     
      if (mode === 'windows') {
        return throwError(() => error);
      }
      
      if (error.status !== 401 || isAuthRequest || !auth.getRefreshToken()) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap(() => {
          const newToken = auth.getToken();

          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`
            }
          });

          return next(retryReq);
        }),
        catchError(refreshError => {
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
