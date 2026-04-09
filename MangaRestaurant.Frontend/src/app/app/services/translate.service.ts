import { Injectable } from '@angular/core';
import { TranslateService as NgxTranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class TranslateService {
  constructor(private translate: NgxTranslateService) {
    this.translate.addLangs(['en', 'ar']);
    this.translate.setDefaultLang('en');
    const saved = (localStorage.getItem('lang') || 'en') as 'en' | 'ar';
    this.setLanguage(saved);
  }

  setLanguage(lang: 'en' | 'ar'): void {
    localStorage.setItem('lang', lang);
    this.translate.use(lang);
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
  }

  get currentLanguage(): string {
    return this.translate.currentLang || this.translate.defaultLang;
  }

  instant(key: string): string {
    return this.translate.instant(key) as string;
  }

  get onLangChange() {
    return this.translate.onLangChange;
  }
}

