import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../services/product.service';
import { UserClientService } from '../../user/service/user-client.service';
import { error } from 'jquery';
import { ToastService } from '../../../../core/services/toast-service/toast.service';
import { CartService } from '../../cart/service/cart.service';
import { BASE_URL_API } from '../../../../app.config';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit {

  path: string = '';
  products: any[] = [];
  brands: string[] = ['Samsung', 'Apple', 'Xiaomi', 'Vsmart', 'Oppo', 'Vivo', 'Nokia', 'Huawei'];
  priceRanges = [
    { label: 'Dưới 2 triệu', value: 'under2m' },
    { label: '2 - 5 triệu', value: '2to5m' },
    { label: '5 - 10 triệu', value: '5to10m' },
    { label: '10 - 15 triệu', value: '10to15m' },
    { label: 'Trên 15 triệu', value: 'above15m' }
  ];
  screenSizes = [
    { label: 'Dưới 5 inch', value: 'under5' },
    { label: 'Trên 6 inch', value: 'above6' }
  ];
  internalMemories = [
    { label: 'Dưới 32GB', value: 'under32' },
    { label: '64GB hoặc 128GB', value: '64and128' },
    { label: '256GB hoặc 512GB', value: '256and512' },
    { label: 'Trên 512GB', value: 'above512' }
  ];
  filterRequest: any = {
    Search: '',
    Brands: [],
    Prices: [],
    ScreenSizes: [],
    InternalMemory: [],
    Sort: '',
    PageNumber: 1,
    PageSize: 15
  };
  totalPages: number = 1; // Tổng số trang

  constructor(private productService: ProductService, private userService: UserClientService, private cartService: CartService, private toastService: ToastService, private route: ActivatedRoute) { }
  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['search']) {
        this.filterRequest.search = params['search'];
      }
      this.getFilteredProducts();
    });

    this.path = this.route.snapshot.url.join('/');
  }
  getFilteredProducts() {
    console.log('filterRequest:', this.filterRequest);
    this.productService.filterProducts(this.filterRequest).subscribe(
      (data) => {
        console.log('Data:', data);
        console.log('Product:', data.products);
        console.log('totalspage:', data.totalPages);
        // Kiểm tra dữ liệu trả về
        if (data && Array.isArray(data.products)) {
          this.products = data.products;
          this.totalPages = data.totalPages;  // đảm bảo totalPages có giá trị mặc định
          this.calculatePagination(); // Tính toán danh sách phân trang
        } else {
          console.error('Dữ liệu trả về không có trường Products hoặc không phải mảng.');
        }
      },
      (error) => {
        console.error('Lỗi khi lấy danh sách sản phẩm:', error);
      }
    );
  }
  onBrandFilterChange(event: any) {
    const brand = event.target.value;
    if (event.target.checked) {
      this.filterRequest.Brands.push(brand);
    } else {
      this.filterRequest.Brands = this.filterRequest.Brands.filter((b: string) => b !== brand);
    }
    this.getFilteredProducts();
  }
  onPriceFilterChange(event: any) {
    const price = event.target.value;
    if (event.target.checked) {
      this.filterRequest.Prices.push(price);
    } else {
      this.filterRequest.Prices = this.filterRequest.Prices.filter((p: string) => p !== price);
    }
    this.getFilteredProducts();
  }
  onScreenSizeFilterChange(event: any) {
    const screenSize = event.target.value;
    if (event.target.checked) {
      this.filterRequest.ScreenSizes.push(screenSize);
    } else {
      this.filterRequest.ScreenSizes = this.filterRequest.ScreenSizes.filter((s: string) => s !== screenSize);
    }
    this.getFilteredProducts();
  }
  onInternalMemoryFilterChange(event: any) {
    const memory = event.target.value;
    if (event.target.checked) {
      this.filterRequest.InternalMemory.push(memory);
    } else {
      this.filterRequest.InternalMemory = this.filterRequest.InternalMemory.filter((m: string) => m !== memory);
    }
    this.getFilteredProducts();
  }
  onSortChange(event: any) {
    this.filterRequest.Sort = event.target.value;
    this.getFilteredProducts();
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

  addToCart(productId: number) {
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
  changePage(pageNumber: number) {
    if (pageNumber < 1 || pageNumber > this.totalPages) return; // Kiểm tra số trang hợp lệ
    this.filterRequest.PageNumber = pageNumber;
    this.getFilteredProducts();
  }
  pagination: any[] = [];

  calculatePagination() {
    const current = this.filterRequest.PageNumber;
    const total = this.totalPages;
    const delta = 2; // Số trang hiển thị xung quanh trang hiện tại
    const range = [];
    const rangeWithDots: any[] = [];
    let l: number;

    // Xác định khoảng hiển thị
    for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
      range.push(i);
    }

    // Thêm các trang đầu, cuối và dấu "..."
    if (current - delta > 2) {
      rangeWithDots.push(1, '...');
    } else {
      rangeWithDots.push(1);
    }

    range.forEach((i) => {
      rangeWithDots.push(i);
    });

    if (current + delta < total - 1) {
      rangeWithDots.push('...', total);
    } else if (total > 1) {
      rangeWithDots.push(total);
    }

    this.pagination = rangeWithDots;
  }
}
