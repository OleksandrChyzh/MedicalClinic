import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Використовуємо твій сигнал isLoggedIn для перевірки стану
  if (authService.isLoggedIn()) {
    return true;
  }

  // Якщо токена немає або юзер не залогінений — відправляємо на вхід
  return router.parseUrl('/login');
};
