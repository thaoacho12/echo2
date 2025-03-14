import { Injectable } from '@angular/core';
import { BASE_URL_API } from '../../../app.config';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ImageService {

  constructor(private http: HttpClient) { }


  getImagesbyPage(pageNumber: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${BASE_URL_API}/Images/get-all-images-by-page?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getImageById(imageId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/Images/get-image-by-id/${imageId}`);
  }

  addImage(image: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/Images/add-new-image`, image);
  }

}