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
import { TenantService } from './app/services/tenant.service';

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
  showStoreLayout = signal(true);
  isPlatformMode = signal(false);

  profileMenuItems: MenuItem[] = [];

  constructor(
    private auth: AuthService, 
    private translateService: TranslateService, 
    private basketService: BasketService,
    private router: Router,
    public notificationService: NotificationService,
    private settingsService: SettingsService,
    private titleService: Title,
    private metaService: Meta,
    private tenantService: TenantService
  ) {
    this.isPlatformMode.set(this.tenantService.isPlatform());
    this.settings$ = this.settingsService.settings$;
    this.settingsService.loadSettings();
    this.notificationService.createHubConnection();

    // Listen to router events for Layout Switching and SEO
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updatePageInfo();
      this.checkLayout();
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
    const siteName = this.translateService.instant('COMMON.SITE_NAME');
    const currentUrl = this.router.url;
    
    let pageTitleKey = '';
    
    if (currentUrl === '/' || currentUrl === '/home') {
      pageTitleKey = 'NAV.HOME';
    } else if (currentUrl.includes('/products') && !currentUrl.includes('/product/')) {
      pageTitleKey = 'NAV.MENU';
    } else if (currentUrl.includes('/basket')) {
      pageTitleKey = 'BASKET.TITLE';
    } else if (currentUrl.includes('/checkout')) {
      pageTitleKey = 'CHECKOUT.TITLE';
    } else if (currentUrl.includes('/admin')) {
      const tab = currentUrl.split('/').pop();
      if (tab === 'reports') pageTitleKey = 'ADMIN.REPORTS';
      else if (tab === 'orders') pageTitleKey = 'ADMIN.ORDER_LOG';
      else if (tab === 'products') pageTitleKey = 'ADMIN.PRODUCTS';
      else if (tab === 'users') pageTitleKey = 'ADMIN.STAFF_MANAGEMENT';
      else if (tab === 'settings') pageTitleKey = 'ADMIN.SETTINGS';
      else pageTitleKey = 'ADMIN.DASHBOARD_TITLE';
    } else if (currentUrl.includes('/profile')) {
      pageTitleKey = 'PROFILE.TITLE';
    } else if (currentUrl.includes('/orders')) {
      pageTitleKey = 'ORDERS.TITLE';
    } else if (currentUrl.includes('/wishlist')) {
      pageTitleKey = 'NAV.FAVORITES';
    }

    if (pageTitleKey) {
      this.titleService.setTitle(`${siteName} - ${this.translateService.instant(pageTitleKey)}`);
    } else if (!currentUrl.includes('/product/')) {
       this.titleService.setTitle(siteName);
    }
    
    // Global Meta Description
    const siteDesc = this.translateService.instant('COMMON.SITE_DESCRIPTION');
    this.metaService.updateTag({ name: 'description', content: siteDesc });
  }

  initProfileMenu(): void {
    this.profileMenuItems = [
      {
        label: this.user()?.displayName || this.translateService.instant('COMMON.USER'),
        items: [
          {
            label: this.translateService.instant('ORDERS.TITLE'),
            icon: 'pi pi-shopping-bag',
            routerLink: '/orders'
          },
          {
            label: this.translateService.instant('NAV.FAVORITES'),
            icon: 'pi pi-heart',
            routerLink: '/wishlist'
          },
          {
            label: this.translateService.instant('PROFILE.TITLE'),
            icon: 'pi pi-user-edit',
            routerLink: '/profile'
          },
          {
            label: this.translateService.instant('AUTH.LOGOUT'),
            icon: 'pi pi-sign-out',
            command: () => this.logout()
          }
        ]
      }
    ];

    if (this.isAdmin()) {
      this.profileMenuItems[0].items?.unshift({
        label: this.translateService.instant('ADMIN.DASHBOARD_TITLE'),
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

  checkLayout(): void {
    const url = this.router.url;
    const isLanding = url === '/landing' || url.includes('/onboarding') || url.includes('/onboarding-success') || (url === '/' && this.isPlatformMode());
    const isSuperAdmin = url.includes('/super-admin');
    
    // Hide store navbar/footer for landing pages and super admin dashboard
    this.showStoreLayout.set(!isLanding && !isSuperAdmin);
  }

  getProfilePicUrl(url?: string): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return environment.apiUrl.replace('/api/', '') + url;
  }
}
