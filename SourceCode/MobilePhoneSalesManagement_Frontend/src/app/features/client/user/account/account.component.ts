import { Component, model } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { UserService } from '../../../admin/user-management/services/user.service';
import { ToastService } from '../../../../core/services/toast-service/toast.service';
import dayjs from 'dayjs';
import { CommonModule } from '@angular/common';
import { ValidatorsService } from '../../../../core/services/validators-service/validators.service';
import { AuthService } from '../../../auth/services/auth.service';
import { log } from '@angular-devkit/build-angular/src/builders/ssr-dev-server';
import { Router } from '@angular/router';
import { UserClientService } from '../service/user-client.service';
import { OrderService } from '../../../admin/order-management/service/order.service';

@Component({
  selector: 'app-account',
  imports: [FormsModule, CommonModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.css',
})
export class AccountComponent {
  user = {
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
    dateOfBirth: '',
    gender: 'male',
  };
  orders: any[] = [];
  passwordModel = {
    currentPassword: '',
    password: '',
    re_password: '',
  };
  isDetailView = false;
  selectedOrder: any = null;

  private modelChangePassword: any;

  currentTab: string = 'account-info';

  constructor(
    private router: Router,
    private orderAdminService: OrderService,
    private userService: UserClientService,
    private toastService: ToastService,
    private validateService: ValidatorsService
  ) {}

  ngOnInit(): void {
    this.userService.getCurrentUser().subscribe(
      (res) => {
        this.user.fullName = res.data.fullName;
        this.user.email = res.data.email;
        this.user.phoneNumber = res.data.phoneNumber;
        this.user.address = res.data.address;
        this.user.dateOfBirth = dayjs(res.data.dateOfBirth).format(
          'YYYY-MM-DD'
        );
        this.user.gender = res.data.gender;
      },
      (error) => {
        this.router.navigateByUrl('/login');
      }
    );

    // get orders
    this.loadOrders();
  }

  loadOrders(): void {
    this.userService.getOrders().subscribe(
      (res) => {
        this.orders = res.data;
        console.log(res.data);
      },
      (err) => {
        console.error(err);
      }
    );
  }

  // order tab
  viewOrderDetails(order: any) {
    this.selectedOrder = order;
    this.isDetailView = true;
  }
  goBack() {
    this.isDetailView = false;
    this.selectedOrder = null;
  }
  transform(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Chờ xác nhận';
      case 'Delivered':
        return 'Đang vận chuyển';
      case 'Shipped':
        return 'Đã vận chuyển';
      default:
        return 'Không xác định';
    }
  }
  cancelOrder(orderId: number): void {
    if (confirm('Bạn có chắc chắn muốn hủy đơn hàng này không?')) {
      // Gửi yêu cầu đến API để hủy đơn hàng
      this.orderAdminService.cancelOrder(orderId).subscribe(
        (response) => {
          if (response.success) {
            this.loadOrders();
            this.toastService.showSuccess(response.message);
            this.goBack();
          } else {
            this.toastService.showError(response.message);
          }
        },
        (error) => {
          console.error('Lỗi khi hủy đơn hàng:', error);
          this.toastService.showError(
            'Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại.'
          );
        }
      );
    }
  }

  // account tab
  onSubmit(): void {
    this.userService.updateCurrentUser(this.user).subscribe(
      (res) => {
        if (res.success) {
          this.toastService.showSuccess(res.message);
          setTimeout(() => {
            window.location.reload();
          }, 600);
        } else {
          this.toastService.showError(res.message);
        }
      },
      (error) => {
        this.toastService.showError('Có lỗi bên server');
        console.error('Failed to get user info:', error);
      }
    );
  }

  switchTab(tab: string) {
    this.currentTab = tab;
  }

  // chage password tab
  onChangePassword(form: NgForm) {
    this.validateService.matchPasswords(form);
    // Gửi dữ liệu tới server hoặc xử lý logic khác
    if (!form.invalid) {
      this.modelChangePassword = {
        currentPassword: this.passwordModel.currentPassword,
        newPassword: this.passwordModel.password,
        confirmPassword: this.passwordModel.re_password,
      };
      this.userService.changePassword(this.modelChangePassword).subscribe(
        (res) => {
          if (res.success) {
            this.toastService.showSuccess(res.message);
          } else {
            this.toastService.showError(res.message);
          }
        },
        (err) => {
          console.error('Đổi mật khẩu thất bại:', err);
          if (!err.error.success) {
            this.toastService.showError(err.error.message);
          } else {
            this.toastService.showError('Lỗi gửi yêu cầu');
          }
        }
      );
    }
  }
}
