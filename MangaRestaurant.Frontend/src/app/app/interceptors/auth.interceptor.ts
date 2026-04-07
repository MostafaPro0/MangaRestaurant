import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.token;
    const lang = localStorage.getItem('lang') || 'ar'; // Default to 'ar' to match backend

    let headers: any = {
      'Accept-Language': lang
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const authReq = req.clone({
      setHeaders: headers,
    });

    return next.handle(authReq);
  }
}

