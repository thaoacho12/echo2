import { Component, Input, SimpleChanges } from '@angular/core';
import { UserService } from '../services/user.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../../core/services/toast-service/toast.service';
import dayjs from 'dayjs';

@Component({
  selector: 'app-user-edit',
  imports: [FormsModule, CommonModule],
  templateUrl: './user-edit.component.html',
  styleUrl: './user-edit.component.css',
})
export class UserEditComponent {
  @Input() user: any = {};

  isModalOpen = false; // Biến để điều khiển việc hiển thị modal
  action: 'delete' = 'delete';

  constructor(
    private userService: UserService,
    private toastService: ToastService
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['user'] && this.user.email != null) {
      this.user.dateOfBirth = dayjs(this.user.dateOfBirth).format('YYYY-MM-DD');
      this.user.role = this.user.role.toLowerCase();
    }
  }

  // Mở modal xác nhận xóa người dùng
  openModal(action: 'delete') {
    if (action === 'delete') {
      this.isModalOpen = true; // Mở modal xác nhận xóa
    }
  }

  // Đóng modal
  closeModal() {
    this.isModalOpen = false;
  }

  confirmAction() {
    if (this.action === 'delete') {
      this.deleteUserById();
    }
    this.closeModal(); // Đóng modal sau khi thực hiện hành động
  }

  // Xóa người dùng
  deleteUserById(): void {
    this.userService.deleteUserById(this.user.userId).subscribe(
      (res) => {
        alert('Xóa người dùng thành công.');
        window.location.reload();
        this.closeModal(); // Đóng modal sau khi xóa thành công
      },
      (error) => {
        console.error(error);
        alert('Đã có lỗi xảy ra khi xóa người dùng.');
        this.closeModal(); // Đóng modal nếu có lỗi
      }
    );
  }

  submitForm() {
    if (
      !this.user.email ||
      !this.user.passwordHash ||
      !this.user.fullName ||
      !this.user.dateOfBirth ||
      !this.user.address ||
      !this.user.phoneNumber
    ) {
      this.toastService.showError('Vui lòng nhập đủ thông tin');
      return;
    }

    // Chuyển `DateOfBirth` về định dạng yyyy-MM-dd
    this.user.dateOfBirth = dayjs(this.user.dateOfBirth).format('YYYY-MM-DD');

    this.user.status = Boolean(this.user.status);

    this.userService.updateUser(this.user.userId, this.user).subscribe(
      (response) => {
        this.toastService.showSuccess('Cập nhật thành công!');
        setTimeout(function () {
          window.location.reload();
        }, 600);
      },
      (error) => {
        console.log(error);

        alert(error.error.message);
      }
    );
  }
}
