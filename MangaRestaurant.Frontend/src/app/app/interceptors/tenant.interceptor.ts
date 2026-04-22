import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { TenantService } from '../services/tenant.service';

@Injectable()
export class TenantInterceptor implements HttpInterceptor {

  constructor(private tenantService: TenantService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Determine the tenant slug
    const tenantSlug = this.tenantService.getTenantSlug();
    
    // Only intercept requests going to our API backend.
    // Skip requests to local assets (like translation json files)
    // AND skip platform-wide SaaS info requests
    if (request.url.includes('/api/') && tenantSlug && !request.url.includes('/saas-info/')) {
      const clonedRequest = request.clone({
        headers: request.headers.set('X-Tenant-Slug', tenantSlug)
      });
      return next.handle(clonedRequest);
    }
    
    return next.handle(request);
  }
}
