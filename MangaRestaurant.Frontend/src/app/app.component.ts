import { Component, signal } from '@angular/core';
import { environment } from '../environments/environment';
import { CommonModule } from '@angular/common';
import { RouterModule, NavigationEnd, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Title, Meta } from '@angular/platform-browser';
import { filter } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { MenuItem } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { OverlayPanelModule } from 'primeng/overlaypanel';

import { AuthService } from './app/services/auth.service';
import { TranslateService } from './app/services/translate.service';
import { BasketService } from './app/services/basket.service';
import { NotificationService } from './app/services/notification.service';
import { SettingsService } from './app/services/settings.service';
import { User } from './app/models/user.model';
import { SiteSettings } from './app/models/site-settings.model';

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
    MenuModule,
    OverlayPanelModule
  ],
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'MangaRestaurant';
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');
  isAuthenticated = signal(false);
  isAdmin    = signal(false);
  isDelivery = signal(false);
  user = signal<User | null>(null);
  lang = signal<'en' | 'ar'>((localStorage.getItem('lang') as 'en' | 'ar') || 'en');
  basketCount = signal(0);
  notificationCount = signal(0);
  notifications = signal<any[]>([]);
  mobileMenuOpened = signal(false);
  settings$! : Observable<SiteSettings | null>;

  profileMenuItems: MenuItem[] = [];

  constructor(
    private auth: AuthService, 
    private translateService: TranslateService, 
    private basketService: BasketService,
    private router: Router,
    public notificationService: NotificationService,
    private settingsService: SettingsService,
    private titleService: Title,
    private metaService: Meta
  ) {
    this.settings$ = this.settingsService.settings$;
    this.settingsService.loadSettings();
    this.notificationService.createHubConnection();

    // Listen to router events for Dynamic Titles and SEO
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updatePageInfo();
    });

    // Listen to language changes
    this.translateService.onLangChange.subscribe(() => {
      this.updatePageInfo();
    });
    
    this.notificationService.unreadCount$.subscribe(count => {
       this.notificationCount.set(count);
    });
    this.notificationService.notifications$.subscribe(notifs => {
       this.notifications.set(notifs);
    });
    this.basketService.basket$.subscribe(basket => {
      const count = basket.items.reduce((sum, item) => sum + item.quantity, 0);
      this.basketCount.set(count);
    });
    this.auth.user.subscribe((user) => {
      this.user.set(user);
      this.isAuthenticated.set(!!user);
      this.isAdmin.set(this.auth.isAdmin(user?.token ?? null));
      this.isDelivery.set(this.auth.isDelivery(user?.token ?? null));
      this.initProfileMenu();
    });
    this.applyTheme(this.currentTheme());
  }

  updatePageInfo(): void {
    const isAr = this.translateService.currentLanguage === 'ar';
    const siteName = isAr ? 'مطعم مانجا' : 'Manga Restaurant';
    const currentUrl = this.router.url;
    
    let pageTitle = '';
    
    if (currentUrl === '/' || currentUrl === '/home') {
      pageTitle = isAr ? 'الرئيسية' : 'Home';
    } else if (currentUrl.includes('/products') && !currentUrl.includes('/product/')) {
      pageTitle = isAr ? 'القائمة' : 'Menu';
    } else if (currentUrl.includes('/basket')) {
      pageTitle = isAr ? 'سلّة المشتريات' : 'Shopping Cart';
    } else if (currentUrl.includes('/checkout')) {
      pageTitle = isAr ? 'إتمام الدفع' : 'Checkout';
    } else if (currentUrl.includes('/admin')) {
      const tab = currentUrl.split('/').pop();
      if (tab === 'reports') pageTitle = isAr ? 'التقارير' : 'Reports';
      else if (tab === 'orders') pageTitle = isAr ? 'سجل الطلبات' : 'Order Log';
      else if (tab === 'products') pageTitle = isAr ? 'المنتجات' : 'Products';
      else if (tab === 'users') pageTitle = isAr ? 'الموظفين' : 'Employees';
      else if (tab === 'settings') pageTitle = isAr ? 'الإعدادات' : 'Settings';
      else pageTitle = isAr ? 'لوحة التحكم' : 'Admin Control';
    } else if (currentUrl.includes('/profile')) {
      pageTitle = isAr ? 'الملف الشخصي' : 'Profile';
    } else if (currentUrl.includes('/orders')) {
      pageTitle = isAr ? 'طلباتي' : 'My Orders';
    }

    if (pageTitle) {
      this.titleService.setTitle(`${siteName} - ${pageTitle}`);
    } else if (!currentUrl.includes('/product/')) {
       this.titleService.setTitle(siteName);
    }
    
    // Global Meta Description
    const siteDesc = isAr 
      ? 'مطعم مانجا - تجربة الطعم الياباني الأصيل من أفخر أنواع الرامن والسوشي.' 
      : 'Manga Restaurant - Authentic Japanese taste from the finest Ramen and Sushi.';
    this.metaService.updateTag({ name: 'description', content: siteDesc });
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
            label: isAr ? 'قائمة الأمنيات' : 'My Wishlist',
            icon: 'pi pi-heart',
            routerLink: '/wishlist'
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

  handleNotificationItemClick(notification: any, op: any): void {
    op.hide();
    this.notificationService.handleNotificationClick({ message: { data: notification } });
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
