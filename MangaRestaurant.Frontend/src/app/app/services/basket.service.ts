import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { BasketItem, CustomerBasket } from '../models/basket.model';
import { MessageService } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private basket = new BehaviorSubject<CustomerBasket>({ id: '', items: [] });
  basket$ = this.basket.asObservable();

  constructor(private api: ApiService, private messageService: MessageService, private translate: TranslateService) {
    const saved = localStorage.getItem('customerBasket');
    if (saved) {
      const parsed: CustomerBasket = JSON.parse(saved);
      if (parsed && parsed.items && parsed.items.length > 0 && !parsed.id) {
        parsed.id = this.generateBasketId();
        this.storeBasket(parsed);
      } else {
        this.basket.next(parsed || { id: '', items: [] });
      }
    }
  }

  private storeBasket(basket: CustomerBasket): void {
    localStorage.setItem('customerBasket', JSON.stringify(basket));
    this.basket.next(basket);
  }

  private generateBasketId(): string {
    return crypto.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.floor(Math.random() * 1000000)}`;
  }

  private getOrCreateBasketId(): string {
    const current = this.basket.value;
    if (current.id) return current.id;

    const newId = this.generateBasketId();
    const updated = { ...current, id: newId };
    this.storeBasket(updated);
    return newId;
  }

  getBasket(id: string): Observable<CustomerBasket> {
    return this.api.get<any>(`Basket?id=${id}`).pipe(
      map((serverBasket: any) => {
        if (!serverBasket) {
          return { id, items: [] } as CustomerBasket;
        }

        const converted: CustomerBasket = {
          id: serverBasket.id,
          paymentIntentId: serverBasket.paymentIntentId,
          clientSecret: serverBasket.clientSecret,
          deliveryMethodId: serverBasket.deliveryMethodId,
          items: (serverBasket.items || []).map((item: any) => ({
            productId: item.id,
            productName: item.name,
            productNameAr: item.nameAr,
            price: item.price,
            quantity: item.quantity,
            pictureUrl: item.prictureUrl,
            brand: item.brand,
            category: item.category
          }))
        };

        this.storeBasket(converted);
        return converted;
      })
    );
  }

  updateBasket(basket: CustomerBasket): Observable<CustomerBasket> {
    const payload = {
      id: basket.id,
      paymentIntentId: basket.paymentIntentId,
      clientSecret: basket.clientSecret,
      deliveryMethodId: basket.deliveryMethodId,
      items: basket.items.map((item) => ({
        id: item.productId,
        name: item.productName,
        nameAr: item.productNameAr,
        prictureUrl: item.pictureUrl,
        price: item.price,
        quantity: item.quantity,
        brand: item.brand || '',
        category: item.category || ''
      }))
    };

    return this.api.post<any>('Basket', payload).pipe(
      map((updated: any) => {
        if (!updated) {
          throw new Error('Basket update failed');
        }
        const converted: CustomerBasket = {
          id: updated.id,
          paymentIntentId: updated.paymentIntentId,
          clientSecret: updated.clientSecret,
          deliveryMethodId: updated.deliveryMethodId,
          items: (updated.items || []).map((item: any) => ({
            productId: item.id,
            productName: item.name,
            productNameAr: item.nameAr,
            price: item.price,
            quantity: item.quantity,
            pictureUrl: item.prictureUrl,
            brand: item.brand,
            category: item.category
          }))
        };
        this.storeBasket(converted);
        return converted;
      })
    );
  }

  deleteBasket(id: string): Observable<boolean> {
    localStorage.removeItem('customerBasket');
    this.basket.next({ id: '', items: [] });
    return this.api.delete<boolean>(`Basket?id=${id}`);
  }

  addItem(product: any, quantity = 1): void {
    const current = this.basket.value;
    const basketId = current.id || this.getOrCreateBasketId();
    current.id = basketId;

    const index = current.items.findIndex((x) => x.productId === product.id);
    if (index >= 0) {
      current.items[index].quantity += quantity;
    } else {
      current.items.push({
        productId: product.id,
        productName: product.name,
        productNameAr: product.nameAr,
        price: product.price,
        quantity,
        pictureUrl: product.pictureUrl,
        brand: product.brand || '',
        category: product.category || ''
      });
    }

    const basket: CustomerBasket = { ...current, id: basketId, items: [...current.items] };
    this.storeBasket(basket);
    this.updateBasket(basket).subscribe({
      next: (updated) => {
        if (updated && updated.id) {
          this.storeBasket(updated);
        }
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS') || 'Success',
          detail: this.translate.instant('TOAST.ITEM_ADDED') || 'Product added to basket',
          life: 3000
        });
      },
      error: (err) => console.error('Basket update failed', err)
    });
  }

  getCurrentBasket(): CustomerBasket {
    const current = this.basket.value;
    if (!current.id) {
      const id = this.getOrCreateBasketId();
      const updated = { ...current, id };
      this.storeBasket(updated);
      return updated;
    }
    return current;
  }

  updateItemQuantity(productId: number, quantity: number): void {
    const current = this.basket.value;
    const index = current.items.findIndex((x) => x.productId === productId);
    if (index >= 0) {
      if (quantity <= 0) {
        this.removeItem(productId);
        return;
      }
      current.items[index].quantity = quantity;
      const basket: CustomerBasket = { ...current, items: [...current.items] };
      this.storeBasket(basket);
      this.updateBasket(basket).subscribe({
        next: (updated) => {
          if (updated && updated.id) {
            this.storeBasket(updated);
          }
        },
        error: (err) => console.error('Basket quantity update failed', err)
      });
    }
  }

  removeItem(productId: number): void {
    const current = this.basket.value;
    const items = current.items.filter((item) => item.productId !== productId);
    const basket = { ...current, items };
    this.storeBasket(basket);
    this.updateBasket(basket).subscribe({
      next: (updated) => {
        if (updated && updated.id) {
          this.storeBasket(updated);
        }
      },
      error: (err) => console.error('Basket update removed item failed', err)
    });
  }

  calculateTotals() {
    const current = this.basket.value;
    if (!current || !current.items) return { subtotal: 0, total: 0 };
    const subtotal = current.items.reduce((a, b) => (b.price * b.quantity) + a, 0);
    const total = subtotal; 
    return { subtotal, total };
  }
}
