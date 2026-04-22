import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Tenant {
  id: number;
  name: string;
  nameAr: string;
  slug: string;
  planId: number;
  isActive: boolean;
  createdAt: Date;
  subscriptionEndDate: Date;
  plan: any; // Add exact type if needed
}

export interface CreateTenantDto {
  name: string;
  nameAr: string;
  slug: string;
  adminEmail: string;
  adminName: string;
  adminPassword: string;
  planId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class SuperAdminService {
  private baseUrl = environment.apiUrl + '/super-admin/tenants';

  constructor(private http: HttpClient) {}

  getAllTenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.baseUrl);
  }

  createTenant(dto: CreateTenantDto): Observable<Tenant> {
    return this.http.post<Tenant>(this.baseUrl, dto);
  }

  deleteTenant(slug: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${slug}`);
  }

  updateTenant(slug: string, dto: any): Observable<Tenant> {
    return this.http.put<Tenant>(`${this.baseUrl}/${slug}`, dto);
  }
}
