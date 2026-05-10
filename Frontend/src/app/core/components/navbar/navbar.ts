import { Component, inject} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class NavbarComponent {
  private authService = inject(AuthService);

  // Отримуємо доступ до сигналу з сервісу
  isLoggedIn = this.authService.isLoggedIn;

  onLogout(): void {
    // AuthService.logout() вже очищає localStorage та робить редирект
    this.authService.logout();
  }
}
