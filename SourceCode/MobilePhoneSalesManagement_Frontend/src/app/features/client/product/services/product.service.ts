import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';
import { Product } from '../../../admin/product-management/models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  constructor(private http: HttpClient) { }
  getSubjects(): Observable<Product[]> {
    return this.http.get<Product[]>(`${BASE_URL_API}/Products/get-all-products`);
  }
  // Lọc sản phẩm
  filterProducts(filterRequest: any): Observable<{ products: Product[], totalPages: number }> {
    return this.http.post<{ products: Product[], totalPages: number }>(`${BASE_URL_API}/Products/filter`, filterRequest);
  }
  newestProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${BASE_URL_API}/Products/get-newest-product`);
  }
  discountedProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${BASE_URL_API}/Products/get-discounted-product`);
  }
}
