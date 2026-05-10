import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout/main-layout';
import { UserLayoutComponent } from './features/user/layout/user-layout/user-layout';
import { ProfileComponent } from './features/user/pages/profile/profile';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // ==========================================
  // ПУБЛІЧНА ГІЛКА (використовує MainLayout з Navbar)
  // ==========================================
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        // Ліниве завантаження компонента
        loadComponent: () => import('./features/home/home').then(m => m.HomeComponent),
        title: 'Головна - MediClinic'
      },
      {
        path: 'emergency',
        loadComponent: () => import('./features/emergency/emergency').then(m => m.EmergencyComponent),
        title: 'Невідкладна допомога'
      },
      {
        path: 'services',
        loadComponent: () => import('./features/services/services').then(m => m.ServicesComponent),
        title: 'Послуги'
      },
      {
        path: 'contacts',
        loadComponent: () => import('./features/contacts/contacts').then(m => m.ContactsComponent),
        title: 'Контакти'
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent),
        title: 'Вхід в систему'
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent),
        title: 'Реєстрація' // Виправлено назву тайтлу
      }
    ]
  },

  // ==========================================
  // ПРИВАТНА ГІЛКА (використовує UserLayout з Navbar та Sidebar)
  // ==========================================
  {
    path: 'user',
    component: UserLayoutComponent,
    canActivate: [authGuard], // Захист маршруту
    children: [
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
      { path: 'profile', component: ProfileComponent },
      // У майбутньому тут будуть: { path: 'appointments', component: AppointmentsComponent }
    ]
  },

  // ==========================================
  // FALLBACK МАРШРУТ (Завжди в кінці)
  // ==========================================
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];
