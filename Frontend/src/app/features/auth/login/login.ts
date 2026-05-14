import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    // Використовуємо getRawValue() для отримання даних форми
    this.authService.login(this.loginForm.getRawValue() as any).subscribe({
      next: (response) => {
        this.isLoading.set(false);

        // Отримуємо ролі з відповіді бекенду
        const roles = response.roles || [];

        // Логіка перенаправлення залежно від ролі
        if (roles.includes('Doctor')) {
          this.router.navigate(['/doctor/profile']);
        } else if (roles.includes('User')) {
          this.router.navigate(['/user/profile']);
        } else if (roles.includes('Admin')) {
          this.router.navigate(['/admin/dashboard']);
        } else {
          // Фолбек на головну, якщо ролі не визначені
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        // Обробка помилок авторизації
        if (err.status === 401) {
          this.errorMessage.set('Невірний email або пароль');
        } else if (err.status === 403) {
          this.errorMessage.set('Доступ заборонено');
        } else {
          this.errorMessage.set('Помилка сервера. Спробуйте пізніше');
        }
      }
    });
  }
}
