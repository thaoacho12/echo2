import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Product } from '../../admin/product-management/models/product';
import { ProductService } from '../product/services/product.service';
import { CartService } from '../cart/service/cart.service';
import { UserClientService } from '../user/service/user-client.service';
import { ToastService } from '../../../core/services/toast-service/toast.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  newestProducts: Product[] = [];
  discountedProducts: Product[] = [];

  constructor(private productService: ProductService, private cartService: CartService, private userService: UserClientService, private toastService: ToastService) { }

  ngOnInit(): void {
    this.loadNewestProducts();
    this.loadDiscountedProducts();
  }

  loadNewestProducts(): void {
    this.productService.newestProducts().subscribe(
      (products) => {
        this.newestProducts = products;
        console.log(products);
        
      },
      (error) => {
        console.error('Error loading newest products:', error);
      }
    );
  }
  loadDiscountedProducts(): void {
    this.productService.discountedProducts().subscribe(
      (products) => {
        this.discountedProducts = products;
      },
      (error) => {
        console.error('Error loading newest products:', error);
      }
    );
  }

  toggleWishList(productId: any) {
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
  }

  addToCart(productId: any) {
    this.cartService.updateCart(productId, 1).subscribe(
      (res) => {
        if (res.success) {
          this.toastService.showSuccess(res.message);
          this.cartService.fetchCartCount();
        } else {
          this.toastService.showError(res.message);
        }
      },
      (err) => {
        console.log(err);
        
      }
    )
  }
}
