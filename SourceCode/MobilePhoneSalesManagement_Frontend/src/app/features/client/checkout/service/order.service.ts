import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private http: HttpClient) {}

  // Lấy danh sách tất cả các Order
  getAllOrders(): Observable<any> {
    return this.http.get(`${BASE_URL_API}/order`);
  }

  // Lấy thông tin chi tiết Order theo ID
  getOrderById(orderId: number): Observable<any> {
    return this.http.get(`${BASE_URL_API}/order/${orderId}`);
  }

  // Tạo mới Order
  createOrder(orderData: any): Observable<any> {
    console.log(orderData);
    
    return this.http.post(`${BASE_URL_API}/order/create`, orderData, {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    }).pipe(
      catchError(error => {
        console.error('Error from API:', error); // Debug lỗi nếu xảy ra
        return throwError(() => new Error(error.message || 'Server Error'));
      })
    );
  }

  // Cập nhật Order
  updateOrder(orderId: number, orderData: any): Observable<any> {
    return this.http.put(`${BASE_URL_API}/order/${orderId}`, orderData, {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    });
  }

  // Xóa Order
  deleteOrder(orderId: number): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/order/${orderId}/status`);
  }
}
