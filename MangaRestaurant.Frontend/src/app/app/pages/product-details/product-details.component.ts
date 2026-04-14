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
import { Title, Meta } from '@angular/platform-browser';
import { ProductsService } from '../../services/products.service';
import { BasketService } from '../../services/basket.service';
import { TranslateService } from '../../services/translate.service';
import { AuthService } from '../../services/auth.service';
import { SettingsService } from '../../services/settings.service';
import { TooltipModule } from 'primeng/tooltip';
import { WishlistService } from '../../services/wishlist.service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, ProgressSpinnerModule, TranslateModule, SkeletonModule, BadgeModule, Rating, InputTextarea, FormsModule, ToastModule, TooltipModule],
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
  addingToBasket = false;
  settings$! : any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService,
    private basketService: BasketService,
    public translateService: TranslateService,
    public authService: AuthService,
    private messageService: MessageService,
    private settingsService: SettingsService,
    private title: Title,
    private meta: Meta,
    public wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (!id) return;
      this.loadProduct(id);
    });

    // Language change listener for SEO
    this.translateService.onLangChange?.subscribe(() => {
      if (this.product) this.updateSeo(this.product);
    });

    if (this.authService.isAuthenticated()) {
        this.wishlistService.getWishlist().subscribe();
    }
  }

  updateSeo(product: any) {
    const isAr = this.translateService.currentLanguage === 'ar';
    const name = isAr ? (product.nameAr || product.name) : product.name;
    const desc = isAr ? (product.descriptionAr || product.description) : product.description;
    const siteTitle = isAr ? 'مطعم مانجا' : 'Manga Restaurant';
    
    // Dynamic Page Title
    this.title.setTitle(`${siteTitle} - ${name}`);

    // Meta Tags
    this.meta.updateTag({ name: 'description', content: desc });
    this.meta.updateTag({ property: 'og:title', content: `${siteTitle} - ${name}` });
    this.meta.updateTag({ property: 'og:description', content: desc });
    this.meta.updateTag({ property: 'og:image', content: product.pictureUrl });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
  }

  shareProduct() {
     if (navigator.share) {
        const isAr = this.translateService.currentLanguage === 'ar';
        const name = isAr ? (this.product.nameAr || this.product.name) : this.product.name;
        navigator.share({
           title: name,
           text: isAr ? `اكتشف ${name} في مطعم مانجا!` : `Discover ${name} at Manga Restaurant!`,
           url: window.location.href
        }).catch(() => {});
     } else {
        this.copyLink();
     }
  }

  copyLink() {
     navigator.clipboard.writeText(window.location.href).then(() => {
        this.messageService.add({
           severity: 'success',
           summary: this.translateService.instant('TOAST.SUCCESS'),
           detail: this.translateService.instant('PRODUCTS.LINK_COPIED')
        });
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
        this.updateSeo(product);
        this.loadRecommended(product, id);
      },
      error: () => (this.loading = false)
    });
  }

  submitReview(): void {
    if (!this.authService.isAuthenticated()) {
      this.messageService.add({ 
        severity: 'warn', 
        summary: this.translateService.instant('TOAST.WARN'), 
        detail: this.translateService.instant('REVIEWS.LOGIN_PROMPT') 
      });
      this.router.navigate(['/login']);
      return;
    }

    if (this.userRating === 0) {
      this.messageService.add({ 
        severity: 'error', 
        summary: this.translateService.instant('TOAST.ERROR'), 
        detail: this.translateService.instant('REVIEWS.YOUR_RATING') 
          + " " + (this.translateService.currentLanguage === 'ar' ? 'مطلوب' : 'is required')
      });
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
        this.messageService.add({ 
          severity: 'success', 
          summary: this.translateService.instant('TOAST.SUCCESS'), 
          detail: this.translateService.instant('REVIEWS.SUCCESS') 
        });
        this.resetReviewForm();
        this.loadProduct(this.product.id); // Refresh to show new review and updated rating
        this.submittingReview = false;
      },
      error: (err: any) => {
        this.messageService.add({ 
          severity: 'error', 
          summary: this.translateService.instant('TOAST.ERROR'), 
          detail: err.error?.message || this.translateService.instant('REVIEWS.FAIL') 
        });
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
    if (!this.product || this.addingToBasket) return;
    this.addingToBasket = true;
    this.basketService.addItem(this.product, 1);
    
    // Tiny delay for UX feedback
    setTimeout(() => {
      this.addingToBasket = false;
    }, 600);
  }

  addRecommendedToBasket(product: any): void {
    this.basketService.addItem(product, 1);
  }

  goToProduct(id: number): void {
    this.router.navigate(['/products', id]);
  }

  toggleWishlist(): void {
    if (!this.authService.isAuthenticated()) {
        this.router.navigate(['/login']);
        return;
    }
    this.wishlistService.toggleWishlist(this.product.id).subscribe();
  }

  isInWishlist(): boolean {
    if (!this.product) return false;
    return this.wishlistService.isInWishlistLocal(this.product.id);
  }
}
