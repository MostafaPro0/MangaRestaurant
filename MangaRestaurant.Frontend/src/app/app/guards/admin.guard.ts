import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  const decision = auth.getAdminDecision();
  if (decision === 'admin') return true;
  
  // If not admin, redirect to home and deny access
  return router.parseUrl('/');
};
