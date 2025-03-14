import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { Brand } from '../models/brand.model';
import { BrandService } from '../services/brand.service';
import { RequestBrand } from '../models/add-brand-request.model';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-or-update-brand',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-or-update-brand.component.html',
  styleUrl: './add-or-update-brand.component.css'
})
export class AddOrUpdateBrandComponent {
  @Output() close = new EventEmitter<void>();
  @Output() add = new EventEmitter<string>();
  @Input() brandToUpdate?: Brand;

  model: RequestBrand;
  addBrandSubscription?: Subscription;

  constructor(private brandService: BrandService, private toastr: ToastrService) {
    this.model = {
      name: '',
      isActive: true,
      imageId: 0,
      image: {
        imageBase64: '',
        name: ''
      }
    };
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['brandToUpdate'] && this.brandToUpdate) {
      this.model.name = this.brandToUpdate.name;
      this.model.isActive = this.brandToUpdate.isActive;
      this.model.imageId = this.brandToUpdate.imageId;
      this.model.image.imageBase64 = 'data:image/jpeg;base64,' + this.brandToUpdate.image.imageBase64;
      console.log(this.brandToUpdate);
    }
  }
  // Hàm xử lý khi người dùng chọn ảnh
  onImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {

      console.log("gg", this.model)
      const reader = new FileReader();
      reader.onload = () => {
        this.model.image.imageBase64 = reader.result as string;
        console.log("gg", this.model)
      };
      reader.readAsDataURL(file);
    }
  }
  closeModal() {
    this.close.emit();
  }

  onFormSubmit() {

    console.log("gg", this.model)
    if (this.brandToUpdate) {
      this.updateBrand();
    } else {
      this.addBrandSubscription = this.brandService.addBrand(this.model).subscribe({
        next: response => {
          this.add.emit(this.model.name);
          this.closeModal();
        },
        error: err => {
          console.log(err);
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          } else {
            this.toastr.error('Đã xảy ra lỗi khi thêm thương hiệu.', 'Lỗi');
          }
        }
      });
    }
  }


  updateBrand() {
    if (this.brandToUpdate) {
      this.addBrandSubscription = this.brandService.updateBrand(this.brandToUpdate.brandId, this.model).subscribe({
        next: response => {
          this.toastr.success('Thương hiệu đã được cập nhật thành công!', 'Thành công');
          this.add.emit(this.model.name);
          this.closeModal();
        },
        error: err => {
          console.log(err);
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          } else {
            this.toastr.error('Đã xảy ra lỗi khi thêm thương hiệu.', 'Lỗi');
          }
        }
      });
    }
  }
}
