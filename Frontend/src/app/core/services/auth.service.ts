import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface LoginDTO {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiration: string;
  userRole: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = 'https://localhost:7272/api/Auth';

  // Сигнал для перевірки статусу авторизації в реальному часі
  isLoggedIn = signal<boolean>(!!localStorage.getItem('token'));

  login(credentials: LoginDTO): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, credentials).pipe(
      tap(response => {
        // Зберігаємо дані в браузері
        localStorage.setItem('token', response.token);
        localStorage.setItem('userRole', response.userRole);
        this.isLoggedIn.set(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }
}
