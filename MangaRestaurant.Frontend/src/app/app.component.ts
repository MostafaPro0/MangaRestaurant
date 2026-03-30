import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from './app/services/auth.service';
import { TranslateService } from './app/services/translate.service';
import { BasketService } from './app/services/basket.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ButtonModule, TooltipModule, TranslateModule],
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'MangaRestaurant';
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');
  isAuthenticated = signal(false);
  isAdmin = signal(false);
  lang = signal<'en' | 'ar'>((localStorage.getItem('lang') as 'en' | 'ar') || 'en');
  basketCount = signal(0);
  mobileMenuOpened = signal(false);

  constructor(private auth: AuthService, private translateService: TranslateService, private basketService: BasketService) {
    this.basketService.basket$.subscribe(basket => {
      const count = basket.items.reduce((sum, item) => sum + item.quantity, 0);
      this.basketCount.set(count);
    });
    this.auth.user.subscribe((user) => {
      this.isAuthenticated.set(!!user);
      this.isAdmin.set(this.auth.isAdmin(user?.token ?? null));
    });
    this.applyTheme(this.currentTheme());
  }

  toggleTheme(): void {
    const next = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.currentTheme.set(next);
    this.applyTheme(next);
    localStorage.setItem('theme', next);
  }

  applyTheme(theme: 'light' | 'dark'): void {
    document.body.classList.toggle('theme-dark', theme === 'dark');
    document.body.classList.toggle('theme-light', theme === 'light');
  }

  setLang(lang: 'en' | 'ar'): void {
    this.translateService.setLanguage(lang);
    this.lang.set(lang);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpened.set(!this.mobileMenuOpened());
  }

  logout(): void {
    this.auth.logout();
  }
}

