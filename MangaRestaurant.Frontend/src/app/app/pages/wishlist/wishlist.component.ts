import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { WishlistService } from '../../services/wishlist.service';
import { TranslateService } from '../../services/translate.service';
import { BasketService } from '../../services/basket.service';
import { SettingsService } from '../../services/settings.service';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, TranslateModule, SkeletonModule],
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.css']
})
export class WishlistComponent implements OnInit {
  items: any[] = [];
  loading = false;
  settings$! : any;

  constructor(
    private wishlistService: WishlistService,
    public translateService: TranslateService,
    private basketService: BasketService,
    private settingsService: SettingsService
  ) {}

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.loadWishlist();
  }

  loadWishlist(): void {
    this.loading = true;
    this.wishlistService.getWishlist().subscribe({
      next: (items) => {
        this.items = items;
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  removeFromWishlist(id: number): void {
    this.wishlistService.removeFromWishlist(id).subscribe(() => {
        this.items = this.items.filter(i => i.id !== id);
    });
  }

  addToBasket(product: any): void {
    this.basketService.addItem(product, 1);
  }
}
