import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: Record<string, string | number>): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${path}`, { params: params as any });
  }

  post<T>(path: string, body: any, headers?: HttpHeaders): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${path}`, body, { headers });
  }

  put<T>(path: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${path}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${path}`);
  }
}
