import { Component } from '@angular/core';
import { UserClientService } from '../service/user-client.service';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../../core/services/toast-service/toast.service';

@Component({
  selector: 'app-wishlist',
  imports: [CommonModule],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.css'
})
export class WishlistComponent {
  wishList: any[] = [];  // Khai báo mảng lưu danh sách yêu thích
  loading: boolean = false;  // Biến kiểm tra trạng thái tải dữ liệu
  errorMessage: string = '';  // Biến lưu thông báo lỗi

  constructor(private userService: UserClientService, private toastService: ToastService) {}

  ngOnInit(): void {
    this.fetchWishList();
  }

  // Hàm gọi API để lấy danh sách yêu thích
  fetchWishList(): void {
    this.loading = true;  // Bắt đầu tải
    this.userService.getWishList().subscribe(
      (res) => {
        this.wishList = res;  // Lưu kết quả vào wishList
        this.loading = false;  // Dừng loading
        console.log('Danh sách yêu thích:', this.wishList);
      },
      (err) => {
        this.errorMessage = 'Đã xảy ra lỗi khi tải danh sách yêu thích.';  // Hiển thị thông báo lỗi
        this.loading = false;  // Dừng loading
        console.error('Lỗi khi lấy danh sách yêu thích:', err);
      }
    );
  }

  removeFromWishList(productId: number): void {
    this.userService.toggleWishList(productId).subscribe(
      (res) => {
        if (res.success) {
          this.toastService.showSuccess(res.message);
        } else {
          this.toastService.showError(res.message);
        }
        
      },
      (err) => {
        this.toastService.showError('Lỗi khi gửi yêu cầu');
        console.log(err);
      }
    )
    this.fetchWishList();
  }
}
