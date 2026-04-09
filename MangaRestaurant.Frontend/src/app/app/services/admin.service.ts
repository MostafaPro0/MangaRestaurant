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

  getUsers(): Observable<any[]> {
    return this.api.get<any[]>('Accounts/All');
  }

  getUsersByRole(role: string): Observable<any[]> {
    return this.api.get<any[]>(`Accounts/ListByRole?role=${role}`);
  }

  createUser(user: any, role: string): Observable<any> {
    return this.api.post<any>(`Accounts/Admin/Create?role=${role}`, user);
  }

  updateUserRole(userId: string, role: string): Observable<any> {
    return this.api.put<any>(`Accounts/Admin/UpdateRole?userId=${userId}&role=${role}`, {});
  }

  deleteUser(userId: string): Observable<any> {
    return this.api.delete<any>(`Accounts/Admin/Delete/${userId}`);
  }

  toggleUserBan(userId: string): Observable<any> {
    return this.api.put<any>(`Accounts/Admin/ToggleBan/${userId}`, {});
  }

  uploadProductImage(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.post<string>('Upload/image', formData);
  }
}
