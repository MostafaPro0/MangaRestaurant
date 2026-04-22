import { Component, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { CommonModule, ViewportScroller } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, RippleModule, TooltipModule, SkeletonModule],
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class LandingPageComponent implements OnInit {
  publicTenants: any[] = [];
  plans: any[] = [];
  loadingTenants = true;
  loadingPlans = true;
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');

  // Fallback plans if API returns empty or fails
  private fallbackPlans = [
    {
      name: 'Free', nameAr: 'مجاني', monthlyPrice: 0, maxProducts: 20, maxStaff: 2,
      hasLuckyRewards: false, hasAdvancedReports: false, hasCustomDomain: false,
      hasDeliveryTracking: false, hasEmailNotifications: false
    },
    {
      name: 'Professional', nameAr: 'احترافي', monthlyPrice: 99, maxProducts: 200, maxStaff: 10,
      hasLuckyRewards: true, hasAdvancedReports: true, hasCustomDomain: false,
      hasDeliveryTracking: true, hasEmailNotifications: true
    },
    {
      name: 'Enterprise', nameAr: 'مؤسسي', monthlyPrice: 299, maxProducts: 999999, maxStaff: 999999,
      hasLuckyRewards: true, hasAdvancedReports: true, hasCustomDomain: true,
      hasDeliveryTracking: true, hasEmailNotifications: true
    }
  ];

  constructor(
    private http: HttpClient,
    private translate: TranslateService,
    private scroller: ViewportScroller
  ) {}

  get currentLang(): string {
    return this.translate.currentLang || localStorage.getItem('lang') || 'en';
  }

  ngOnInit(): void {
    this.loadTenants();
    this.loadPlans();
    this.applyTheme(this.currentTheme());
  }

  loadTenants(): void {
    this.http.get<any[]>(`${environment.apiUrl}/saas-info/active-tenants`).subscribe({
      next: (data) => {
        this.publicTenants = data || [];
        this.loadingTenants = false;
      },
      error: () => {
        this.loadingTenants = false;
      }
    });
  }

  loadPlans(): void {
    this.http.get<any[]>(`${environment.apiUrl}/saas-info/plans`).subscribe({
      next: (data) => {
        this.plans = (data && data.length > 0) ? data : this.fallbackPlans;
        this.loadingPlans = false;
      },
      error: () => {
        this.plans = this.fallbackPlans;
        this.loadingPlans = false;
      }
    });
  }

  scrollTo(id: string): void {
    const el = document.getElementById(id);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'start' });
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
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
  }

  getTenantUrl(slug: string): string {
    const protocol = window.location.protocol;
    const host = window.location.host;
    if (host.includes('localhost')) {
      const port = host.split(':')[1] || '4200';
      return `${protocol}//${slug}.localhost:${port}`;
    }
    return `${protocol}//${slug}.${host.replace('www.', '')}`;
  }

  getPlanFeatures(plan: any): string[] {
    const isAr = this.currentLang === 'ar';
    const features: string[] = [];

    features.push(isAr ? `حتى ${plan.maxProducts} منتج` : `Up to ${plan.maxProducts} products`);
    features.push(isAr ? `حتى ${plan.maxStaff} موظفين` : `Up to ${plan.maxStaff} staff members`);

    if (plan.hasAdvancedReports) features.push(isAr ? 'تقارير متقدمة' : 'Advanced Reports');
    if (plan.hasDeliveryTracking) features.push(isAr ? 'تتبع التوصيل' : 'Delivery Tracking');
    if (plan.hasLuckyRewards) features.push(isAr ? 'نظام المكافآت' : 'Lucky Rewards System');
    if (plan.hasEmailNotifications) features.push(isAr ? 'إشعارات البريد' : 'Email Notifications');
    if (plan.hasCustomDomain) features.push(isAr ? 'دومين خاص' : 'Custom Domain');

    return features;
  }
}
