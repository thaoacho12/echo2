import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { FormBuilder, FormGroup, FormsModule, NgForm, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast-service/toast.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ValidatorsService } from '../../../core/services/validators-service/validators.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule , CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  email: string = '';
  code: string = '';

  user = {
    email: '',
    password: '',
    re_password: '',
    fullName: '',
    phoneNumber: '',
  };
  userModelSubmit: any
  

  constructor(private toastService: ToastService, private authService: AuthService, private route: ActivatedRoute, private router: Router, private validatorsService: ValidatorsService) {
  }
  ngOnInit(): void {
  }


  onSubmit(form: NgForm) {
    
    this.validatorsService.matchPasswords(form);

    if (!form.invalid) {
      this.userModelSubmit = {
        email: this.user.email,
        passwordHash: this.user.password,
        status: true,
        role: 'client',
        fullName: this.user.fullName,
        dateOfBirth: new Date().toISOString().split('T')[0],
        gender: 'Nam',
        address: '',
        phoneNumber: this.user.phoneNumber,
        lastOnlineAt: new Date()
      };
      
      this.authService.register(this.userModelSubmit).subscribe(
        (res)=> {
          if (res.success) {
            this.toastService.showSuccess(res.message);
          } else {
            this.toastService.showError(res.message);
          }
        },
        (error) => {
          this.toastService.showError(error.Message)
        }
      )
    }
  }
}