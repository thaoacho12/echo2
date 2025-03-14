import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { BASE_URL_API } from '../../../../app.config';
import { AuthService } from '../../../auth/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private token: string | null = null;

  constructor(private http: HttpClient, private authService: AuthService) { }

  // Phương thức lấy danh sách người dùng
  getUsers(currentPage: number, pageSize: number, searchKey?: string, lastActiveDays?: number): Observable<any> {
    const params: any = {
      page: currentPage,
      pageSize: pageSize,
    };
  
    // Thêm tham số tìm kiếm nếu có
    if (searchKey) {
      params.keySearch = searchKey;
    }
  
    // Thêm tham số ngày hoạt động nếu có
    if (lastActiveDays) {
      params.days = lastActiveDays;
    }
  
    // Gửi yêu cầu HTTP với các tham số
    return this.http.get(`${BASE_URL_API}/users`, { params });
  }

  // Phương thức lấy thông tin một người dùng theo ID
  getUserById(userId: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/users/${userId}`);
  }

  // Phương thức thêm mới người dùng
  addUser(user: any): Observable<any> {
    return this.http.post(`${BASE_URL_API}/users`, user);
  }

  // Phương thức cập nhật thông tin người dùng
  updateUser(id: number, user: any): Observable<any> {
    return this.http.put(`${BASE_URL_API}/users/${id}`, user).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Validation Errors:', error.error.errors);
        return throwError(() => error);
      })
    );
  }

  // Phương thức xóa người dùng
  deleteUserById(userId: number): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/users/${userId}`);
  }
  // Xóa người dùng theo danh sách ID
  deleteUsersByIdList(userIds: number[]): Observable<any> {
    return this.http.delete(`${BASE_URL_API}/users/delete-users-by-id-list`, {
      body: userIds,
    });
  }

  // Lọc người dùng theo số ngày hoạt động gần đây
  filterByLastActive(days: number): Observable<any> {
    return this.http.get(`${BASE_URL_API}/users/filter-by-last-active/${days}`);
  }

  // Lọc người dùng theo từ khóa tìm kiếm
  filterByKeySearch(query: string): Observable<any> {
    return this.http.get(`${BASE_URL_API}/users/filter-search/${query}`);
  }

  toggleBlockUser(userId: number): Observable<any> {
    return this.http.post<any>(`${BASE_URL_API}/users/toggle-block/${userId}`, null);
  }
  toggleBlockUsers(userIds: number[]): Observable<any> {
    return this.http.post(`${BASE_URL_API}/users/toggle-block-users`, userIds);
  }

  
}
