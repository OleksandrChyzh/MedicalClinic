import { Component } from '@angular/core';

@Component({
  selector: 'app-profile',
  standalone: true,
  template: `
    <div class="profile-page">
      <h1 class="page-title">Мій профіль</h1>
      <div class="card profile-card">
        <div class="user-info">
          <div class="avatar">ІП</div>
          <div class="details">
            <h2>Іван Петренко</h2>
            <p>Пацієнт</p>
            <p class="email">ivan.petrenko&#64;example.com</p>
          </div>
        </div>
        <button class="btn btn-outline mt-3">Редагувати дані</button>
      </div>
    </div>
  `,
  styles: [`
    .page-title { font-size: 1.8rem; margin-bottom: 1.5rem; color: #1e293b; }
    .profile-card { padding: 2rem; max-width: 600px; background: #fff; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
    .user-info { display: flex; gap: 1.5rem; align-items: center; }
    .avatar { width: 80px; height: 80px; background: #0ea5e9; color: white; border-radius: 50%; display: flex; justify-content: center; align-items: center; font-size: 2rem; font-weight: bold; }
    .details h2 { margin: 0 0 0.25rem 0; font-size: 1.4rem; }
    .details p { margin: 0; color: #64748b; }
    .mt-3 { margin-top: 1.5rem; }
  `]
})
export class ProfileComponent {}
