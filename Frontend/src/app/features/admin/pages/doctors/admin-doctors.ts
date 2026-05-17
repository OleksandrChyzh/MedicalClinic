import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of, switchMap } from 'rxjs';
import { DoctorService } from '../../../../core/services/doctor.service';
import { ClinicService } from '../../../../core/services/clinic.service';
import { ScheduleService } from '../../../../core/services/schedule.service';
import { Doctor } from '../../../../models/doctor.model';
import { Direction } from '../../../../models/service.models';
import { Schedule } from '../../../../models/schedule.model';

@Component({
  selector: 'app-admin-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="admin-page">
      <div class="page-header">
        <div>
          <p class="eyebrow">Адмін-панель</p>
          <h1>Лікарі</h1>
          <p>Додавайте, редагуйте та видаляйте лікарів. При створенні автоматично формується базовий розклад.</p>
        </div>
        <button class="primary" (click)="startCreate()">+ Додати лікаря</button>
      </div>

      @if (isFormOpen()) {
        <form class="panel form-grid" (ngSubmit)="saveDoctor()">
          <h2>{{ editingDoctorId() ? 'Редагувати лікаря' : 'Новий лікар' }}</h2>
          @if (errorMessage()) {
            <div class="error-message wide">{{ errorMessage() }}</div>
          }
          <label>Напрямок<select [(ngModel)]="form.directionId" name="directionId" required><option [ngValue]="0">Оберіть</option>@for (d of directions(); track d.id) {<option [ngValue]="d.id">{{ d.name }}</option>}</select></label>
          <label>Прізвище<input [(ngModel)]="form.lastName" name="lastName" required /></label>
          <label>Ім'я<input [(ngModel)]="form.firstName" name="firstName" required /></label>
          <label>По батькові<input [(ngModel)]="form.middleName" name="middleName" /></label>
          <label>Досвід<input type="number" [(ngModel)]="form.experienceYears" name="experienceYears" min="0" /></label>
          <label class="wide">Опис<textarea [(ngModel)]="form.description" name="description"></textarea></label>

          @if (!editingDoctorId()) {
            <label>Email акаунта<input type="email" [(ngModel)]="form.account.email" name="email" required /></label>
            <label>Логін<input [(ngModel)]="form.account.userName" name="userName" required /></label>
            <label>Телефон<input [(ngModel)]="form.account.phoneNumber" name="phoneNumber" /></label>
            <label>Пароль<input type="password" [(ngModel)]="form.account.password" name="password" required /></label>
          }

          <div class="wide schedule-editor">
            <h3>Розклад лікаря</h3>
            @for (item of schedule(); track item.weekDay) {
              <div class="schedule-row">
                <strong>{{ item.weekDay }}</strong>
                <input type="time" [(ngModel)]="item.startTime" [name]="item.weekDay + 'start'" />
                <input type="time" [(ngModel)]="item.endTime" [name]="item.weekDay + 'end'" />
              </div>
            }
          </div>

          <div class="actions wide">
            <button type="button" class="ghost" (click)="cancelForm()">Скасувати</button>
            <button class="primary" type="submit">Зберегти</button>
          </div>
        </form>
      }

      <div class="grid-list">
        @for (doctor of doctors(); track doctor.id) {
          <article class="card">
            <div><h3>{{ doctor.fullName }}</h3><p>{{ doctor.directionName }}</p></div>
            <p>{{ doctor.experienceYears }} років досвіду</p>
            <p>{{ doctor.contactEmail || 'email не вказано' }}</p>
            <div class="actions"><button (click)="editDoctor(doctor)">Редагувати</button><button class="danger" (click)="deleteDoctor(doctor.id)">Видалити</button></div>
          </article>
        }
      </div>
    </section>
  `,
  styles: [`
    .admin-page{display:flex;flex-direction:column;gap:20px}.page-header{display:flex;justify-content:space-between;gap:16px;align-items:flex-start}.eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}h1,h2,h3{margin:0;color:#0f172a}.panel,.card{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05)}.form-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:14px}.wide{grid-column:1/-1}label{display:flex;flex-direction:column;gap:6px;font-weight:700;color:#334155}input,select,textarea{padding:10px 12px;border:1px solid #cbd5e1;border-radius:10px}textarea{min-height:80px}.primary{background:#0284c7;color:#fff;border:0;border-radius:10px;padding:10px 14px;font-weight:800;cursor:pointer}.ghost{background:#f8fafc;color:#334155;border:1px solid #cbd5e1;border-radius:10px;padding:10px 14px}.danger{background:#fee2e2;color:#b91c1c;border:0;border-radius:10px;padding:9px 12px}.actions{display:flex;gap:10px;justify-content:flex-end;flex-wrap:wrap}.grid-list{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:14px}.card{display:flex;flex-direction:column;gap:10px}.schedule-editor{display:flex;flex-direction:column;gap:10px}.schedule-row{display:grid;grid-template-columns:1fr 120px 120px;gap:10px;align-items:center}.error-message{background:#fee2e2;color:#b91c1c;padding:12px;border-radius:10px;font-weight:700}@media(max-width:760px){.form-grid{grid-template-columns:1fr}.page-header{flex-direction:column}.schedule-row{grid-template-columns:1fr}}
  `]
})
export class AdminDoctorsComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private clinicService = inject(ClinicService);
  private scheduleService = inject(ScheduleService);

  doctors = signal<Doctor[]>([]);
  directions = signal<Direction[]>([]);
  schedule = signal<any[]>([]);
  isFormOpen = signal(false);
  editingDoctorId = signal<number | null>(null);
  errorMessage = signal<string | null>(null);

  readonly weekDays = ['Понеділок', 'Вівторок', 'Середа', 'Четвер', "П'ятниця", 'Субота', 'Неділя'];
  form: any = this.createEmptyForm();

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.doctorService.getAllDoctors().subscribe(data => this.doctors.set(data));
    this.clinicService.getDirections().subscribe(data => this.directions.set(data));
  }

  startCreate(): void {
    this.editingDoctorId.set(null);
    this.form = this.createEmptyForm();
    this.schedule.set(this.defaultSchedule());
    this.isFormOpen.set(true);
  }

  editDoctor(doctor: Doctor): void {
    const [firstName = '', lastName = ''] = doctor.fullName.split(' ');
    this.editingDoctorId.set(doctor.id);
    this.form = {
      id: doctor.id,
      directionId: doctor.directionId,
      firstName,
      lastName,
      middleName: '',
      experienceYears: doctor.experienceYears,
      description: doctor.description || ''
    };
    this.scheduleService.getDoctorSchedule(doctor.id).subscribe(items => {
      const byDay = new Map(items.map(item => [item.weekDay, item]));
      this.schedule.set(this.weekDays.map(day => byDay.get(day) || { doctorId: doctor.id, weekDay: day, startTime: '09:00', endTime: '17:00' }));
    });
    this.isFormOpen.set(true);
  }

  saveDoctor(): void {
    this.errorMessage.set(null);
    const id = this.editingDoctorId();
    if (id) {
      const dto = { ...this.form, id };
      this.doctorService.updateDoctor(id, dto).pipe(switchMap(() => this.saveSchedule(id))).subscribe({
        next: () => { this.cancelForm(); this.loadData(); },
        error: (err) => this.handleError(err)
      });
      return;
    }

    this.doctorService.createDoctor(this.form).pipe(
      switchMap(created => this.saveSchedule(created.id))
    ).subscribe({
      next: () => { this.cancelForm(); this.loadData(); },
      error: (err) => this.handleError(err)
    });
  }

  private handleError(err: any): void {
    console.error('Помилка при збереженні лікаря:', err);
    if (err.status === 400 || err.error?.errors) {
      const errors = err.error?.errors || [];
      if (errors.some((e: any) => e.includes('Email'))) {
        this.errorMessage.set('Користувач з таким email вже існує.');
      } else {
        this.errorMessage.set('Перевірте введені дані.');
      }
    } else {
      this.errorMessage.set('Сталася помилка при збереженні лікаря.');
    }
  }

  private saveSchedule(doctorId: number) {
    const requests = this.schedule().map(item => {
      const dto = { ...item, doctorId };
      return dto.id ? this.scheduleService.updateSchedule(dto) : this.scheduleService.createSchedule(dto);
    });
    return requests.length ? forkJoin(requests) : of([]);
  }

  deleteDoctor(id: number): void {
    if (!confirm('Видалити лікаря?')) return;
    this.doctorService.deleteDoctor(id).subscribe(() => this.loadData());
  }

  cancelForm(): void {
    this.isFormOpen.set(false);
    this.editingDoctorId.set(null);
    this.errorMessage.set(null);
  }

  private createEmptyForm(): any {
    return { directionId: 0, lastName: '', firstName: '', middleName: '', experienceYears: 0, description: '', account: { email: '', password: '', userName: '', phoneNumber: '' } };
  }

  private defaultSchedule(): any[] {
    return this.weekDays.map(day => ({ weekDay: day, startTime: day === 'Субота' || day === 'Неділя' ? '10:00' : '09:00', endTime: day === 'Субота' || day === 'Неділя' ? '14:00' : '17:00' }));
  }
}
