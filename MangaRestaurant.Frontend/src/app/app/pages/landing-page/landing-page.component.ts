import { Component, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { CommonModule, ViewportScroller } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { SaasInfoService } from '../../services/saas-info.service';
import { SaasPlan, SaasTenant } from '../../models/saas.models';
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
  publicTenants: SaasTenant[] = [];
  plans: SaasPlan[] = [];
  loadingTenants = true;
  loadingPlans = true;
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');

  constructor(
    private saasService: SaasInfoService,
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
    this.saasService.getActiveTenants().subscribe({
      next: (data) => {
        this.publicTenants = data;
        this.loadingTenants = false;
      },
      error: () => this.loadingTenants = false
    });
  }

  loadPlans(): void {
    this.saasService.getAvailablePlans().subscribe({
      next: (data) => {
        this.plans = data;
        this.loadingPlans = false;
      },
      error: () => this.loadingPlans = false
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

  getTenantUrl(tenant: any): string {
    if (tenant.customDomain) return `${window.location.protocol}//${tenant.customDomain}`;
    
    const protocol = window.location.protocol;
    const host = window.location.host;
    const slug = tenant.slug;

    if (host.includes('localhost')) {
      const port = host.split(':')[1] || '4200';
      return `${protocol}//${slug}.localhost:${port}`;
    }
    return `${protocol}//${slug}.${host.replace('www.', '')}`;
  }

  getPlanFeatures(plan: any): string[] {
    const features: string[] = [];

    const unlimitedText = this.translate.instant('LANDING.UNLIMITED');
    const upToText = this.translate.instant('LANDING.UP_TO');

    const productsText = plan.maxProducts >= 9999
      ? `${unlimitedText} ${this.translate.instant('LANDING.PRODUCTS_COUNT')}`
      : `${upToText} ${plan.maxProducts} ${this.translate.instant('LANDING.PRODUCTS_COUNT')}`;
    features.push(productsText);

    const staffText = plan.maxStaff >= 9999
      ? `${unlimitedText} ${this.translate.instant('LANDING.STAFF_COUNT')}`
      : `${upToText} ${plan.maxStaff} ${this.translate.instant('LANDING.STAFF_COUNT')}`;
    features.push(staffText);

    if (plan.hasAdvancedReports) features.push(this.translate.instant('ADMIN.REPORTS'));
    if (plan.hasDeliveryTracking) features.push(this.translate.instant('TRACKING.TITLE'));
    if (plan.hasLuckyRewards) features.push(this.translate.instant('GACHA.TITLE'));
    if (plan.hasEmailNotifications) features.push(this.translate.instant('ADMIN.SETTINGS'));
    if (plan.hasCustomDomain) features.push(this.translate.instant('LANDING.CUSTOM_DOMAIN'));

    return features;
  }
}
