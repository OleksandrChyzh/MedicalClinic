import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http'; // ДОДАНО
import { provideRouter } from '@angular/router';
import { authInterceptor } from './core/interceptors/auth.interceptor';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(
      withInterceptors([authInterceptor]) // Підключаємо наш функціональний інтерцептор
    ),// ДОДАНО: без цього запити до API не працюватимуть!
    provideRouter(routes)
  ]
};
