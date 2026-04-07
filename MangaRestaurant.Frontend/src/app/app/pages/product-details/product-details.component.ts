import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { BadgeModule } from 'primeng/badge';
import { Rating } from 'primeng/rating';
import { InputTextarea } from 'primeng/inputtextarea';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, ProgressSpinnerModule, TranslateModule, SkeletonModule, BadgeModule, Rating, InputTextarea, FormsModule, ToastModule],
  providers: [MessageService],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css'
})
export class ProductDetailsComponent implements OnInit {
  product: any;
  loading = false;
  recommended: any[] = [];
  loadingRecommended = false;
  
  userRating: number = 0;
  userComment: string = '';
  submittingReview = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    private basketService: BasketService,
    public translateService: TranslateService,
    public authService: AuthService,
    private messageService: MessageService
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
    this.resetReviewForm();

    this.productsService.getProduct(id).subscribe({
      next: (product) => {
        this.product = product;
        this.loading = false;
        this.loadRecommended(product, id);
      },
      error: () => (this.loading = false)
    });
  }

  submitReview(): void {
    if (!this.authService.isAuthenticated()) {
      this.messageService.add({ severity: 'warn', summary: 'Sign In Required', detail: 'Please login to rate this product' });
      this.router.navigate(['/login']);
      return;
    }

    if (this.userRating === 0) {
      this.messageService.add({ severity: 'error', summary: 'Rating Required', detail: 'Please select a rating' });
      return;
    }

    this.submittingReview = true;
    const reviewData = {
      productId: this.product.id,
      rating: this.userRating,
      comment: this.userComment
    };

    this.productsService.addReview(reviewData).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Thank You!', detail: 'Your review has been submitted' });
        this.resetReviewForm();
        this.loadProduct(this.product.id); // Refresh to show new review and updated rating
        this.submittingReview = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Submission Failed', detail: err.error?.message || 'Could not submit review' });
        this.submittingReview = false;
      }
    });
  }

  resetReviewForm(): void {
    this.userRating = 0;
    this.userComment = '';
  }

  loadRecommended(product: any, currentId: number): void {
    this.loadingRecommended = true;
    this.productsService.getProducts(1, 10, '', product.categoryId).subscribe({
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
