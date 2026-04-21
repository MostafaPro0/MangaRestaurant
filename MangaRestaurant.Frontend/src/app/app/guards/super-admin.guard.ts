import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const superAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  // Check for the specific SuperAdmin role
  if (auth.hasRole('SuperAdmin')) {
    return true;
  }
  
  // If not super admin, redirect to home
  return router.parseUrl('/');
};
