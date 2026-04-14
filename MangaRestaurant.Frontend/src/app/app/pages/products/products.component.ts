import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { SkeletonModule } from 'primeng/skeleton';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { SettingsService } from '../../services/settings.service';
import { WishlistService } from '../../services/wishlist.service';
import { AuthService } from '../../services/auth.service';

import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, CardModule, ProgressSpinnerModule, InputTextModule, BadgeModule, TranslateModule, SkeletonModule, TooltipModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  loading = false;
  search = '';
  selectedCategoryId = 0;
  settings$! : any;

  constructor(
    private productsService: ProductsService, 
    private basketService: BasketService,
    public translateService: TranslateService,
    private settingsService: SettingsService,
    public wishlistService: WishlistService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.loadProducts();
    this.loadCategories();
    
    // Only load wishlist if user is authenticated
    if (this.authService.getCurrentUser()) {
        this.wishlistService.getWishlist().subscribe();
    }
  }

  loadProducts(): void {
    this.loading = true;
    const catId = this.selectedCategoryId === 0 ? null : this.selectedCategoryId;
    this.productsService.getProducts(1, 40, this.search, catId).subscribe({
      next: (result) => {
        this.products = result?.data ?? [];
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  loadCategories(): void {
    this.productsService.getCategories().subscribe({
      next: (categories) => (this.categories = categories),
      error: () => {}
    });
  }

  filterByCategory(id: number): void {
    this.selectedCategoryId = id;
    this.loadProducts();
  }

  clearSearch(): void {
    this.search = '';
    this.loadProducts();
  }

  addToBasket(product: any): void {
    this.basketService.addItem(product, 1);
  }

  toggleWishlist(product: any): void {
    if (!this.authService.getCurrentUser()) {
        // You might want to redirect to login or show a message
        return;
    }
    this.wishlistService.toggleWishlist(product.id).subscribe();
  }

  isInWishlist(productId: number): boolean {
    return this.wishlistService.isInWishlistLocal(productId);
  }
}
