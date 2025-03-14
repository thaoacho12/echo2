import { Component } from '@angular/core';
import { ProductService } from './services/product.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AddOrUpdateProductComponent } from "./add-or-update-product/add-or-update-product.component";
import { Product } from './models/product';
import { PagedResult } from '../brand-management/models/brand.model';

@Component({
  selector: 'app-product-management',
  imports: [CommonModule, FormsModule, AddOrUpdateProductComponent],
  templateUrl: './product-management.component.html',
  styleUrls: ['./product-management.component.css'],
  standalone: true
})
export class ProductManagementComponent {



  page$?: Observable<PagedResult<Product>>;
  orderBy = false;
  isAddProductVisible = false;
  productToUpdate?: Product;
  page: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  totalCount: number = 0;
  isLoading: boolean = false;
  filter: string = "ALL";
  // Trạng thái checkbox
  selectAllChecked: boolean = false;
  productCheckboxes: boolean[] = [];
  selectedProductIds: string[] = [];
  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.page$ = this.productService.getProducts(this.page, this.pageSize, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      if (this.totalPages > res.totalPages) this.page = 1;
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
      this.productCheckboxes = Array(res.items.length).fill(false);
      this.selectedProductIds = [];
      console.log("res", res);
    });
    this.isLoading = false;
  }
  loadProductsFilter(filter: boolean): void {
    this.isLoading = true;
    this.page$ = this.productService.filterProductsbyPage(this.page, this.pageSize, filter, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      this.totalCount = res.totalCount;
      if (res.items.length == 0 && res.totalCount > 0) {
        this.page = 1;
        this.onOnwitchloadProducts();
      }
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
      this.productCheckboxes = Array(res.items.length).fill(false);
      this.selectedProductIds = [];
    });
    this.isLoading = false;
  }

  toggleSelectAll(): void {
    this.productCheckboxes = this.productCheckboxes.map(() => this.selectAllChecked);
    this.page$?.subscribe(res => {
      if (this.selectAllChecked) {
        this.selectedProductIds = res.items.map((product) => product.productId)
      } else {
        this.selectedProductIds = []
      }
    });
  }

  updateSelectAllState(): void {
    this.selectAllChecked = this.productCheckboxes.every((checked) => checked);
    this.page$?.subscribe((res) => {
      const selectedIds = res.items
        .filter((_, index) => this.productCheckboxes[index])
        .map((product) => product.productId);

      this.selectedProductIds = Array.from(new Set([...this.selectedProductIds, ...selectedIds]));

      this.selectedProductIds = this.selectedProductIds.filter((id) =>
        res.items.some((product, index) => product.productId === id && this.productCheckboxes[index])
      );
    });
  }
  formatCurrencyVND(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  resetSelectedProductIds() {
    this.selectedProductIds = [];
    this.selectAllChecked = false;
    if (this.page$)
      this.page$.subscribe(res => {
        this.productCheckboxes = Array(res.items.length).fill(false);
        this.selectedProductIds = [];
      });
  }
  deleteSelectedProducts() {
    // console.log(this.selectedProductIds);
    this.onDeleteMultipleProducts();
  }

  onSearchKeyChange($event: Event) {
    const target = $event.target as HTMLSelectElement;
    this.isLoading = true;
    this.page$ = this.productService.searchProductsbyPage(this.page, this.pageSize, target.value, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
    }
    );
    this.isLoading = false;
  }

  onItemsPerPageChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.pageSize = parseInt(target.value, 10);
    this.page = 1;
    this.onOnwitchloadProducts();
  }
  onOnwitchloadProducts(): void {
    if (this.filter == "Active") {
      this.loadProductsFilter(true);
    } else if (this.filter == "Inactive") {
      this.loadProductsFilter(false);
    } else {
      this.loadProducts();
    }
    this.resetSelectedProductIds();
  }
  onItemsFilerChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.filter = target.value;
    this.onOnwitchloadProducts();
  }
  showAddProduct(): void {
    this.isAddProductVisible = true;
  }

  hideAddProduct(): void {
    this.productToUpdate = undefined;
    this.isAddProductVisible = false;
    this.onOnwitchloadProducts();
  }

  onAddProduct(): void {
    this.page = 1;
    this.onOnwitchloadProducts();
  }

  onDeleteProduct(productId: any, isActive: boolean): void {
    const confirmMessage = isActive
      ? 'Bạn chắc chắn muốn chuyển thương hiệu này vào thùng rác?'
      : 'Bạn chắc chắn muốn xóa thương hiệu này?';

    if (confirm(confirmMessage)) {
      this.productService.deleteProduct(productId).subscribe({
        next: () => {
          this.onOnwitchloadProducts();
        },
        error: (err) => console.error(err),
      });
    }
  }
  onDeleteMultipleProducts(): void {
    const confirmMessage = this.filter == "Active"
      ? 'Bạn chắc chắn muốn chuyển các thương hiệu này vào thùng rác?'
      : 'Bạn chắc chắn muốn xóa các thương hiệu này?';
    console.log(this.selectedProductIds);
    if (confirm(confirmMessage)) {
      this.productService.deleteMultipleProducts(this.selectedProductIds).subscribe({
        next: () => {
          this.onOnwitchloadProducts();
          this.resetSelectedProductIds();
        },
        error: (err) => console.error(err),
      });
    }
  }
  onRestoreMultipleProducts(): void {
    this.productService.restoreProducts(this.selectedProductIds).subscribe({
      next: () => {
        this.onOnwitchloadProducts();
      },
      error: (err) => console.error(err),
    });
  }
  updateProduct(productId: string): void {
    this.isAddProductVisible = true;
    this.productService.getProductById(productId).subscribe((product: Product) => {
      this.productToUpdate = product;
      console.log(this.productToUpdate)
      this.onOnwitchloadProducts();
    });
  }
  restoreProduct(productId: string): void {
    this.productService.getProductById(productId).subscribe((product: Product) => {
      if (product) {
        product.isActive = true;
        product.image.imageBase64 = 'data:image/jpeg;base64,' + product.image.imageBase64;
        this.productService.updateProduct(productId, product).subscribe({
          next: response => {
            this.onOnwitchloadProducts();
          },
          error: err => {
            console.log(err);
          }
        });
      }
    });
  }
  // Điều hướng tới trang tiếp theo
  nextPage(): void {
    console.log("fdsfs", this.totalPages)
    if (this.page < this.totalPages) {
      this.page++;
      this.onOnwitchloadProducts();
      this.page$?.subscribe(res =>

        console.log(res)
      )
    }
  }
  changePage(page: number): void {
    this.page = page;
    this.onOnwitchloadProducts();
  }
  // Điều hướng tới trang trước
  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.onOnwitchloadProducts();
    }
  }
  // trackBy để tránh render lại toàn bộ bảng
  trackByProductId(index: number, product: Product): string {
    return product.productId;
  }
  sortField: string = '';

  sort(field: string): void {
    if (this.sortField === field) {
      this.orderBy = !this.orderBy;
    } else {
      this.sortField = field;
      this.orderBy = false;
    }

    // Gọi API hoặc thực hiện logic sắp xếp tại đây
    this.applySorting();
  }

  getSortIcon(field: string): string {
    if (this.sortField === field) {
      return this.orderBy === false
        ? 'fas fa-arrow-up' // Icon mũi tên lên
        : this.orderBy === true
          ? 'fas fa-arrow-down' // Icon mũi tên xuống
          : 'fas fa-sort'; // Icon mặc định
    }
    return 'fas fa-sort'; // Icon sắp xếp mặc định
  }

  applySorting(): void {
    this.onOnwitchloadProducts();
    console.log(`Sorting by ${this.sortField} in ${this.orderBy} order`);
  }

}
