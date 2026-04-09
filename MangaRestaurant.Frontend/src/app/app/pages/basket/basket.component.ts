import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CarouselModule } from 'primeng/carousel';
import { SkeletonModule } from 'primeng/skeleton';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { SettingsService } from '../../services/settings.service';
import { ProductsService } from '../../services/products.service';
import { CustomerBasket } from '../../models/basket.model';
import { environment } from '../../../../environments/environment';

import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-basket',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, TableModule, RouterLink, TranslateModule, CarouselModule, SkeletonModule, TooltipModule],
  templateUrl: './basket.component.html',
  styleUrl: './basket.component.css'
})
export class BasketComponent implements OnInit {
  basket: CustomerBasket = { id: '', items: [] };
  deals: any[] = [];
  loadingDeals = true;
  settings$! : any;
  
  responsiveOptions = [
    { breakpoint: '1024px', numVisible: 3, numScroll: 3 },
    { breakpoint: '768px', numVisible: 2, numScroll: 2 },
    { breakpoint: '560px', numVisible: 1, numScroll: 1 }
  ];

  constructor(
    private basketService: BasketService, 
    public translateService: TranslateService,
    private productsService: ProductsService,
    private settingsService: SettingsService
  ) {}

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.basketService.basket$.subscribe((basket) => (this.basket = basket));
    this.loadDeals();
  }

  loadDeals() {
    this.productsService.getDeals().subscribe({
      next: (data) => {
        this.deals = data;
        this.loadingDeals = false;
      },
      error: () => (this.loadingDeals = false)
    });
  }

  getImageUrl(path: string): string {
    if (!path) return 'assets/images/products/placeholder.jpg';
    if (path.startsWith('http')) return path;
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}/${path}`;
  }

  addToBasket(product: any) {
    this.basketService.addItem(product);
  }

  removeItem(productId: number) {
    this.basketService.removeItem(productId);
  }

  updateQuantity(productId: number, currentQuantity: number, change: number) {
    const newQuantity = currentQuantity + change;
    if (newQuantity > 0) {
      this.basketService.updateItemQuantity(productId, newQuantity);
    } else {
      this.removeItem(productId);
    }
  }

  get total(): number {
    return this.basket.items.reduce((sum: number, item: any) => sum + item.price * item.quantity, 0);
  }
}
