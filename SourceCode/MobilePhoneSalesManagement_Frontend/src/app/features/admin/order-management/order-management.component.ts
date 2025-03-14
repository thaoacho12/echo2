import { Component } from '@angular/core';
import { OrderService } from './service/order.service';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast-service/toast.service';
declare var $: any;

@Component({
  selector: 'app-order-management',
  imports: [CommonModule],
  templateUrl: './order-management.component.html',
  styleUrl: './order-management.component.css',
  standalone: true,
})
export class OrderManagementComponent {
  orders: any[] = [];
  selectedOrder: any;

  currentPage: number = 1;
  totalPages: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  pages: number[] = [];
  searchKey: string = '';
  // pagination
  showEllipsisBefore: boolean = false;
  showEllipsisAfter: boolean = false;
  // Gọi API để load danh sách người dùng

  // Cập nhật danh sách các trang cần hiển thị
  updatePagination(): void {
    // Xác định nếu cần hiển thị dấu '...'
    this.showEllipsisBefore = this.currentPage > 2;
    this.showEllipsisAfter = this.currentPage < this.totalPages - 1;
  }

  // Điều hướng tới trang tiếp theo
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagination(); // Cập nhật lại phân trang
      this.loadOrders(); // Gọi lại phương thức tải dữ liệu (nếu cần)
    }
  }

  // Điều hướng tới trang trước
  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagination(); // Cập nhật lại phân trang
      this.loadOrders(); // Gọi lại phương thức tải dữ liệu (nếu cần)
    }
  }

  // Thay đổi số trang hiển thị mỗi trang
  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement; // Ép kiểu sang HTMLSelectElement
    this.pageSize = parseInt(target.value, 10);
    this.currentPage = 1; // Đặt lại về trang đầu khi thay đổi số lượng trang
    this.loadOrders();
  }

  // Thay đổi từ khóa tìm kiếm
  onSearchKeyChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchKey = target.value;
    this.currentPage = 1; // Đặt lại về trang đầu khi thay đổi từ khóa tìm kiếm
    this.loadOrders();
  }

  constructor(
    private orderService: OrderService,
    private toastService: ToastService
  ) {}
  ngOnInit(): void {
    this.loadOrders();
    this.updatePagination();
  }
  loadOrders(): void {
    this.orderService
      .getAllOrders(this.currentPage, this.pageSize, this.searchKey)
      .subscribe(
        (data: any) => {
          this.orders = data.items || [];
          // Cập nhật phân trang
          this.totalCount = data.totalCount;
          this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        },
        (error) => {
          console.error('Error loading users:', error);
        }
      );
  }

  // Các hành động khác
  viewDetails(order: any): void {
    console.log('Chi tiết đơn hàng:', order);
  }
  currentModal: string | null = null;
  modalTitle: string = '';
  modalMessage: string = '';

  selectedOrderId: number | null = null;

  openModal(modalData: { type: string; orderId: number }): void {
    const modalType = modalData.type;
    const orderId = modalData.orderId;

    this.currentModal = modalType;
    this.selectedOrderId = orderId;
    switch (modalType) {
      case 'confirmOrder':
        this.modalTitle = 'Xác nhận đơn hàng';
        this.modalMessage = 'Bạn có chắc muốn xác nhận đơn hàng này?';
        break;
      case 'confirmDelivery':
        this.modalTitle = 'Xác nhận vận chuyển';
        this.modalMessage =
          'Bạn có chắc muốn xác nhận vận chuyển đơn hàng này?';
        break;
      case 'cancelOrder':
        this.modalTitle = 'Hủy đơn hàng';
        this.modalMessage = 'Bạn có chắc muốn hủy đơn hàng này?';
        break;
    }
  }

  closeModal() {
    this.currentModal = null;
    this.selectedOrderId = null;
  }

  confirmAction() {
    if (this.currentModal === 'confirmOrder') {
      this.orderService.confirmOrder(Number(this.selectedOrderId)).subscribe(
        (res) => {
          if (res.success) {
            this.toastService.showSuccess(res.message);
            this.loadOrders();
          } else {
            this.toastService.showError(res.message);
          }
        },
        (error) => {
          alert('Xác nhận đơn hàng thất bại.');
          console.error('Error:', error);
        }
      );
    } else if (this.currentModal === 'confirmDelivery') {
      this.orderService.confirmDelivery(Number(this.selectedOrderId)).subscribe(
        (res) => {
          if (res.success) {
            this.toastService.showSuccess(res.message);
            this.loadOrders();
          } else {
            this.toastService.showError(res.message);
          }
        },
        (error) => {
          alert('Xác nhận vận chuyển thất bại.');
          console.error('Error:', error);
        }
      );
    } else if (this.currentModal === 'cancelOrder') {
      this.orderService.cancelOrder(Number(this.selectedOrderId)).subscribe(
        (res) => {
          if (res.success) {
            this.toastService.showSuccess(res.message);
            this.loadOrders();
          } else {
            this.toastService.showError(res.message);
          }
        },
        (error) => {
          alert('Hủy đơn hàng thất bại.');
          console.error('Error:', error);
        }
      );
    }
    this.closeModal(); // Đóng modal sau khi xác nhận
  }
  capitalizeFirstLetter(value: string): string {
    if (!value) return '';
    return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
  }

  preventClose(event: Event) {
    event.stopPropagation();
  }

  // Mở modal khi click vào button
  openOrderModal(order: any) {
    this.selectedOrder = order;

    const modal: any = document.getElementById('exampleModal');
    $(modal).modal('show'); // Sử dụng jQuery để mở modal
  }

  // Đóng modal
  closeOrderModal() {
    const modal: any = document.getElementById('exampleModal');
    $(modal).modal('hide'); // Sử dụng jQuery để đóng modal
  }
}
