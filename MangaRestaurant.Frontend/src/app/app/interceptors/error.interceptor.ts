import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        const isLoginRequest = req.url.toLowerCase().includes('/accounts/login');

        // Only redirect to login on 401 if it's NOT the login request itself
        if (error.status === 401 && !isLoginRequest) {
          this.auth.logout();
          this.router.navigate(['/login']);
        }

        return throwError(() => error);
      })
    );
  }
}
