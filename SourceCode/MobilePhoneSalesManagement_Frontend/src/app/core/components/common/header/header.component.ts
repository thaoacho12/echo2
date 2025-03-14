import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../features/auth/services/auth.service';
import { CartService } from '../../../../features/client/cart/service/cart.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  isAuthenticated = false;
  count: number = 0;
  cartItems: any[] = [];
  searchKeyword: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    // Lắng nghe trạng thái xác thực
    this.authService.isAuthenticated.subscribe((status) => {
      this.isAuthenticated = status;
      
      if (this.isAuthenticated) {
        this.cartService.fetchCartCount();
      } else {
        this.count = 0;
      }
    });
  
    // Lắng nghe thay đổi số lượng giỏ hàng
    this.cartService.cartCount$.subscribe((count) => {
      this.count = count;
    });
  }
  

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  onSearch(): void {
    if (this.searchKeyword) {
      // Chuyển hướng đến trang danh sách sản phẩm và gửi từ khóa qua query params
      this.router.navigate(['/products'], {
        queryParams: { search: this.searchKeyword },
      });
    }
  }
}
