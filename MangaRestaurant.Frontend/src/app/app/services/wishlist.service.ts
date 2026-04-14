import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap, of, forkJoin } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  baseUrl = environment.apiUrl;
  private wishlistCountSource = new BehaviorSubject<number>(0);
  wishlistCount$ = this.wishlistCountSource.asObservable();
  
  private wishlistedIdsSource = new BehaviorSubject<number[]>([]);
  wishlistedIds$ = this.wishlistedIdsSource.asObservable();

  constructor(private http: HttpClient) {
    this.loadInitialWishlist();
  }

  private loadInitialWishlist() {
    const isAuth = !!localStorage.getItem('currentUser');
    if (isAuth) {
      this.getWishlist().subscribe();
    } else {
      const guestWishlist = localStorage.getItem('guestWishlist');
      if (guestWishlist) {
        const ids = JSON.parse(guestWishlist);
        this.wishlistedIdsSource.next(ids);
        this.wishlistCountSource.next(ids.length);
      }
    }
  }

  getWishlist(): Observable<any[]> {
    const isAuth = !!localStorage.getItem('currentUser');
    if (!isAuth) {
      return of([]); // For guests, getWishlist returns empty as we only track IDs locally
    }
    return this.http.get<any[]>(this.baseUrl + '/wishlist').pipe(
        tap(items => {
            this.wishlistCountSource.next(items.length);
            this.wishlistedIdsSource.next(items.map(i => i.id));
        })
    );
  }

  toggleWishlist(productId: number): Observable<any> {
    const isAuth = !!localStorage.getItem('currentUser');
    if (!isAuth) {
       let ids = [...this.wishlistedIdsSource.value];
       if (ids.includes(productId)) {
         ids = ids.filter(id => id !== productId);
       } else {
         ids.push(productId);
       }
       this.wishlistedIdsSource.next(ids);
       this.wishlistCountSource.next(ids.length);
       localStorage.setItem('guestWishlist', JSON.stringify(ids));
       return of({ success: true });
    }

    return this.http.post(this.baseUrl + '/wishlist/' + productId, {}).pipe(
        tap(() => this.getWishlist().subscribe())
    );
  }

  isInWishlistLocal(productId: number): boolean {
    return this.wishlistedIdsSource.value.includes(productId);
  }

  isInWishlistServer(productId: number): Observable<boolean> {
    const isAuth = !!localStorage.getItem('currentUser');
    if (!isAuth) return of(this.isInWishlistLocal(productId));
    return this.http.get<boolean>(this.baseUrl + '/wishlist/check/' + productId);
  }

  removeFromWishlist(productId: number): Observable<any> {
    const isAuth = !!localStorage.getItem('currentUser');
    if (!isAuth) {
      const ids = this.wishlistedIdsSource.value.filter(id => id !== productId);
      this.wishlistedIdsSource.next(ids);
      this.wishlistCountSource.next(ids.length);
      localStorage.setItem('guestWishlist', JSON.stringify(ids));
      return of({ success: true });
    }

    return this.http.delete(this.baseUrl + '/wishlist/' + productId).pipe(
        tap(() => this.getWishlist().subscribe())
    );
  }

  mergeGuestWishlist(): Observable<any> {
    const guestWishlist = localStorage.getItem('guestWishlist');
    if (!guestWishlist) return of(null);

    const ids: number[] = JSON.parse(guestWishlist);
    if (ids.length === 0) return of(null);

    // Merge by toggling items onto the server
    // Note: This is an optimistic merge. For a better one, we should have a batch endpoint.
    const requests = ids.map(id => this.http.post(this.baseUrl + '/wishlist/' + id, {}));
    
    return forkJoin(requests).pipe(
      tap(() => {
        localStorage.removeItem('guestWishlist');
        this.getWishlist().subscribe();
      })
    );
  }
}
