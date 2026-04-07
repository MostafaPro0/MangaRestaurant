import { Component, signal } from '@angular/core';
import { environment } from '../environments/environment';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AuthService } from './app/services/auth.service';
import { TranslateService } from './app/services/translate.service';
import { BasketService } from './app/services/basket.service';
import { NotificationService } from './app/services/notification.service';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { Router } from '@angular/router';
import { User } from './app/models/user.model';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule, 
    RouterModule,
    ToastModule,
    ButtonModule, 
    TooltipModule, 
    TranslateModule,
    AvatarModule,
    MenuModule
  ],
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'MangaRestaurant';
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');
  isAuthenticated = signal(false);
  isAdmin = signal(false);
  user = signal<User | null>(null);
  lang = signal<'en' | 'ar'>((localStorage.getItem('lang') as 'en' | 'ar') || 'en');
  basketCount = signal(0);
  notificationCount = signal(0);
  mobileMenuOpened = signal(false);

  profileMenuItems: MenuItem[] = [];

  constructor(
    private auth: AuthService, 
    private translateService: TranslateService, 
    private basketService: BasketService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.notificationService.createHubConnection();
    this.notificationService.unreadCount$.subscribe(count => {
       this.notificationCount.set(count);
    });
    this.basketService.basket$.subscribe(basket => {
      const count = basket.items.reduce((sum, item) => sum + item.quantity, 0);
      this.basketCount.set(count);
    });
    this.auth.user.subscribe((user) => {
      this.user.set(user);
      this.isAuthenticated.set(!!user);
      this.isAdmin.set(this.auth.isAdmin(user?.token ?? null));
      this.initProfileMenu();
    });
    this.applyTheme(this.currentTheme());
  }

  initProfileMenu(): void {
    const isAr = this.lang() === 'ar';
    this.profileMenuItems = [
      {
        label: this.user()?.displayName || (isAr ? 'المستخدم' : 'User'),
        items: [
          {
            label: isAr ? 'طلباتي' : 'My Orders',
            icon: 'pi pi-shopping-bag',
            routerLink: '/orders'
          },
          {
            label: isAr ? 'الملف الشخصي' : 'Profile',
            icon: 'pi pi-user-edit',
            routerLink: '/profile'
          },
          {
            label: isAr ? 'خروج' : 'Logout',
            icon: 'pi pi-sign-out',
            command: () => this.logout()
          }
        ]
      }
    ];

    if (this.isAdmin()) {
      this.profileMenuItems[0].items?.unshift({
        label: isAr ? 'لوحة التحكم' : 'Admin Control',
        icon: 'pi pi-lock',
        command: () => this.router.navigate(['/admin'])
      });
    }
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
    this.initProfileMenu();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpened.set(!this.mobileMenuOpened());
  }

  resetNotificationCount(): void {
    this.notificationService.clearUnreadCount();
  }

  logout(): void {
    this.auth.logout();
  }

  getProfilePicUrl(url?: string): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return environment.apiUrl.replace('/api/', '') + url;
  }
}

