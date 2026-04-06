import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { BadgeModule } from 'primeng/badge';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, ProgressSpinnerModule, TranslateModule, SkeletonModule, BadgeModule],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css'
})
export class ProductDetailsComponent implements OnInit {
  product: any;
  loading = false;
  recommended: any[] = [];
  loadingRecommended = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    private basketService: BasketService,
    public translateService: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (!id) return;
      this.loadProduct(id);
    });
  }

  loadProduct(id: number): void {
    this.loading = true;
    this.product = null;
    this.recommended = [];

    this.productsService.getProduct(id).subscribe({
      next: (product) => {
        this.product = product;
        this.loading = false;
        this.loadRecommended(product, id);
      },
      error: () => (this.loading = false)
    });
  }

  loadRecommended(product: any, currentId: number): void {
    this.loadingRecommended = true;
    this.productsService.getProducts(1, 10, '', product.category || '').subscribe({
      next: (result) => {
        this.recommended = (result?.data ?? [])
          .filter((p: any) => p.id !== currentId)
          .slice(0, 4);
        this.loadingRecommended = false;
      },
      error: () => (this.loadingRecommended = false)
    });
  }

  addToBasket(): void {
    if (!this.product) return;
    this.basketService.addItem(this.product, 1);
  }

  addRecommendedToBasket(product: any): void {
    this.basketService.addItem(product, 1);
  }

  goToProduct(id: number): void {
    this.router.navigate(['/products', id]);
  }
}
