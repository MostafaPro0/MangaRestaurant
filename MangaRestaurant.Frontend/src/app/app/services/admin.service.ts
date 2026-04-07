import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private api: ApiService) {}

  getAdminReport(): Observable<any> {
    return this.api.get<any>('Orders/Admin/Report');
  }

  getEmployees(): Observable<any> {
    return this.api.get<any>('Employee');
  }

  getEmployee(id: number): Observable<any> {
    return this.api.get<any>(`Employee/${id}`);
  }

  uploadProductImage(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.post<string>('Upload/image', formData);
  }
}
