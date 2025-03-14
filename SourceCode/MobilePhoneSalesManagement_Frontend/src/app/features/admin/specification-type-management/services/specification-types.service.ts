import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BASE_URL_API } from '../../../../app.config';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SpecificationTypesService {

  constructor(private http: HttpClient) { }

  getSpecificationTypess(): Observable<any> {
    return this.http.get<any>
      (`${BASE_URL_API}/SpecificationTypes/get-all-specificationTypes`);
  }

  getSpecificationTypesById(specificationTypesId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/SpecificationTypess/get-specificationTypes-by-id/${specificationTypesId}`);
  }

  addSpecificationTypes(specificationTypes: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/SpecificationTypes/add-new-specificationType`, specificationTypes);
  }

  updateSpecificationTypes(specificationTypesId: number, specificationTypes: any): Observable<any> {
    console.log(specificationTypesId, specificationTypes);
    return this.http.put(`${BASE_URL_API}/SpecificationTypes/update-specificationType/${specificationTypesId}`, specificationTypes);
  }

  deleteSpecificationTypes(specificationTypesId: string): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/SpecificationTypes/delete-specificationType-by-id/${specificationTypesId}`);
  }
}
