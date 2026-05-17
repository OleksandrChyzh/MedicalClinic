import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppointmentService } from '../../../../core/services/appointment.service';

@Component({
  selector: 'app-admin-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="admin-page">
      <div class="page-header"><div><p class="eyebrow">Адмін-панель</p><h1>Всі записи</h1><p>Перегляд записів із фільтрацією за періодом і розрахунок прибутку.</p></div></div>
      <div class="panel filters">
        <label>Від<input type="date" [(ngModel)]="filter.from" (change)="loadReport()" /></label>
        <label>До<input type="date" [(ngModel)]="filter.to" (change)="loadReport()" /></label>
        <button class="ghost" (click)="resetFilter()">Скинути фільтр</button>
      </div>
      @if (report()) {
        <div class="panel summary"><strong>Загальний прибуток за період: {{ report()?.revenue ?? 0 }} грн</strong></div>
        <div class="panel table-wrap">
          <table><thead><tr><th>Дата</th><th>Лікар</th><th>Пацієнт</th><th>Послуга</th><th>Ціна</th><th>Статус</th></tr></thead><tbody>
            @for (item of report()!.items; track item.id) {
              <tr><td>{{ formatDate(item.appointmentDate) }}</td><td>{{ item.doctorFullName }}</td><td>{{ item.patientFullName }}</td><td>{{ item.serviceName }}</td><td>{{ item.servicePrice }} грн</td><td><span [class]="statusClass(item.status)">{{ item.status }}</span></td></tr>
            }
          </tbody></table>
        </div>
      } @else {
        <div class="panel">Завантаження...</div>
      }
    </section>
  `,
  styles: [`.admin-page{display:flex;flex-direction:column;gap:20px}.page-header{display:flex;justify-content:space-between}.eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}h1{margin:0;color:#0f172a}.panel{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05)}.filters{display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap}label{display:flex;flex-direction:column;gap:6px;font-weight:700;color:#334155}input{padding:10px 12px;border:1px solid #cbd5e1;border-radius:10px}.ghost{background:#f8fafc;color:#334155;border:1px solid #cbd5e1;border-radius:10px;padding:10px 14px;font-weight:800;cursor:pointer}.summary{font-size:1.1rem;color:#166534}.table-wrap{overflow:auto}table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:12px;border-bottom:1px solid #e2e8f0}th{color:#64748b;font-size:.8rem;text-transform:uppercase}.CREATED{color:#0369a1}.CONFIRMED{color:#166534}.CANCELLED{color:#b91c1c}.COMPLETED{color:#475569}`]
})
export class AdminAppointmentsComponent {
  private appointmentService = inject(AppointmentService);
  report = signal<{ items: any[]; revenue: number } | null>(null);
  filter = { from: '', to: '' };

  ngOnInit(): void { this.loadReport(); }

  loadReport(): void {
    this.appointmentService.getAdminReport(this.filter.from || undefined, this.filter.to || undefined).subscribe(data => this.report.set(data));
  }

  resetFilter(): void {
    this.filter = { from: '', to: '' };
    this.loadReport();
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('uk-UA') + ' ' + d.toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' });
  }

  statusClass(status: string): string { return status || ''; }
}
