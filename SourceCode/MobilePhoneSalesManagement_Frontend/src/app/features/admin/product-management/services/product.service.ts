import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  constructor(private http: HttpClient) { }

  getProducts(pageNumber: number, pageSize: number, sortField: string, orderBy: boolean): Observable<any> {
    console.log(sortField)
    console.log(orderBy)
    return this.http.get<any>
      (`${BASE_URL_API}/Products/get-all-products-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&sortField=${sortField}&orderBy=${orderBy}`);
  }

  getProductById(productId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/Products/get-product-by-id/${productId}`);
  }

  addProduct(product: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/Products/add-new-product`, product);
  }

  updateProduct(productId: string, product: any): Observable<any> {
    return this.http.put(`${BASE_URL_API}/Products/update-product/${productId}`, product);
  }

  deleteProduct(productId: string): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/Products/delete-product-by-id/${productId}`);
  }

  getSpecificationTypes(): Observable<import("../../specification-type-management/models/specificationType").specificationType[]> {
    return this.http.get<any>(`${BASE_URL_API}/SpecificationTypes/get-all-specificationTypes`);
  }

  searchProductsbyPage(pageNumber: number, pageSize: number, search: string, sortField: string, orderBy: boolean): Observable<any> {
    return this.http.get<any>
      (`${BASE_URL_API}/Products/search-products-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&search=${search}&sortField=${sortField}&orderBy=${orderBy}`);
  }
  filterProductsbyPage(pageNumber: number, pageSize: number, filter: boolean, sortField: string, orderBy: boolean): Observable<any> {
    return this.http.get<any>
      (`${BASE_URL_API}/Products/filter-products-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&filter=${filter}&sortField=${sortField}&orderBy=${orderBy}`);
  }
  deleteMultipleProducts(productIds: string[]): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/Products/delete-multiple-product`, {
      body: productIds  // Gửi danh sách productIds trong body của request
    });
  }

  restoreProducts(productIds: string[]): Observable<any> {
    return this.http.put(`${BASE_URL_API}/Products/restore-multiple-product`, productIds);
  }
}
