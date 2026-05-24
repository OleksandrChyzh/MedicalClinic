import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, AdminUser } from '../../../../core/services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="admin-page">
      <div class="page-header">
        <div>
          <p class="eyebrow">Адмін-панель</p>
          <h1>Користувачі</h1>
          <p>Перегляд користувачів і блокування доступу.</p>
        </div>
      </div>

      @if (error()) {
        <div class="alert">{{ error() }}</div>
      }

      <div class="panel table-wrap">
        @if (isLoading()) {
          <div class="loading">Завантаження користувачів...</div>
        } @else {
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Email</th>
                <th>Логін</th>
                <th>Телефон</th>
                <th>Роль</th>
                <th>Статус</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (user of users(); track user.id) {
                <tr [class.blocked-row]="user.isBlocked">
                  <td>#{{ user.id }}</td>
                  <td>{{ user.email }}</td>
                  <td>{{ user.userName }}</td>
                  <td>{{ user.phoneNumber || '—' }}</td>
                  <td>{{ user.roleDisplay }}</td>
                  <td>
                    <span class="status" [class.blocked]="user.isBlocked">
                      {{ user.isBlocked ? 'Заблокований' : 'Активний' }}
                    </span>
                  </td>
                  <td class="actions-cell">
                    <button
                      [class.danger]="!user.isBlocked"
                      [disabled]="isActionDisabled(user)"
                      (click)="toggleBlock(user)">
                      {{ actionLabel(user) }}
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="7" class="empty">Користувачів не знайдено.</td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>
    </section>
  `,
  styles: [`
    .admin-page{display:flex;flex-direction:column;gap:20px;width:100%;max-width:100%;box-sizing:border-box;overflow-x:hidden}
    .page-header{display:flex;justify-content:space-between}
    .eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}
    h1{margin:0;color:#0f172a}
    p{margin:.25rem 0 0;color:#64748b}
    .panel{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05);box-sizing:border-box}
    .table-wrap{width:100%;max-width:100%;overflow-x:auto}
    table{width:100%;border-collapse:collapse;table-layout:auto}
    th,td{text-align:left;padding:12px 10px;border-bottom:1px solid #e2e8f0;vertical-align:middle}
    th{color:#64748b;font-size:.8rem;text-transform:uppercase;white-space:nowrap}
    .blocked-row{background:#fff7f7}
    .status{display:inline-block;padding:5px 10px;border-radius:999px;background:#dcfce7;color:#166534;font-weight:800;font-size:.82rem}
    .status.blocked{background:#fee2e2;color:#b91c1c}
    .actions-cell{text-align:right;width:130px}
    button{border:0;border-radius:10px;padding:9px 10px;font-weight:800;cursor:pointer;background:#e0f2fe;color:#0369a1;transition:.15s;white-space:nowrap;max-width:100%;box-sizing:border-box}
    button:hover:not(:disabled){filter:brightness(.97);transform:translateY(-1px)}
    button:disabled{opacity:.55;cursor:not-allowed}
    .danger{background:#fee2e2;color:#b91c1c}
    .alert{background:#fee2e2;color:#b91c1c;border:1px solid #fecaca;border-radius:12px;padding:12px 14px;font-weight:700}
    .loading,.empty{text-align:center;color:#64748b;padding:24px;font-weight:600}
  `]
})
export class AdminUsersComponent implements OnInit {
  private adminService = inject(AdminService);
  users = signal<AdminUser[]>([]);
  isLoading = signal(false);
  processingUserId = signal<number | null>(null);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.adminService.getUsers().subscribe({
      next: data => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Не вдалося завантажити користувачів.');
        this.isLoading.set(false);
      }
    });
  }

  toggleBlock(user: AdminUser): void {
    if (this.isActionDisabled(user)) return;

    const nextBlocked = !user.isBlocked;
    const message = nextBlocked
      ? `Заблокувати користувача ${user.email}? Він більше не зможе увійти в систему.`
      : `Розблокувати користувача ${user.email}?`;

    if (!confirm(message)) return;

    this.processingUserId.set(user.id);
    this.error.set(null);
    this.adminService.setUserBlocked(user.id, nextBlocked).subscribe({
      next: () => {
        this.processingUserId.set(null);
        this.loadUsers();
      },
      error: err => {
        this.error.set(err?.error || 'Не вдалося змінити статус користувача.');
        this.processingUserId.set(null);
      }
    });
  }

  isActionDisabled(user: AdminUser): boolean {
    return this.processingUserId() === user.id || user.role === 'Admin' || user.roleDisplay === 'Адмін';
  }

  actionLabel(user: AdminUser): string {
    if (this.processingUserId() === user.id) return 'Обробка...';
    if (user.role === 'Admin' || user.roleDisplay === 'Адмін') return 'Недоступно';
    return user.isBlocked ? 'Розблокувати' : 'Заблокувати';
  }
}
