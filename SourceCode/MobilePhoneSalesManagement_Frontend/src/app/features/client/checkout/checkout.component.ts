import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrderService } from './service/order.service';
import { error } from 'jquery';
import { ToastService } from '../../../core/services/toast-service/toast.service';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent {
  cartItems: any[] = [];
  discountAmount: number = 0;
  totalAmount: number = 0;

  formData: any;

  constructor(private router: Router, private orderService: OrderService, private toastService: ToastService) {
    this.formData = {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      country: '',
      address: '',
      postcode: '',
      note: '',
      isRegister: false,
    };
  }

  ngOnInit(): void {
    // Lấy dữ liệu từ sessionStorage
    const storedData = sessionStorage.getItem('checkoutData');
    if (storedData) {
      const { cartItems, discountAmount, totalAmount } = JSON.parse(storedData);
      this.cartItems = cartItems || [];
      this.discountAmount = discountAmount || 0;
      this.totalAmount = totalAmount || 0;
    }
  }

  onSubmit(): void {
    // Dữ liệu từ form
    const checkoutData = {
      cartItems: this.cartItems.map((item: any) => ({
        productId: item.productId,
        quantity: item.quantity,
        price: item.price,
        color: item.color
      })),
      discountAmount: this.discountAmount,
      totalAmount: this.totalAmount,
      orderInfo: this.formData
    };
    const data = JSON.stringify(checkoutData);
    const dataSend = JSON.parse(data);
    
    this.orderService.createOrder(dataSend).subscribe(
      (res) => {
        if (res.success) {
          this.toastService.showSuccess(res.message);
        } else {
          this.toastService.showError(res.message);
        }
        
      },
      (err) => {
        console.error(err);
      }
    )
  }
}
