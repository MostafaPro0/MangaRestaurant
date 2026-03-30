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

  register(displayName: string, email: string, password: string, phoneNumber?: string): Observable<User> {
    return this.api.post<User>('Accounts/Register', { displayName, email, password, phoneNumber }).pipe(
      map((user) => {
        this.setUser(user);
        return user;
      })
    );
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

    const role = payload?.role ?? payload?.Role;
    const roles = payload?.roles ?? payload?.Roles;
    const isAdmin = payload?.isAdmin ?? payload?.IsAdmin;

    const candidates = [
      typeof role === 'string' ? role : undefined,
      Array.isArray(roles) ? roles : undefined,
      typeof isAdmin === 'boolean' ? isAdmin : undefined,
    ].filter(Boolean) as any[];

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

