import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, map, Observable, of } from 'rxjs';
import { ApiService } from './api.service';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUser$ = new BehaviorSubject<User | null>(null);

  constructor(private api: ApiService, private router: Router) {
    const stored = localStorage.getItem('currentUser');
    if (stored) this.currentUser$.next(JSON.parse(stored));
  }

  get user(): Observable<User | null> {
    return this.currentUser$.asObservable();
  }

  private setUser(user: User): void {
    this.currentUser$.next(user);
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  login(email: string, password: string): Observable<User> {
    return this.api.post<User>('Accounts/Login', { email, password }).pipe(
      map((user) => {
        this.setUser(user);
        return user;
      })
    );
  }

  googleLogin(idToken: string): Observable<User> {
    return this.api.post<User>('Accounts/GoogleLogin', { idToken }).pipe(
      map((user) => {
        this.setUser(user);
        return user;
      })
    );
  }

  register(displayName: string, email: string, password: string, phoneNumber?: string): Observable<User> {
    return this.api.post<User>('Accounts/Register', { displayName, email, password, phoneNumber }).pipe(
      map((user) => {
        this.setUser(user);
        return user;
      })
    );
  }

  updateProfile(updateData: { displayName: string, phoneNumber?: string, phoneNumber2?: string, profilePictureUrl?: string }): Observable<User> {
    return this.api.put<User>('Accounts/UpdateProfile', updateData).pipe(
      map((user) => {
        this.setUser(user);
        return user;
      })
    );
  }

  uploadProfileImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    // Directly use HttpClient or adapt ApiService if it doesn't support FormData.
    // Assuming api.post supports passing FormData directly:
    return this.api.post<{ url: string }>('Accounts/UploadImage', formData);
  }

  getUserAddresses(): Observable<any[]> {
    return this.api.get<any[]>('Accounts/UserAddresses');
  }

  addAddress(address: any): Observable<any> {
    return this.api.post<any>('Accounts/Address', address);
  }

  updateAddress(address: any): Observable<any> {
    return this.api.put<any>('Accounts/Address', address);
  }

  deleteAddress(id: number): Observable<any> {
    return this.api.delete<any>(`Accounts/Address/${id}`);
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUser$.next(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!this.currentUser$.value;
  }

  get currentUser(): User | null {
    return this.currentUser$.value;
  }

  get token(): string | null {
    return this.currentUser?.token ?? null;
  }

  private decodeJwtPayload(token: string): any | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const padLen = (4 - (base64.length % 4)) % 4;
      const padded = base64 + '='.repeat(padLen);
      const json = atob(padded);
      return JSON.parse(json);
    } catch {
      return null;
    }
  }

  getAdminDecision(token: string | null = this.token): 'admin' | 'not-admin' | 'unknown' {
    if (!token) return 'unknown';
    const payload = this.decodeJwtPayload(token);
    if (!payload) return 'unknown';

    // ASP.NET Identity uses the full URI for ClaimTypes.Role
    const msRoleUri = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const msRoleClaims = payload[msRoleUri];

    const role = payload?.role ?? payload?.Role ?? msRoleClaims;
    const roles = payload?.roles ?? payload?.Roles;
    const isAdmin = payload?.isAdmin ?? payload?.IsAdmin;

    // Normalize roles array from MS claim URI
    const msRoles: string[] = Array.isArray(msRoleClaims)
      ? msRoleClaims
      : typeof msRoleClaims === 'string'
      ? [msRoleClaims]
      : [];

    const candidates = [
      typeof role === 'string' ? role : undefined,
      Array.isArray(roles) ? roles : undefined,
      typeof isAdmin === 'boolean' ? isAdmin : undefined,
    ].filter(Boolean) as any[];

    // Check MS claim roles directly
    if (msRoles.length > 0) {
      const hasAdmin = msRoles.some((r) => r.toLowerCase() === 'admin' || r.toLowerCase() === 'administrator');
      return hasAdmin ? 'admin' : 'not-admin';
    }

    if (candidates.length === 0) {
      // Try to find any "role"/"admin" hints in arbitrary claim shapes.
      const values = Object.values(payload ?? {});
      const hasAdminHint = values.some((v) => {
        if (typeof v === 'string') {
          const s = v.toLowerCase();
          return s.includes('admin') || s.includes('administrator');
        }
        if (Array.isArray(v)) {
          return v.some((x) => typeof x === 'string' && (x.toLowerCase().includes('admin') || x.toLowerCase().includes('administrator')));
        }
        return false;
      });

      return hasAdminHint ? 'admin' : 'unknown';
    }

    // If we have boolean isAdmin claim.
    if (typeof isAdmin === 'boolean') return isAdmin ? 'admin' : 'not-admin';

    const normalizedRoles: string[] = [];
    if (typeof role === 'string') normalizedRoles.push(role);
    if (Array.isArray(roles)) normalizedRoles.push(...roles.map(String));

    if (normalizedRoles.length === 0) return 'unknown';
    const hasAdmin = normalizedRoles.some((r) => String(r).toLowerCase() === 'admin' || String(r).toLowerCase() === 'administrator');
    return hasAdmin ? 'admin' : 'not-admin';
  }

  isAdmin(token: string | null = this.token): boolean {
    return this.getAdminDecision(token) === 'admin';
  }
}

