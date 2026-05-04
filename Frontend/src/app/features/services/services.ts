import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClinicService } from '../../core/services/clinic.service';
import { Direction, MedicalService, ServiceType } from '../../models/service.models';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './services.html',
  styleUrl: './services.scss'
})
export class ServicesComponent implements OnInit {
  private clinicService = inject(ClinicService);

  // Перетворюємо наші масиви та змінні на Сигнали
  directions = signal<Direction[]>([]);
  serviceTypes = signal<ServiceType[]>([]);
  services = signal<MedicalService[]>([]);

  selectedDirectionId = signal<number | null>(null);
  selectedTypeId = signal<number | null>(null);

  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadFilters();
    this.loadServices();
  }

  private loadFilters(): void {
    // Оновлюємо значення сигналів через метод .set()
    this.clinicService.getDirections().subscribe(data => this.directions.set(data));
    this.clinicService.getServiceTypes().subscribe(data => this.serviceTypes.set(data));
  }

  loadServices(): void {
    this.isLoading.set(true);
    // Щоб отримати значення з сигналу, викликаємо його як функцію: this.selectedDirectionId()
    this.clinicService.getServices(this.selectedDirectionId(), this.selectedTypeId())
      .subscribe({
        next: (data) => {
          this.services.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Помилка завантаження послуг', err);
          this.isLoading.set(false);
        }
      });
  }

  // Скидання фільтрів
  resetFilters(): void {
    this.selectedDirectionId.set(null);
    this.selectedTypeId.set(null);
    this.loadServices();
  }
}
