import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router'; // Додано RouterModule
import { NavbarComponent } from '../../components/navbar/navbar';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterModule, NavbarComponent], // Додано RouterModule сюди
  templateUrl: './main-layout.html', // Підключаємо твій HTML-файл
  styleUrl: './main-layout.scss'     // Підключаємо твій SCSS-файл
})
export class MainLayoutComponent {
  private authService = inject(AuthService);

  // Отримуємо стан авторизації (це сигнал)
  isLoggedIn = this.authService.isLoggedIn;

  hasRole(role: string): boolean {
    return this.authService.getUserRoles().includes(role);
  }
}
