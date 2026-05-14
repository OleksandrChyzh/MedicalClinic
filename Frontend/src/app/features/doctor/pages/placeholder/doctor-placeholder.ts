import { Component } from '@angular/core';

@Component({
  selector: 'app-doctor-placeholder',
  standalone: true,
  template: `
    <div class="placeholder-card">
      <h2>Розділ у розробці</h2>
      <p>Функціонал цієї сторінки буде додано пізніше.</p>
    </div>
  `,
  styles: [`
    .placeholder-card {
      padding: 2rem;
      background: #ffffff;
      border-radius: 1rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      color: #475569;
    }

    h2 {
      margin: 0 0 0.5rem;
      color: #0f172a;
    }

    p {
      margin: 0;
    }
  `]
})
export class DoctorPlaceholderComponent {}
