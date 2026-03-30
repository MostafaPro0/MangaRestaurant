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
  if (decision === 'not-admin') return router.parseUrl('/');

  // If we can't determine admin from token, allow and let backend enforce.
  return true;
};

