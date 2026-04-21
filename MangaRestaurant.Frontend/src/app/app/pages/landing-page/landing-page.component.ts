import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { RippleModule } from 'primeng/ripple';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, CardModule, RippleModule, TooltipModule, SkeletonModule],
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css']
})
export class LandingPageComponent implements OnInit {
  publicTenants: any[] = [];
  plans: any[] = [];
  loadingTenants = true;
  loadingPlans = true;
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');

  constructor(private http: HttpClient, private translate: TranslateService) {}

  get currentLang(): string {
    return this.translate.currentLang || 'en';
  }

  ngOnInit(): void {
    this.loadTenants();
    this.loadPlans();
    this.applyTheme(this.currentTheme());
  }

  loadTenants(): void {
    this.http.get<any[]>(`${environment.apiUrl}/saas-info/active-tenants`).subscribe({
      next: (data) => {
        this.publicTenants = data;
        this.loadingTenants = false;
      },
      error: (err) => {
        console.error('Error loading tenants', err);
        this.loadingTenants = false;
      }
    });
  }

  loadPlans(): void {
    this.http.get<any[]>(`${environment.apiUrl}/saas-info/plans`).subscribe({
      next: (data) => {
        this.plans = data;
        this.loadingPlans = false;
      },
      error: (err) => {
        console.error('Error loading plans', err);
        this.loadingPlans = false;
      }
    });
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
}
