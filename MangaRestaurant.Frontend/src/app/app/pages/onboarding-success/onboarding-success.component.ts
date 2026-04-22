import { Component, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-onboarding-success',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, TranslateModule, TooltipModule],
  templateUrl: './onboarding-success.component.html',
  styleUrl: './onboarding-success.component.css',
  encapsulation: ViewEncapsulation.None
})
export class OnboardingSuccessComponent implements OnInit {
  newSiteUrl = '';
  subdomain = '';
  currentTheme = signal<'light' | 'dark'>(localStorage.getItem('theme') === 'dark' ? 'dark' : 'light');

  constructor(
    private route: ActivatedRoute, 
    private translate: TranslateService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.newSiteUrl = params['url'] || '';
      this.subdomain = params['slug'] || '';
    });
    this.applyTheme(this.currentTheme());
  }

  get currentLang() {
    return this.translate.currentLang || localStorage.getItem('lang') || 'en';
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

  goToSite() {
    if (this.newSiteUrl) {
      window.location.href = this.newSiteUrl;
    }
  }

  scrollTo(id: string): void {
    this.router.navigate(['/'], { fragment: id }).then(() => {
      const el = document.getElementById(id);
      if (el) el.scrollIntoView({ behavior: 'smooth' });
    });
  }
}
