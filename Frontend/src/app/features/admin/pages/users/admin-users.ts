import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, AdminUser } from '../../../../core/services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="admin-page">
      <div class="page-header"><div><p class="eyebrow">Адмін-панель</p><h1>Користувачі</h1><p>Перегляд користувачів і блокування доступу.</p></div></div>
      <div class="panel table-wrap">
        <table><thead><tr><th>ID</th><th>Email</th><th>Логін</th><th>Телефон</th><th>Статус</th><th></th></tr></thead><tbody>
          @for (user of users(); track user.id) {
            <tr><td>#{{ user.id }}</td><td>{{ user.email }}</td><td>{{ user.userName }}</td><td>{{ user.phoneNumber || '—' }}</td><td><span [class.blocked]="user.isBlocked" class="status">{{ user.isBlocked ? 'Заблокований' : 'Активний' }}</span></td><td><button [class.danger]="!user.isBlocked" (click)="toggleBlock(user)">{{ user.isBlocked ? 'Розблокувати' : 'Заблокувати' }}</button></td></tr>
          }
        </tbody></table>
      </div>
    </section>
  `,
  styles: [`.admin-page{display:flex;flex-direction:column;gap:20px}.page-header{display:flex;justify-content:space-between}.eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}h1{margin:0;color:#0f172a}.panel{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05)}.table-wrap{overflow:auto}table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:12px;border-bottom:1px solid #e2e8f0}th{color:#64748b;font-size:.8rem;text-transform:uppercase}.status{display:inline-block;padding:5px 10px;border-radius:999px;background:#dcfce7;color:#166534;font-weight:800}.status.blocked{background:#fee2e2;color:#b91c1c}button{border:0;border-radius:10px;padding:9px 12px;font-weight:800;cursor:pointer;background:#e0f2fe;color:#0369a1}.danger{background:#fee2e2;color:#b91c1c}`]
})
export class AdminUsersComponent implements OnInit {
  private adminService = inject(AdminService);
  users = signal<AdminUser[]>([]);
  ngOnInit(): void { this.loadUsers(); }
  loadUsers(): void { this.adminService.getUsers().subscribe(data => this.users.set(data)); }
  toggleBlock(user: AdminUser): void { this.adminService.setUserBlocked(user.id, !user.isBlocked).subscribe(() => this.loadUsers()); }
}
