import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  constructor() {}

  /**
   * Identifies the current tenant based on the URL.
   * Returns:
   * - 'slug' (string) if a valid restaurant subdomain is found.
   * - null if we are on the main domain (Landing Page / Platform).
   */
  getTenantSlug(): string | null {
    const host = window.location.hostname;
    const parts = host.split('.');

    // Handle Localhost Development
    // kfc.localhost -> parts = ['kfc', 'localhost']
    if (parts.length === 2 && parts[1] === 'localhost') {
        return parts[0];
    }
    if (host === 'localhost' || host === '127.0.0.1') {
        return null; // Localhost without subdomain is the PLATFORM
    }

    // Handle Production Domains (e.g., manga.com, kfc.manga.com)
    if (parts.length >= 3) {
      const subdomain = parts[0];
      // Skip common technical subdomains
      if (subdomain !== 'www' && subdomain !== 'api' && subdomain !== 'admin-control') {
        return subdomain;
      }
    }

    // Default: Main Domain (Platform)
    return null;
  }

  /**
   * Returns true if the current visitor is on the main platform domain.
   */
  isPlatform(): boolean {
    return this.getTenantSlug() === null;
  }
}
