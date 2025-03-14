import { Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class ValidatorsService {
  matchPasswords(form: NgForm): void {
    
    const password = form.controls['password']?.value;
    const confirmPassword = form.controls['re_password']?.value;
    const email = form.controls['email']?.value;

    // Kiểm tra email hợp lệ
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    if (email && !emailPattern.test(email) || email === '') {
      form.controls['email']?.setErrors({ invalidEmail: true });
    } else {
      form.controls['email']?.setErrors(null);
    }

    // Kiểm tra mật khẩu (ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường và chữ số)
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/;
    if (password && !passwordPattern.test(password) || password === '') {
      form.controls['password']?.setErrors({ weakPassword: true });
    } else {
      form.controls['password']?.setErrors(null);
    }

    // Kiểm tra xác nhận mật khẩu
    if (password && confirmPassword && password !== confirmPassword || confirmPassword === '') {
      form.controls['re_password']?.setErrors({ noMatch: true });
    } else {
      form.controls['re_password']?.setErrors(null);
    }
  }
}
