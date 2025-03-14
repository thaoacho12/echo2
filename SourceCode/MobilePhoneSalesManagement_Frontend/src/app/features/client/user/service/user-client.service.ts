import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../auth/services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class UserClientService {
  constructor(private http: HttpClient, private authService: AuthService) {}
  // client
  getCurrentUser(): Observable<any> {
    const token = this.authService.getCookie('Authentication');
    const headers = new HttpHeaders({
      Authorization: `${token}`,
    });
    return this.http.get(`${BASE_URL_API}/users/me`, {
      headers,
      withCredentials: true,
    });
  }
  updateCurrentUser(user: any): Observable<any> {
    const token = this.authService.getCookie('Authentication');
    const headers = new HttpHeaders({
      Authorization: `${token}`,
    });

    return this.http.put(`${BASE_URL_API}/users/update-me`, user, {
      headers,
      withCredentials: true,
    });
  }
  changePassword(model: any): Observable<any> {
    const token = this.authService.getCookie('Authentication');
    const headers = new HttpHeaders({
      Authorization: `${token}`,
    });

    return this.http.post(`${BASE_URL_API}/users/change-password`, model, {
      headers,
      withCredentials: true,
    });
  }

  getWishList(): Observable<any> {
    return this.http.get(`${BASE_URL_API}/users/get-wish-list`);
  }
  toggleWishList(productId: number): Observable<any> {
    const token = this.authService.getCookie('Authentication');
    const headers = new HttpHeaders({
      Authorization: `${token}`,
    });
    // Gọi API và xử lý
    return this.http.put(`${BASE_URL_API}/users/toggle-wish-list`, productId, {
      headers,
      withCredentials: true,
    });
  }

  getOrders(): Observable<any> {
    return this.http.get(`${BASE_URL_API}/order/get-all-order-by-user-id`)
  }
}
