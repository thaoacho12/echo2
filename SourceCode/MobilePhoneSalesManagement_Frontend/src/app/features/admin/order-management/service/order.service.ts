import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  constructor(private http: HttpClient) {}

  // Lấy danh sách tất cả các Order
  getAllOrders(currentPage: number, pageSize: number, searchKey?: string): Observable<any> {
    const params: any = {
      page: currentPage,
      pageSize: pageSize,
    };
  
    // Thêm tham số tìm kiếm nếu có
    if (searchKey) {
      params.keySearch = searchKey;
    }
  
    // Gửi yêu cầu HTTP với các tham số
    return this.http.get(`${BASE_URL_API}/order`, { params });
  }
  // Xác nhận đơn hàng
  confirmOrder(orderId: number): Observable<any> {
    return this.http.post(`${BASE_URL_API}/order/${orderId}/confirm`, {});
  }

  // Xác nhận vận chuyển
  confirmDelivery(orderId: number): Observable<any> {
    return this.http.post(`${BASE_URL_API}/order/${orderId}/deliver`, {});
  }

  // Hủy đơn hàng
  cancelOrder(orderId: number): Observable<any> {
    return this.http.post(`${BASE_URL_API}/order/${orderId}/cancel`, {});
  }
}
