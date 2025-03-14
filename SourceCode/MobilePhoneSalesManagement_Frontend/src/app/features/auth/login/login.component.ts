import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { LoginRequest } from '../models/login-request.model';
import { AuthService } from '../services/auth.service';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast-service/toast.service';
import { error } from 'jquery';
import { log } from '@angular-devkit/build-angular/src/builders/ssr-dev-server';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  loginData: any;

  constructor(
    private authService: AuthService,
    private cookieService: CookieService,
    private router: Router,
    private toastr: ToastService
  ) {
    this.loginData = {
      email: '',
      password: '',
    };
  }

  matchValidForm(form: NgForm) {
    const email = form.controls['email']?.value;

    // Kiểm tra email hợp lệ
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    if ((email && !emailPattern.test(email)) || email === '') {
      form.controls['email']?.setErrors({ invalidEmail: true });
    } else {
      form.controls['email']?.setErrors(null);
    }
  }

  onSubmit(form: NgForm): void {
    this.matchValidForm(form);

    if (!form.invalid) {
      this.authService.login(this.loginData).subscribe({
        next: (res) => {
          if (res.token) {
            this.cookieService.set(
              'Authentication',
              `Bearer ${res.token}`,
              undefined,
              '/',
              undefined,
              true,
              'Strict'
            );
            this.cookieService.set(
              'RefreshToken',
              res.refreshToken,
              undefined,
              '/',
              undefined,
              true,
              'Strict'
            );
            this.authService.setUser({ email: this.loginData.email });
            this.router.navigateByUrl('/');
          } else {
            this.toastr.showError('Thông tin không chính xác');
            return;
          }
        },
        error: (err) => {
          if (!err.error.success && err.error.message) {
            this.toastr.showError(err.error.message);
          } else {
            console.log(err.error);
            this.toastr.showError('Lỗi kết nối server');
          }
        },
      });
    } else {
      this.toastr.showError('Vui lòng nhập đủ thông tin');
    }
  }

  // Trạng thái hiển thị mật khẩu
  showPassword = false;

  // Hàm toggle trạng thái
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
