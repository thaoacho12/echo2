import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { AuthService } from '../../features/auth/services/auth.service';
import { Router } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const cookieService = inject(CookieService);
  const authToken = cookieService.get('Authentication');  // Lấy token từ cookie
  const refreshToken = cookieService.get('RefreshToken');  // Lấy refresh token từ cookie
  const authService = inject(AuthService);
  const router = inject(Router);

  // Nếu token không có, không gửi yêu cầu
  if (!authToken) {
    return next(req);
  }

  // Thêm token vào header của yêu cầu
  const authReq = req.clone({
    setHeaders: {
      'Authorization': authToken
    }
  });
  // Gửi yêu cầu HTTP
  return next(authReq).pipe(
    
    catchError((error) => {
      // Nếu gặp lỗi 401 (Unauthorized) và có refreshToken, thực hiện làm mới token
      if (error.status === 401 && refreshToken) {
        return authService.refreshToken(refreshToken).pipe(
          switchMap((res) => {
            // Lưu token mới vào cookie
            cookieService.set('Authentication', `Bearer ${res.token}`);
            cookieService.set('RefreshToken', res.refreshToken);

            // Tạo lại yêu cầu ban đầu với token mới
            const newReq = req.clone({
              setHeaders: {
                'Authorization': `Bearer ${res.token}`
              }
            });

            // Tiếp tục gửi lại yêu cầu với token mới
            return next(newReq);
          }),
          catchError((refreshError) => {
            // Nếu refresh token không thành công, logout và chuyển hướng đến trang login
            cookieService.delete('Authentication');
            cookieService.delete('RefreshToken');
            router.navigate(['/login']);
            return of(refreshError);  // Trả về error nếu không thể làm mới token
          })
        );
      }
      
      // trường hợp client chưa xóa RefreshToken mà sang bên server đã bị xóa
      if (error.status == 440) {
        cookieService.delete('Authentication');
        cookieService.delete('RefreshToken');
        router.navigate(['/login']);
        return of(error);
      }

      // Nếu không phải lỗi 401, ném lỗi gốc
      return of(error);
    })
  );
};