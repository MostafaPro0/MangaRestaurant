import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  baseUrl = environment.apiUrl;
  private wishlistCountSource = new BehaviorSubject<number>(0);
  wishlistCount$ = this.wishlistCountSource.asObservable();
  
  private wishlistedIdsSource = new BehaviorSubject<number[]>([]);
  wishlistedIds$ = this.wishlistedIdsSource.asObservable();

  constructor(private http: HttpClient) { }

  getWishlist(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl + '/wishlist').pipe(
        tap(items => {
            this.wishlistCountSource.next(items.length);
            this.wishlistedIdsSource.next(items.map(i => i.id));
        })
    );
  }

  toggleWishlist(productId: number): Observable<any> {
    return this.http.post(this.baseUrl + '/wishlist/' + productId, {}).pipe(
        tap(() => this.getWishlist().subscribe())
    );
  }

  isInWishlistLocal(productId: number): boolean {
    return this.wishlistedIdsSource.value.includes(productId);
  }

  isInWishlistServer(productId: number): Observable<boolean> {
    return this.http.get<boolean>(this.baseUrl + '/wishlist/check/' + productId);
  }

  removeFromWishlist(productId: number): Observable<any> {
    return this.http.delete(this.baseUrl + '/wishlist/' + productId).pipe(
        tap(() => this.getWishlist().subscribe())
    );
  }
}
