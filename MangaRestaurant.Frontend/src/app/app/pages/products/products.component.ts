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

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, CardModule, ProgressSpinnerModule, InputTextModule, BadgeModule, TranslateModule, SkeletonModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  loading = false;
  search = '';
  selectedCategoryId = 0;

  constructor(
    private productsService: ProductsService, 
    private basketService: BasketService,
    public translateService: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
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
}
