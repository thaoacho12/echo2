import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, throwError } from 'rxjs';
import { User } from '../models/user.model';
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { BASE_URL_API } from '../../../app.config';
import { RegisterModel } from '../models/register-model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  $user = new BehaviorSubject<User | undefined>(undefined);
  private _isAuthenticated = new BehaviorSubject<boolean>(false);
  private token: any;

  constructor(private http: HttpClient,
    private cookieService: CookieService
  ) {
    this.token = this.cookieService.get('Authentication');
    this._isAuthenticated.next(!!this.token);
  }


  setUser(user: User): void {
    this.$user.next(user);
    localStorage.setItem('user-email', user.email);
    this._isAuthenticated.next(true);
  }

  user(): Observable<User | undefined> {
    return this.$user.asObservable();
  }

  getUser(): User | undefined {
    const email = localStorage.getItem("user-email");

    if (email) {
      return {
        email: email
      };
    }

    return undefined;
  }
  refreshToken(refreshToken: string): Observable<any> {
    return this.http.post<any>(`${BASE_URL_API}/authentication/refresh-token`, { refreshToken });
  }
  // Phương thức để lấy giá trị cookie theo tên
  getCookie(name: string): string | null {
    return this.cookieService.get(name);  // Trả về giá trị cookie hoặc null nếu không tìm thấy
  }

  // Phương thức kiểm tra cookie có tồn tại hay không
  hasCookie(name: string): boolean {
    return this.cookieService.check(name);  // Trả về true nếu cookie tồn tại
  }

  // Phương thức xóa cookie
  deleteCookie(name: string): void {
    this.cookieService.delete(name);  // Xóa cookie với tên tương ứng
  }
  
  login(request: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/authentication/login-user`, request);
  }

  logout(): void {
    //localStorage.removeItem("user-email");
    localStorage.clear();
    this.cookieService.delete('Authentication', '/');
    this.cookieService.delete('RefreshToken', '/');
    this.$user.next(undefined);
    this._isAuthenticated.next(false);
  }

  register(model: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/authentication/register-user`, model)
      .pipe(
        catchError((error) => {
          // Kiểm tra lỗi trả về từ server và in chi tiết lỗi ra console
          console.error('API Error:', error);
          
          if (error.error && error.error.errors) {
            // Hiển thị các lỗi từ server (ví dụ, lỗi validation)
            console.log('Validation Errors:', error.error.errors);
          }
  
          // Nếu bạn muốn trả về lỗi từ API cho component xử lý
          return throwError(error); // Trả về lỗi cho phần còn lại của code (component, service)
        })
      );
  }

  resetPassword(model: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/authentication/reset-password`, model);
  }

  get isAuthenticated(): Observable<boolean> {
    return this._isAuthenticated.asObservable();
  }
  
  forgotPassword(email: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/authentication/forgot-password?email=${email}`);
  }
  // xác thực mã code khi register
  verifyEmail(email: string, code: string ): Observable<any> {
    return this.http.get(`${BASE_URL_API}/authentication/verify-email?email=${email}&code=${code}`);
  }
  
}
