import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { AiTriageService, AiTriageResult } from '../../core/services/ai-triage.service';

@Component({
  selector: 'app-home',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomeComponent {
  readonly auth = inject(AuthService);
  private aiService = inject(AiTriageService);

  symptomText = signal('');
  isLoading = signal(false);
  result = signal<AiTriageResult | null>(null);
  errorMessage = signal<string | null>(null);

  analyze(): void {
    const text = this.symptomText().trim();
    if (!text) return;
    this.isLoading.set(true);
    this.result.set(null);
    this.errorMessage.set(null);
    this.aiService.analyze({ symptomText: text }).subscribe({
      next: (res) => {
        this.result.set(res);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Не вдалося отримати відповідь від AI. Спробуйте пізніше.');
        this.isLoading.set(false);
      }
    });
  }

  confirmBooking(): void {
    // TODO: реалізувати логіку підтвердження запису
  }
}
