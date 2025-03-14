import { Component } from '@angular/core';
import { BrandService } from './services/brand.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Brand, PagedResult } from './models/brand.model';
import { Observable } from 'rxjs';
import { AddOrUpdateBrandComponent } from "./add-or-update-brand/add-or-update-brand.component";
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-brand-management',
  imports: [CommonModule, FormsModule, AddOrUpdateBrandComponent],
  templateUrl: './brand-management.component.html',
  styleUrls: ['./brand-management.component.css'],
  standalone: true
})
export class BrandManagementComponent {

  sortField: string = '';
  orderBy = false;
  page$?: Observable<PagedResult<Brand>>;
  isAddBrandVisible = false;
  brandToUpdate?: Brand;
  page: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  totalCount: number = 0;
  isLoading: boolean = false;
  filter: string = "ALL";
  selectAllChecked: boolean = false;
  brandCheckboxes: boolean[] = [];
  selectedBrandIds: string[] = [];
  model = {
    imageUrl: '',
  };

  constructor(
    private brandService: BrandService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadBrands();
  }
  onImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.model.imageUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }
  loadBrands(): void {
    this.isLoading = true;
    this.page$ = this.brandService.getBrandsbyPage(this.page, this.pageSize, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      if (this.totalPages > res.totalPages) this.page = 1;
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
      this.brandCheckboxes = Array(res.items.length).fill(false);
      this.selectedBrandIds = [];
      console.log(res)
    });
    this.isLoading = false;
  }

  loadBrandsFilter(filter: boolean): void {
    this.isLoading = true;
    this.page$ = this.brandService.filterBrandsbyPage(this.page, this.pageSize, filter, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      this.totalCount = res.totalCount;
      if (res.items.length == 0 && res.totalCount > 0) {
        this.page = 1;
        this.onOnwitchloadBrands();
      }
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
      this.brandCheckboxes = Array(res.items.length).fill(false);
      this.selectedBrandIds = [];
      if (res.items[1]?.image?.imageBase64) {
        this.model.imageUrl = 'data:image/jpeg;base64,/9j/' + res.items[1].image.imageBase64;
      } else {
        this.model.imageUrl = ''; // Giá trị mặc định nếu không có imageBase64
      }
      console.log(this.model.imageUrl);

      console.log(this.model.imageUrl);
      console.log(res.items);

    });
    this.isLoading = false;
  }

  toggleSelectAll(): void {
    this.brandCheckboxes = this.brandCheckboxes.map(() => this.selectAllChecked);
    this.page$?.subscribe(res => {
      if (this.selectAllChecked) {
        this.selectedBrandIds = res.items.map((brand) => brand.brandId)
      } else {
        this.selectedBrandIds = []
      }
    });
  }

  updateSelectAllState(): void {
    this.selectAllChecked = this.brandCheckboxes.every((checked) => checked);
    this.page$?.subscribe((res) => {
      const selectedIds = res.items
        .filter((_, index) => this.brandCheckboxes[index])
        .map((brand) => brand.brandId);

      this.selectedBrandIds = Array.from(new Set([...this.selectedBrandIds, ...selectedIds]));
      this.selectedBrandIds = this.selectedBrandIds.filter((id) =>
        res.items.some((brand, index) => brand.brandId === id && this.brandCheckboxes[index])
      );
    });
  }

  resetSelectedBrandIds() {
    this.selectedBrandIds = [];
    this.selectAllChecked = false;
    if (this.page$)
      this.page$.subscribe(res => {
        this.brandCheckboxes = Array(res.items.length).fill(false);
        this.selectedBrandIds = [];
      });
  }

  deleteSelectedBrands() {
    this.onDeleteMultipleBrands();
  }

  onSearchKeyChange($event: Event) {
    const target = $event.target as HTMLSelectElement;
    this.isLoading = true;
    this.page$ = this.brandService.searchBrandsbyPage(this.page, this.pageSize, target.value, this.sortField, this.orderBy);
    this.page$.subscribe(res => {
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;
      console.log(res)
    });
    this.isLoading = false;
  }

  onItemsPerPageChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.pageSize = parseInt(target.value, 10);
    this.page = 1;
    this.onOnwitchloadBrands();
  }

  onOnwitchloadBrands(): void {
    if (this.filter == "Active") {
      this.loadBrandsFilter(true);
    } else if (this.filter == "Inactive") {
      this.loadBrandsFilter(false);
    } else {
      this.loadBrands();
    }
    this.resetSelectedBrandIds();
  }

  onItemsFilerChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.filter = target.value;
    this.onOnwitchloadBrands();
  }

  showAddBrand(): void {
    this.isAddBrandVisible = true;
  }

  hideAddBrand(): void {
    this.brandToUpdate = undefined;
    this.isAddBrandVisible = false;
    this.onOnwitchloadBrands();
  }

  onAddBrand(): void {
    this.page = 1;
    this.onOnwitchloadBrands();
    this.toastr.success('Thương hiệu đã được thêm thành công!', 'Thành công');
  }

  onDeleteBrand(brandId: any, isActive: boolean): void {
    const confirmMessage = isActive
      ? 'Bạn chắc chắn muốn chuyển thương hiệu này vào thùng rác?'
      : 'Bạn chắc chắn muốn xóa thương hiệu này?';

    if (confirm(confirmMessage)) {
      this.brandService.deleteBrand(brandId).subscribe({
        next: () => {
          this.toastr.success('Thương hiệu đã được xóa thành công!', 'Thành công');
          this.onOnwitchloadBrands();
        },
        error: (err) => {
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          }
          this.toastr.error('Đã xảy ra lỗi khi xóa thương hiệu.', 'Lỗi');
          console.error(err);
        }
      });
    }
  }

  onDeleteMultipleBrands(): void {
    const confirmMessage = this.filter == "Active"
      ? 'Bạn chắc chắn muốn chuyển các thương hiệu này vào thùng rác?'
      : 'Bạn chắc chắn muốn xóa các thương hiệu này?';

    if (confirm(confirmMessage)) {
      this.brandService.deleteMultipleBrands(this.selectedBrandIds).subscribe({
        next: () => {
          this.toastr.success('Các thương hiệu đã được xóa thành công!', 'Thành công');
          this.onOnwitchloadBrands();
          this.resetSelectedBrandIds();
        },
        error: (err) => {
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          }
          this.toastr.error('Đã xảy ra lỗi khi xóa các thương hiệu.', 'Lỗi');
          console.error(err);
        }
      });
    }
  }

  onRestoreMultipleBrands(): void {
    this.brandService.restoreBrands(this.selectedBrandIds).subscribe({
      next: () => {
        this.toastr.success('Các thương hiệu đã được khôi phục thành công!', 'Thành công');
        this.onOnwitchloadBrands();
      },
      error: (err) => {
        if (err.error && err.error.Message) {
          this.toastr.error(err.error.Message, 'Lỗi');
        }
        this.toastr.error('Đã xảy ra lỗi khi khôi phục thương hiệu.', 'Lỗi');
        console.error(err);
      }
    });
  }

  updateBrand(brandId: string): void {
    this.isAddBrandVisible = true;
    this.brandService.getBrandById(brandId).subscribe((brand: Brand) => {
      this.brandToUpdate = brand;
      this.onOnwitchloadBrands();
      console.log(brand)
    });
  }

  restoreBrand(brandId: string): void {
    this.brandService.getBrandById(brandId).subscribe((brand: Brand) => {

      console.log("r", brandId, brand)
      if (brand.isActive == false) {
        brand.isActive = true;
        brand.image.imageBase64 = 'data:image/jpeg;base64,' + brand.image.imageBase64;
        console.log("r", brandId, brand)
        this.brandService.updateBrand(brandId, brand).subscribe({
          next: response => {
            this.toastr.success('Thương hiệu đã được khôi phục thành công!', 'Thành công');
            this.onOnwitchloadBrands();
          },
          error: err => {
            if (err.error && err.error.Message) {
              this.toastr.error(err.error.Message, 'Lỗi');
            }
            this.toastr.error('Đã xảy ra lỗi khi khôi phục thương hiệu.', 'Lỗi');
            console.log(err);
          }
        });
      }
    });
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.onOnwitchloadBrands();
    }
  }
  changePage(page: number): void {
    this.page = page;
    this.onOnwitchloadBrands();
  }
  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.onOnwitchloadBrands();
    }
  }

  trackByBrandId(index: number, brand: Brand): string {
    return brand.brandId;
  }
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
    this.onOnwitchloadBrands();
    console.log(`Sorting by ${this.sortField} in ${this.orderBy} order`);
  }
}
