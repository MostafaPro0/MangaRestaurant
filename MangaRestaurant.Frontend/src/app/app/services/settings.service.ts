import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { SiteSettings } from '../models/site-settings.model';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private settingsSource = new BehaviorSubject<SiteSettings | null>(null);
  settings$ = this.settingsSource.asObservable();

  constructor(private api: ApiService) {}

  loadSettings(): void {
    this.api.get<SiteSettings>('Settings').subscribe({
      next: (settings) => this.settingsSource.next(settings),
      error: (err) => console.error('Error loading site settings', err)
    });
  }

  getSettings(): SiteSettings | null {
    return this.settingsSource.value;
  }

  updateSettings(settings: SiteSettings): Observable<SiteSettings> {
    return this.api.put<SiteSettings>('Settings', settings).pipe(
      map(updated => {
        this.settingsSource.next(updated);
        return updated;
      })
    );
  }
}
