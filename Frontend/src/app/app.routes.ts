import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout/main-layout';
import { ProfileComponent } from './features/user/pages/profile/profile';
import { PatientsComponent } from './features/user/pages/patients/patients'; // <--- 1. ДОДАНО ІМПОРТ НОВОГО КОМПОНЕНТА
import { AppointmentsComponent } from './features/user/pages/appointments/appointments';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent, // <--- ЄДИНИЙ Layout на весь додаток
    children: [
      // ==========================================
      // ПУБЛІЧНІ СТОРІНКИ
      // ==========================================
      {
        path: '',
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
        path: 'doctors',
        loadComponent: () => import('./features/doctors/doctors').then(m => m.DoctorsComponent),
        title: 'Наші лікарі - MediClinic'
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent),
        title: 'Вхід в систему'
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent),
        title: 'Реєстрація'
      },

      // ==========================================
      // ПРИВАТНІ СТОРІНКИ (Тепер вони ТУТ, всередині MainLayout)
      // ==========================================
      {
        path: 'user',
        canActivate: [authGuard], // Захищаємо цю групу
        children: [
          { path: '', redirectTo: 'profile', pathMatch: 'full' },
          { path: 'profile', component: ProfileComponent, title: 'Мій профіль' },
          { path: 'patients', component: PatientsComponent, title: 'Мої пацієнти' }, // <--- 2. ДОДАНО РОУТ ДЛЯ ПАЦІЄНТІВ
          {
            path: 'appointments',
            component: AppointmentsComponent,
            title: 'Мої записи на прийом'
          }
        ]
      }
    ]
  },

  // ==========================================
  // FALLBACK МАРШРУТ
  // ==========================================
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];
