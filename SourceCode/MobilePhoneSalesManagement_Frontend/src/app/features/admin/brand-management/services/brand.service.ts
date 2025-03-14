import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';
import { RequestBrand } from '../models/add-brand-request.model';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  constructor(private http: HttpClient) { }

  getBrands(): Observable<any> {
    return this.http.get<any>(`${BASE_URL_API}/Brands/get-all-brands`);
  }
  getBrandsbyPage(pageNumber: number, pageSize: number, sortField: string, orderBy: boolean): Observable<any> {
    return this.http.get<any>(`${BASE_URL_API}/Brands/get-all-brands-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&sortField=${sortField}&orderBy=${orderBy}`);
  }
  searchBrandsbyPage(pageNumber: number, pageSize: number, search: string, sortField: string, orderBy: boolean): Observable<any> {
    return this.http.get<any>(`${BASE_URL_API}/Brands/search-brands-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&search=${search}&sortField=${sortField}&orderBy=${orderBy}`);
  }
  filterBrandsbyPage(pageNumber: number, pageSize: number, filter: boolean, sortField: string, orderBy: boolean): Observable<any> {
    return this.http.get<any>(`${BASE_URL_API}/Brands/filter-brands-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}&filter=${filter}&sortField=${sortField}&orderBy=${orderBy}`);
  }
  getBrandById(brandId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/Brands/get-brand-by-id/${brandId}`);
  }
  getBrandByIdtoSearch(brandId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/Brands/get-brand-by-id/${brandId}`);
  }
  addBrand(brand: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/Brands/add-new-brand`, brand);
  }

  updateBrand(brandId: string, brand: any): Observable<any> {
    return this.http.put(`${BASE_URL_API}/Brands/update-brand/${brandId}`, brand);
  }

  deleteBrand(brandId: string): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/Brands/delete-brand-by-id/${brandId}`);
  }

  deleteMultipleBrands(brandIds: string[]): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/Brands/delete-multiple-brand`, {
      body: brandIds  // Gửi danh sách brandIds trong body của request
    });
  }

  restoreBrands(brandIds: string[]): Observable<any> {
    return this.http.put(`${BASE_URL_API}/Brands/restore-multiple-brand`, brandIds);
  }
}
