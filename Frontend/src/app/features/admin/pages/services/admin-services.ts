import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClinicService } from '../../../../core/services/clinic.service';
import { Direction, MedicalService, ServiceType } from '../../../../models/service.models';

@Component({
  selector: 'app-admin-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="admin-page">
      <div class="page-header"><div><p class="eyebrow">Адмін-панель</p><h1>Послуги</h1><p>Керуйте медичними послугами клініки.</p></div><button class="primary" (click)="startCreate()">+ Додати послугу</button></div>
      @if (isFormOpen()) {
        <form class="panel form-grid" (ngSubmit)="saveService()">
          <h2 class="wide">{{ editingServiceId() ? 'Редагувати послугу' : 'Нова послуга' }}</h2>
          <label>Назва<input [(ngModel)]="form.name" name="name" required /></label>
          <label>Ціна<input type="number" [(ngModel)]="form.price" name="price" min="1" required /></label>
          @if (!editingServiceId()) {
            <label>Напрямок<select [(ngModel)]="form.directionId" name="directionId" required><option [ngValue]="0">Оберіть</option>@for (d of directions(); track d.id) {<option [ngValue]="d.id">{{ d.name }}</option>}</select></label>
            <label>Тип<select [(ngModel)]="form.typeId" name="typeId" required><option [ngValue]="0">Оберіть</option>@for (t of serviceTypes(); track t.id) {<option [ngValue]="t.id">{{ t.name }}</option>}</select></label>
          }
          <label class="wide">Опис<textarea [(ngModel)]="form.description" name="description"></textarea></label>
          <div class="actions wide"><button type="button" class="ghost" (click)="cancelForm()">Скасувати</button><button class="primary" type="submit">Зберегти</button></div>
        </form>
      }
      <div class="grid-list">@for (service of services(); track service.id) {<article class="card"><h3>{{ service.name }}</h3><p>{{ service.description || 'Без опису' }}</p><strong>{{ service.price }} грн</strong><div class="actions"><button (click)="editService(service)">Редагувати</button><button class="danger" (click)="deleteService(service.id)">Видалити</button></div></article>}</div>
    </section>
  `,
  styles: [`.admin-page{display:flex;flex-direction:column;gap:20px;padding:0 1rem;max-width:1400px;margin:0 auto}.page-header{display:flex;justify-content:space-between;align-items:flex-start;gap:16px}.eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}h1,h2,h3{margin:0;color:#0f172a}.panel,.card{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05)}.form-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:14px}.wide{grid-column:1/-1}label{display:flex;flex-direction:column;gap:6px;font-weight:700;color:#334155}input,select,textarea{padding:10px 12px;border:1px solid #cbd5e1;border-radius:10px}textarea{min-height:80px}.primary{background:#0284c7;color:#fff;border:0;border-radius:10px;padding:10px 14px;font-weight:800;cursor:pointer}.ghost{background:#f8fafc;color:#334155;border:1px solid #cbd5e1;border-radius:10px;padding:10px 14px}.danger{background:#fee2e2;color:#b91c1c;border:0;border-radius:10px;padding:9px 12px}.actions{display:flex;gap:10px;justify-content:flex-end;flex-wrap:wrap}.grid-list{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:14px;padding-right:0}.card{display:flex;flex-direction:column;gap:10px}@media(max-width:760px){.form-grid{grid-template-columns:1fr}.page-header{flex-direction:column}}`]
})
export class AdminServicesComponent implements OnInit {
  private clinicService = inject(ClinicService);
  services = signal<MedicalService[]>([]);
  directions = signal<Direction[]>([]);
  serviceTypes = signal<ServiceType[]>([]);
  isFormOpen = signal(false);
  editingServiceId = signal<number | null>(null);
  form: any = this.emptyForm();

  ngOnInit(): void { this.loadData(); }

  loadData(): void {
    this.clinicService.getServices().subscribe(data => this.services.set(data));
    this.clinicService.getDirections().subscribe(data => this.directions.set(data));
    this.clinicService.getServiceTypes().subscribe(data => this.serviceTypes.set(data));
  }

  startCreate(): void { this.editingServiceId.set(null); this.form = this.emptyForm(); this.isFormOpen.set(true); }
  editService(service: MedicalService): void { this.editingServiceId.set(service.id); this.form = { id: service.id, name: service.name, description: service.description || '', price: service.price }; this.isFormOpen.set(true); }
  saveService(): void {
    const id = this.editingServiceId();
    if (id) {
      this.clinicService.updateService(id, { ...this.form, id }).subscribe(() => { this.cancelForm(); this.loadData(); });
    } else {
      this.clinicService.createService(this.form).subscribe(() => { this.cancelForm(); this.loadData(); });
    }
  }
  deleteService(id: number): void { if (!confirm('Видалити послугу?')) return; this.clinicService.deleteService(id).subscribe(() => this.loadData()); }
  cancelForm(): void { this.isFormOpen.set(false); this.editingServiceId.set(null); }
  private emptyForm(): any { return { name: '', description: '', price: 0, directionId: 0, typeId: 0 }; }
}
