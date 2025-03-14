import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../../../core/services/toast-service/toast.service';

@Component({
  selector: 'app-verify-email',
  imports: [],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.css',
})
export class VerifyEmailComponent {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const type = params['type'];
      const email = params['email'];
      const code = params['code'];

      if (type === 'register' || type === 'reset-password') {
        this.callVerifyEmail(type, email, code);
      } else {
        this.router.navigateByUrl('/login');
        this.toastService.showError('Mã xác thực thất bại');
      }
    });
  }

  callVerifyEmail(type: string, email: string, code: string): void {
    this.authService.verifyEmail(email, code).subscribe(
      (response: any) => {
        if (response.success) {
          if (type === 'register') {
            this.toastService.showSuccess('Xác thực thành công');
            window.location.href = '/login';
          } else {
            localStorage.setItem('email', email);
            this.router.navigateByUrl('/reset-password');
          }
          this.toastService.showSuccess(response.message);
        } else {
          this.toastService.showError(response.message);
          this.router.navigateByUrl('/login');
        }
      },
      (error) => {
        console.error('Lỗi khi xác nhận email:', error);
        this.toastService.showError('Xác thực email thất bại');
        this.router.navigateByUrl('/login');
      }
    );
  }
}
