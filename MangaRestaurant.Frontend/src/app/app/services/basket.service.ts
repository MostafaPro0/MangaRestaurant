import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BasketItem, CustomerBasket } from '../models/basket.model';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private basket = new BehaviorSubject<CustomerBasket>({ id: '', items: [] });
  basket$ = this.basket.asObservable();

  constructor(private api: ApiService) {
    const saved = localStorage.getItem('customerBasket');
    if (saved) this.basket.next(JSON.parse(saved));
  }

  private storeBasket(basket: CustomerBasket): void {
    localStorage.setItem('customerBasket', JSON.stringify(basket));
    this.basket.next(basket);
  }

  getBasket(id: string): Observable<CustomerBasket> {
    return this.api.get<CustomerBasket>(`Basket?id=${id}`);
  }

  updateBasket(basket: CustomerBasket): Observable<CustomerBasket> {
    this.storeBasket(basket);
    return this.api.post<CustomerBasket>('Basket', basket);
  }

  deleteBasket(id: string): Observable<boolean> {
    localStorage.removeItem('customerBasket');
    return this.api.delete<boolean>(`Basket?id=${id}`);
  }

  addItem(product: any, quantity = 1): void {
    const current = this.basket.value;
    const index = current.items.findIndex((x) => x.productId === product.id);
    if (index >= 0) {
      current.items[index].quantity += quantity;
    } else {
      current.items.push({ productId: product.id, productName: product.name, price: product.price, quantity, pictureUrl: product.pictureUrl });
    }
    const basket = { ...current, items: [...current.items] };
    this.storeBasket(basket);
    this.updateBasket(basket).subscribe();
  }

  removeItem(productId: number): void {
    const current = this.basket.value;
    const items = current.items.filter((item) => item.productId !== productId);
    const basket = { ...current, items };
    this.storeBasket(basket);
    this.updateBasket(basket).subscribe();
  }
}
