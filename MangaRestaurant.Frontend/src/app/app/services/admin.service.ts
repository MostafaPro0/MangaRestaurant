import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private api: ApiService) {}

  getStats(): Observable<any> {
    // custom endpoint may be needed in backend, this is placeholder.
    return this.api.get<any>('Employee');
  }

  getEmployees(): Observable<any> {
    return this.api.get<any>('Employee');
  }

  getEmployee(id: number): Observable<any> {
    return this.api.get<any>(`Employee/${id}`);
  }
}
