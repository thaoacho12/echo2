import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../../../core/services/toast-service/toast.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, CommonModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  model: any;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastrService: ToastService
  ) {
    this.model = {
      email: ''
    };
  }

  matchValidForm(form: NgForm) {
    const email = form.controls['email']?.value;
    
    // Kiểm tra email hợp lệ
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    if ((email && !emailPattern.test(email)) || email === '' || !email) {
      form.controls['email']?.setErrors({ invalidEmail: true });
    } else {
      form.controls['email']?.setErrors(null);
    }
  }
  onSubmit (form: NgForm):void {
    this.matchValidForm(form);
    if (!form.invalid) {
      this.authService.forgotPassword(this.model.email).subscribe(
        (res)=>{
          if (res.success) {
            this.toastrService.showSuccess(res.message);
          }
          else {
            this.toastrService.showError(res.message);
          }
        },
        (err) => {
          console.log(err);
          
          this.toastrService.showError(err.error.message);
        }
      )
    }
    
  }
}
