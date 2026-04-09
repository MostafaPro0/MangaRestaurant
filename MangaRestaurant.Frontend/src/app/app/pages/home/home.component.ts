import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { CommonModule } from '@angular/common';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, CardModule, SkeletonModule, TranslateModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  deals: any[] = [];
  latestProducts: any[] = [];
  loadingDeals = true;
  loadingLatest = true;

  constructor(
    private productsService: ProductsService, 
    private basketService: BasketService,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadDeals();
    this.loadLatest();
  }

  loadDeals() {
    this.productsService.getDeals().subscribe({
      next: (data) => {
        this.deals = data;
        this.loadingDeals = false;
      },
      error: () => this.loadingDeals = false
    });
  }

  loadLatest() {
    this.productsService.getLatestProducts().subscribe({
      next: (data) => {
        this.latestProducts = data;
        this.loadingLatest = false;
      },
      error: () => this.loadingLatest = false
    });
  }

  addToBasket(product: any) {
    this.basketService.addItem(product);
  }

  getImageUrl(path: string): string {
    if (!path) return 'assets/images/products/placeholder.jpg';
    if (path.startsWith('http')) return path;
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}/${path}`;
  }
}
