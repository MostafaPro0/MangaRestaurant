import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  constructor() {}

  /**
   * Retrieves the current tenant's slug.
   * Logic:
   * 1. Check Subdomain (e.g. kfc.yoursaas.com -> kfc)
   * 2. Fallback to a hardcoded local development tenant ('demo' or 'kfc')
   *    In production, this fallback can be removed or used for the landing page.
   */
  getTenantSlug(): string {
    const host = window.location.hostname;
    
    // For localhost development (e.g. localhost:4200),
    // you can configure your etc/hosts to map to kfc.localhost or just fallback here.
    if (host === 'localhost' || host === '127.0.0.1') {
      // You can change 'demo' to the slug of a DB you created during testing.
      // E.g., 'manga', 'demo', etc.
      return 'manga'; 
    }

    const parts = host.split('.');
    
    // Assuming structure: tenant.domain.com (3 parts minimum for valid subdomain extraction)
    if (parts.length >= 3) {
      if (parts[0] !== 'www' && parts[0] !== 'api') {
        return parts[0];
      }
    }

    // Default fallback if no subdomain is present (you might want to return null and handle redirect later)
    return 'manga';
  }
}
