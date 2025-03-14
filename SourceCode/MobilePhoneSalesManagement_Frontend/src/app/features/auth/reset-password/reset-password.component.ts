import { CommonModule, NgFor } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../../../core/services/toast-service/toast.service';
import { ValidatorsService } from '../../../core/services/validators-service/validators.service';

@Component({
  selector: 'app-reset-password',
  imports: [FormsModule, CommonModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
})
export class ResetPasswordComponent {
  model: any;
  email: string = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private toastService: ToastService,
    private validatorsService: ValidatorsService
  ) {
    this.model = {
      email: this.email,
      password: '',
      confirmPassword: '',
    };
  }
  

  ngOnInit(): void {
    this.email = localStorage.getItem('email') || '';
    this.model.email = this.email;
    if (this.email === '') {
      this.router.navigateByUrl('/');
    }
  }

  onSubmit(form: NgForm) {
    this.validatorsService.matchPasswords(form);
    if (!form.invalid) {
      this.authService.resetPassword(this.model).subscribe(
        (res)=>{
          if (res.success) {
            this.toastService.showSuccess(res.message)
            this.router.navigateByUrl('/login');
          }
          else {
            this.toastService.showError(res.message);
          }
        },
        (err) => {
          this.toastService.showError(err.error.message);
        }
      )
      localStorage.removeItem('email');
    }
  }
}
