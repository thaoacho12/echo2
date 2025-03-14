import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private apiUrl = 'https://localhost:7001/api/Products/get-all-products-by-page?pageNumber=1&sortField=updatedDate&pageSize=10&orderBy=false';  // Replace with your API URL

    constructor(private http: HttpClient) { }

    getProductById(id: string): Observable<any> {
        // return this.http.get(`${this.apiUrl}/${id}`);  // Fetch product details by ID
        return of([
            {
                productId: 1,
                name: 'Sản phẩm 1',
                price: 1500000,
                discount: 10,
                image: { imageBase64: '...' } // Thay bằng base64 thật
            },
            {
                productId: 2,
                name: 'Sản phẩm 2',
                price: 2000000,
                discount: 15,
                image: { imageBase64: '...' }
            }
        ]);
    }
}


