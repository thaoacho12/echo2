import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BASE_URL_API } from '../../../../app.config';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from '../../../auth/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartCount = new BehaviorSubject<number>(0); // Giá trị mặc định là 0

  constructor(private http: HttpClient, private authService: AuthService) { }

  cartCount$ = this.cartCount.asObservable(); // Biến Observable để lắng nghe

  updateCartCount(count: number): void {
    
    this.cartCount.next(count); // Cập nhật giá trị count
  }
  fetchCartCount(): void {
    this.getCartItems().subscribe(
      (res) => {
        const count = res.length;
        this.updateCartCount(count);
      },
      (err) => {
        this.updateCartCount(0);
      }
    );
  }

  getCartItems(): Observable<any>  {
    const token = this.authService.getCookie('Authentication');
    const headers = new HttpHeaders({
      Authorization: `${token}`,
    });
    return this.http.get(`${BASE_URL_API}/carts`, {
      headers,
      withCredentials: true,
    });
  }
  updateCart(productId: number, quantity: number ): Observable<any>  {
    return this.http.put(`${BASE_URL_API}/carts/update?productId=${productId}&quantity=${quantity}`, null);
  }
  deleteCartItem(productId: number): Observable<any>  {
    return this.http.delete(`${BASE_URL_API}/carts/remove/${productId}`);
  }
  deleteCartItems(selectedProductIds: number[]): Observable<any> {
    const url = `${BASE_URL_API}/carts/delete-multiple`;
    return this.http.post(url, { productIds: selectedProductIds });
  }
}
