import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const deliveryGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  // Allow Admin or Delivery roles
  if (auth.isDelivery() || auth.isAdmin()) return true;

  return router.parseUrl('/');
};
