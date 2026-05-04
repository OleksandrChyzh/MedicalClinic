import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClinicService } from '../../core/services/clinic.service';
import { Direction, MedicalService, ServiceType } from '../../models/clinic.models';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [FormsModule], // Потрібно для двосторонньої прив'язки фільтрів
  templateUrl: './services.html',
  styleUrl: './services.scss'
})
export class ServicesComponent implements OnInit {
  private clinicService = inject(ClinicService);

  // Списки для відображення
  directions: Direction[] = [];
  serviceTypes: ServiceType[] = [];
  services: MedicalService[] = [];

  // Стан фільтрів
  selectedDirectionId: number | null = null;
  selectedTypeId: number | null = null;

  // Індикатор завантаження
  isLoading: boolean = true;

  ngOnInit(): void {
    this.loadFilters();
    this.loadServices();
  }

  // Завантажуємо довідники для випадаючих списків
  private loadFilters(): void {
    this.clinicService.getDirections().subscribe(data => this.directions = data);
    this.clinicService.getServiceTypes().subscribe(data => this.serviceTypes = data);
  }

  // Завантажуємо послуги (з фільтрами або без)
  loadServices(): void {
    this.isLoading = true;
    this.clinicService.getServices(this.selectedDirectionId, this.selectedTypeId)
      .subscribe({
        next: (data) => {
          this.services = data;
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Помилка завантаження послуг', err);
          this.isLoading = false;
        }
      });
  }

  // Викликається при зміні значення у <select>
  applyFilters(): void {
    this.loadServices();
  }

  // Скидання фільтрів
  resetFilters(): void {
    this.selectedDirectionId = null;
    this.selectedTypeId = null;
    this.loadServices();
  }
}
