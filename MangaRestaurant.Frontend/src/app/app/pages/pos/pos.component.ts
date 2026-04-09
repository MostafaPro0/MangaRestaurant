import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { SkeletonModule } from 'primeng/skeleton';
import { TranslateModule } from '@ngx-translate/core';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { SettingsService } from '../../services/settings.service';
import { environment } from '../../../../environments/environment';
import { Product } from '../../models/product.model';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Router } from '@angular/router';
import { OrdersService } from '../../services/orders.service';
import { OrderCreateRequest } from '../../models/order.model';

import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    ButtonModule, 
    InputTextModule, 
    CardModule, 
    BadgeModule, 
    SkeletonModule, 
    TranslateModule,
    ToastModule,
    TooltipModule
  ],
  providers: [MessageService],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.css'
})
export class PosComponent implements OnInit {
  products: Product[] = [];
  categories: any[] = [];
  loading = false;
  search = '';
  selectedCategoryId = 0;
  settings$: any;
  basket$: any;

  constructor(
    public productsService: ProductsService,
    public basketService: BasketService,
    public translate: TranslateService,
    private ordersService: OrdersService,
    private settingsService: SettingsService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.basket$ = this.basketService.basket$;
    this.loadCategories();
    this.loadProducts();
  }

  get totals() {
    return this.basketService.calculateTotals();
  }

  loadCategories() {
    this.productsService.getCategories().subscribe({
      next: (data) => this.categories = data,
      error: (err) => console.error('Error loading categories', err)
    });
  }

  loadProducts() {
    this.loading = true;
    this.productsService.getProducts(1, 50, this.search, this.selectedCategoryId || null).subscribe({
      next: (data) => {
        this.products = data.data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading products', err);
        this.loading = false;
      }
    });
  }

  filterByCategory(id: number) {
    this.selectedCategoryId = id;
    this.loadProducts();
  }

  onSearch() {
    this.loadProducts();
  }

  addToBasket(product: Product) {
    this.basketService.addItem(product);
  }

  removeFromBasket(item: any) {
    this.basketService.removeItem(item.productId);
  }

  incrementQuantity(item: any) {
    this.basketService.updateItemQuantity(item.productId, item.quantity + 1);
  }

  decrementQuantity(item: any) {
    this.basketService.updateItemQuantity(item.productId, item.quantity - 1);
  }

  clearBasket() {
    const currentBasket = this.basketService.getCurrentBasket();
    if (currentBasket.id) {
      this.basketService.deleteBasket(currentBasket.id).subscribe();
    }
  }

  submitOrder() {
    const basket = this.basketService.getCurrentBasket();
    if (!basket || basket.items.length === 0) {
      this.messageService.add({ 
        severity: 'warn', 
        summary: 'Warning', 
        detail: (this.translate.currentLanguage === 'ar' ? 'يرجى إضافة أصناف أولاً' : 'Please add items first')
      });
      return;
    }

    this.loading = true;
    
    // 2. Submit Order
    const orderPayload: OrderCreateRequest = {
      basketId: basket.id,
      deliveryMethodId: 0, // No delivery method needed for Dine-In
      orderType: 1, // DineIn
      shippingAddress: {
        firstName: 'Dine-In',
        lastName: 'Customer',
        street: 'Restaurant Table',
        city: 'Inside',
        state: 'POS',
        zipCode: '0000',
        country: 'Inside'
      }
    };

        this.ordersService.createOrder(orderPayload).subscribe({
          next: () => {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: (this.translate.currentLanguage === 'ar' ? 'تم إرسال طلبك بنجاح!' : 'Order submitted successfully!') 
            });
            this.basketService.deleteBasket(basket.id).subscribe();
            this.loading = false;
          },
          error: () => {
            this.messageService.add({ 
              severity: 'error', 
              summary: 'Error', 
              detail: (this.translate.currentLanguage === 'ar' ? 'فشل إرسال الطلب' : 'Failed to submit order') 
            });
            this.loading = false;
          }
        });
  }

  onCheckout() {
    this.router.navigateByUrl('/checkout');
  }

  getImageUrl(url: string | null | undefined): string {
    if (!url) return 'assets/images/placeholder.png';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl}${url}`;
  }

  get totalItemsCount(): number {
    let count = 0;
    const basket = this.basketService.getCurrentBasket();
    if (basket && basket.items) {
      basket.items.forEach(i => count += i.quantity);
    }
    return count;
  }
}
