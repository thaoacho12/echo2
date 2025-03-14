import { Component, Input, OnInit } from '@angular/core';
import { SpecificationTypesService } from './services/specification-types.service';
import { Observable } from 'rxjs';
import { specificationType } from './models/specificationType';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-specification-type-management',
  imports: [FormsModule, CommonModule],
  templateUrl: './specification-type-management.component.html',
  styleUrls: ['./specification-type-management.component.css']
})
export class SpecificationTypeManagementComponent implements OnInit {
  @Input() specificationTypes?: Observable<specificationType[]>;

  editMode: boolean[] = [];
  isTemporary: boolean[] = [];
  newSpecificationName: string = '';
  isAddingNew: boolean = false;

  constructor(
    private specificationTypesService: SpecificationTypesService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.specificationTypes = this.specificationTypesService.getSpecificationTypess();
  }

  toggleEditMode(index: number): void {
    this.isTemporary[index] = true;
    this.editMode[index] = true;
  }

  startAddingNew() {
    this.isAddingNew = true;
  }

  addSpecification(): void {
    if (this.newSpecificationName.trim()) {
      this.specificationTypesService.addSpecificationTypes({
        "name": this.newSpecificationName
      }).subscribe({
        next: () => {
          this.toastr.success('Thêm đặc tính thành công!', 'Thành công');
          this.newSpecificationName = ''; // Reset sau khi thêm xong
          this.cancelAddingNew();
          this.load();
        },
        error: (err) => {
          if (err.error && err.error.Message) {
            this.toastr.error(err.error.Message, 'Lỗi');
          } else {
            this.toastr.error('Đã xảy ra lỗi khi thêm đặc tính.', 'Lỗi');
          }
        }
      });
    }
  }

  cancelAddingNew() {
    this.isAddingNew = false;
  }

  async saveSpecification(index: number, spec: specificationType): Promise<void> {
    await this.specificationTypesService.updateSpecificationTypes(parseInt(spec.specificationTypeId), {
      "name": spec.name
    }).subscribe({
      next: () => {
        this.editMode[index] = false;
        this.toastr.success('Cập nhật đặc tính thành công!', 'Thành công');
        this.load();
      },
      error: (err) => {
        if (err.error && err.error.Message) {
          this.toastr.error(err.error.Message, 'Lỗi');
        } else {
          this.toastr.error('Đã xảy ra lỗi khi cập nhật đặc tính.', 'Lỗi');
        }
      }
    });
  }

  deleteSpecification(index: number): void {
    if (this.isTemporary[index]) {
      // Nếu là mục tạm thời, chỉ xóa trong danh sách
      this.specificationTypes?.subscribe(specs => {
        specs.splice(index, 1);
        this.editMode.splice(index, 1);
        this.isTemporary.splice(index, 1);
        this.toastr.success('Đã xóa mục tạm thời.', 'Thành công');
      });
    } else {
      this.specificationTypes?.subscribe(specs => {
        const specToDelete = specs[index];
        this.specificationTypesService.deleteSpecificationTypes(specToDelete.specificationTypeId).subscribe({
          next: () => {
            specs.splice(index, 1);
            this.editMode.splice(index, 1);
            this.isTemporary.splice(index, 1);
            this.toastr.success('Đặc tính đã được xóa thành công!', 'Thành công');
            this.load();
          },
          error: (err) => {
            if (err.error && err.error.Message) {
              this.toastr.error(err.error.Message, 'Lỗi');
            } else {
              this.toastr.error('Đã xảy ra lỗi khi xóa đặc tính.', 'Lỗi');
            }
          }
        });
      });
    }
  }
}
