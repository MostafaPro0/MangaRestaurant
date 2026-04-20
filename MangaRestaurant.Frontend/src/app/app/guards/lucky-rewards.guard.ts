import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { SettingsService } from '../services/settings.service';
import { map, take } from 'rxjs/operators';

export const luckyRewardsGuard: CanActivateFn = () => {
  const settingsService = inject(SettingsService);
  const router = inject(Router);

  return settingsService.settings$.pipe(
    take(1),
    map(settings => {
      if (settings?.isLuckyRewardsEnabled) {
        return true;
      }
      
      // If disabled, redirect to home
      router.navigate(['/']);
      return false;
    })
  );
};
