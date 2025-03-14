import { Component, EventEmitter, Input, Output, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { map, Observable, Subscription } from 'rxjs';
import { ProductService } from '../services/product.service';
import { RequestProduct } from '../models/add-product-request.model';
import { CommonModule } from '@angular/common';
import { Product } from '../models/product';
import { Brand } from '../../brand-management/models/brand.model';
import { BrandService } from '../../brand-management/services/brand.service';
import { SpecificationTypeManagementComponent } from '../../specification-type-management/specification-type-management.component';
import { ProductSpecificationWithEditMode } from '../../specification-type-management/models/add-specificationType-request';
import { specificationType } from '../../specification-type-management/models/specificationType';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-or-update-product',
  imports: [FormsModule, CommonModule, SpecificationTypeManagementComponent],
  templateUrl: './add-or-update-product.component.html',
  styleUrls: ['./add-or-update-product.component.css']
})
export class AddOrUpdateProductComponent {

  @Output() close = new EventEmitter<void>();
  @Output() add = new EventEmitter<Product>();
  @Input() productToUpdate?: Product;
  @Input() isAddProductVisible?: Product;
  modelImg: any = {
    imageUrl: ''
  };
  model: RequestProduct;
  brands$?: Observable<Brand[]>;
  selectedColor: string = '';
  specificationTypes$?: Observable<specificationType[]>; // Danh sách SpecificationType
  selectedSpecificationType?: specificationType; // Giá trị được chọn trong select
  addProductSubscription?: Subscription;
  updatedProductSpecifications?: ProductSpecificationWithEditMode[] = [];
  colors: { id: number, color: string }[] = [];
  presetColors: string[] = [
    'red', 'blue', 'green', 'yellow', 'orange',
    'purple', 'pink', 'gray', 'white', 'black',
    'gold', 'silver', 'lavender'
  ];

  isAddSpecificationType = false;

  constructor(private productService: ProductService, private brandService: BrandService, private toastr: ToastrService, private cdRef: ChangeDetectorRef) {
    this.model = {
      name: '',
      imageId: 0,
      image: {
        imageBase64: '',
        name: ''
      },
      brandId: 0,
      colors: '',
      description: '',
      discount: 0,
      manufacturer: '',
      oldPrice: 0,
      price: 0,
      stockQuantity: 0,
      isActive: true,
      productSpecifications: []
    };
  }
  // Hàm xử lý khi người dùng chọn ảnh
  onImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Đọc ảnh và gán vào model.imageUrl
      const reader = new FileReader();
      reader.onload = () => {
        this.model.image.imageBase64 = reader.result as string;
        console.log("gg", this.model)
      };
      reader.readAsDataURL(file);
    }
  }
  ngOnInit(): void {
    this.specificationTypes$ = this.productService.getSpecificationTypes();
    this.specificationTypes$ = this.productService.getSpecificationTypes().pipe();
    this.brands$ = this.brandService.getBrands();
  }

  addColor() {
    if (this.selectedColor) {
      const newColor = { id: Date.now(), color: this.selectedColor }; // Sử dụng timestamp làm id
      this.colors.push(newColor);
      this.selectedColor = ''; // Reset ô nhập
    }
  }

  isColorSelected(color: string): boolean {
    return this.colors.some(c => c.color === color);  // Kiểm tra nếu có màu trùng trong mảng colors
  }

  resetForm() {
    this.model = {
      name: '',
      imageId: 0,
      image: {
        imageBase64: '',
        name: ''
      },
      brandId: 0,
      colors: '',
      description: '',
      discount: 0,
      manufacturer: '',
      oldPrice: 0,
      price: 0,
      stockQuantity: 0,
      isActive: true,
      productSpecifications: []
    };
    this.selectedSpecificationType = undefined;
    this.colors = [];

    this.specificationTypes$ = this.productService.getSpecificationTypes();
  }

  showOnhowOnSpecificationType() {
    this.isAddSpecificationType = true;
    this.specificationTypes$?.subscribe(res => console.log(res));
  }

  selectPresetColor(color: string) {
    const colorIndex = this.colors.findIndex(c => c.color === color);
    if (colorIndex === -1) {
      this.colors.push({ id: Date.now(), color: color });
    } else {
      this.colors.splice(colorIndex, 1);
    }
  }

  removeColor(index: number): void {
    this.colors.splice(index, 1);
  }

  filterSpecificationTypes() {
    this.specificationTypes$ = this.specificationTypes$?.pipe(
      map(types =>
        types.filter(type =>
          !this.model.productSpecifications.some(spec => spec.specificationTypeId === type.specificationTypeId)
        )
      )
    );
  }

  addSpecification() {
    if (this.selectedSpecificationType) {
      this.model.productSpecifications.push({
        specificationTypeId: this.selectedSpecificationType.specificationTypeId,
        value: '',
        specificationType: { specificationTypeId: this.selectedSpecificationType.specificationTypeId, name: this.selectedSpecificationType.name }
      });
      this.filterSpecificationTypes();
      this.selectedSpecificationType = undefined;
    }
  }

  removeSpecification(index: number): void {
    if (index > -1) {
      this.model.productSpecifications.splice(index, 1);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['productToUpdate'] && this.productToUpdate) {
      this.model = this.productToUpdate;
      this.model.image.imageBase64 = 'data:image/jpeg;base64,' + this.productToUpdate.image.imageBase64;
      if (this.model.colors) {
        this.colors = this.model.colors.split(',').map(color => ({
          id: Date.now(),
          color: color.trim()
        }));
      }

      console.log(this.productToUpdate.image);
      this.updatedProductSpecifications = this.model.productSpecifications.map(spec => ({
        ...spec,
        editMode: false
      }));

      console.log(this.updatedProductSpecifications);
    }

    if (changes['specificationTypes$'] && this.specificationTypes$) {
      this.updateSpecificationNames();
    }

    console.log(this.productToUpdate);
    console.log(this.model);
  }
  updateSpecificationNames() {
    if (this.specificationTypes$ && this.model.productSpecifications) {
      this.specificationTypes$.subscribe(specTypes => {
        // Cập nhật lại tên cho mỗi specificationType trong productSpecifications
        this.model.productSpecifications.forEach(spec => {
          const matchedSpecType = specTypes.find(type => type.specificationTypeId === spec.specificationTypeId);
          if (matchedSpecType) {
            spec.specificationType.name = matchedSpecType.name;  // Cập nhật lại tên
          }
        });

        // Đảm bảo thay đổi được phát hiện
        this.cdRef.detectChanges();
      });
    }
  }
  closeModal() {
    this.close.emit();
  }

  submitProduct() {
    this.add.emit();
    this.closeModal();
  }

  onFormSubmit() {
    const form = document.querySelector('form');
    if (form && form.checkValidity() === false) {
      alert('Vui lòng điền đầy đủ thông tin');
    } else {
      this.model.colors = this.colors.length > 0 ? this.colors.map(color => color.color).join(', ') : '';
      if (!this.model.image.imageBase64) {
        this.model.image.imageBase64 = '';
      }
      if (this.productToUpdate) {
        this.updateProduct();
      } else {
        this.addProductSubscription = this.productService.addProduct(this.model).subscribe({
          next: response => {
            this.add.emit();
            this.closeModal();
            this.toastr.success('Sản phẩm đã được thêm thành công!', 'Thành công');
          },
          error: err => {
            console.log(err);
            if (err.error && err.error.Message) {
              this.toastr.error(err.error.Message, 'Lỗi');
            } else {
              this.toastr.error('Đã xảy ra lỗi khi thêm sản phẩm.', 'Lỗi');
            }
          }
        });
      }
    }


  }

  updateProduct() {
    if (this.productToUpdate) {
      this.addProductSubscription = this.productService.updateProduct(this.productToUpdate?.productId, this.model).subscribe({
        next: response => {
          this.closeModal();
          this.add.emit();
          this.toastr.success('Sản phẩm đã được cập nhật thành công!', 'Thành công');
        },
        error: err => {
          console.log(err);
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          } else {
            this.toastr.error('Đã xảy ra lỗi khi cập nhật sản phẩm.', 'Lỗi');
          }
        }
      });
    }
  }

  hideAddSpecificationType() {
    this.isAddSpecificationType = false;
    this.specificationTypes$ = this.productService.getSpecificationTypes();
    this.specificationTypes$.subscribe(res => {
      console.log("hi1", res); // Danh sách specificationTypes nhận được từ service

      // Lặp qua productSpecifications để cập nhật tên của specificationType theo ID
      if (this.model.productSpecifications) {
        this.model.productSpecifications.forEach(spec => {
          // Tìm specificationType tương ứng theo ID
          const matchedSpecType = res.find(type => type.specificationTypeId === spec.specificationTypeId);
          if (matchedSpecType) {
            // Cập nhật tên của specificationType trong productSpecifications
            spec.specificationType.name = matchedSpecType.name;
          }
        });
      }
    }
    )
    console.log("hi2", this.model);
  }
}
