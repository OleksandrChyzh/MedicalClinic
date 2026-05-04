import { Routes } from '@angular/router';
import {LoginComponent} from './features/auth/login/login';

export const routes: Routes = [
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
    // НОВИЙ РОУТ
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
    title: 'Вхід в систему'
  },
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];
