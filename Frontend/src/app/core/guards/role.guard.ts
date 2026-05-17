import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = route => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const allowedRoles = route.data['roles'] as string[] | undefined;

  if (!authService.isLoggedIn()) {
    return router.parseUrl('/login');
  }

  if (!allowedRoles?.length) {
    return true;
  }

  const userRoles = authService.getUserRoles();
  const hasAllowedRole = allowedRoles.some(role => userRoles.includes(role));

  if (hasAllowedRole) {
    return true;
  }

  if (userRoles.includes('Admin')) {
    return router.parseUrl('/admin/doctors');
  }

  if (userRoles.includes('Doctor')) {
    return router.parseUrl('/doctor/profile');
  }

  if (userRoles.includes('User')) {
    return router.parseUrl('/user/profile');
  }

  return router.parseUrl('/');
};
