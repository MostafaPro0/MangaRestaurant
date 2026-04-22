import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SaasPlan, SaasTenant } from '../models/saas.models';

/**
 * Service responsible for fetching public SaaS platform information.
 * Follows Clean Architecture by decoupling API calls from components.
 */
@Injectable({
  providedIn: 'root'
})
export class SaasInfoService {
  private readonly baseUrl = `${environment.apiUrl}/saas-info`;

  constructor(private http: HttpClient) {}

  /**
   * Fetches the list of active public tenants (partners).
   * @returns Observable of SaasTenant array.
   */
  getActiveTenants(): Observable<SaasTenant[]> {
    return this.http.get<SaasTenant[]>(`${this.baseUrl}/active-tenants`).pipe(
      map(data => data || []),
      catchError(error => {
        console.error('Error fetching active tenants:', error);
        return of([]);
      })
    );
  }

  /**
   * Fetches the available subscription plans.
   * @returns Observable of SaasPlan array.
   */
  getAvailablePlans(): Observable<SaasPlan[]> {
    return this.http.get<SaasPlan[]>(`${this.baseUrl}/plans`).pipe(
      map(data => data || []),
      catchError(error => {
        console.error('Error fetching saas plans:', error);
        return of([]);
      })
    );
  }

  /**
   * Submits a new restaurant registration (onboarding).
   * @param registrationData The DTO for creating a new tenant.
   */
  registerRestaurant(registrationData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register-restaurant`, registrationData);
  }
}
