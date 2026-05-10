import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common'; // Обов'язково для DatePipe
import { ClinicService } from '../../core/services/clinic.service';
import { ReviewService } from '../../core/services/review.service';
import { Direction, MedicalService, ServiceType } from '../../models/service.models';
import { Review } from '../../models/review.models';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './services.html',
  styleUrl: './services.scss'
})
export class ServicesComponent implements OnInit {
  private clinicService = inject(ClinicService);
  private reviewService = inject(ReviewService);

  // Стан для відгуків
  expandedServices = signal<Set<number>>(new Set());
  serviceReviews = signal<Map<number, Review[]>>(new Map());
  isLoadingReviews = signal<Set<number>>(new Set());

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

  toggleReviews(serviceId: number): void {
    const currentExpanded = new Set(this.expandedServices());

    // Якщо вже відкрито - закриваємо
    if (currentExpanded.has(serviceId)) {
      currentExpanded.delete(serviceId);
      this.expandedServices.set(currentExpanded);
      return;
    }

    // Відкриваємо картку
    currentExpanded.add(serviceId);
    this.expandedServices.set(currentExpanded);

    // Якщо відгуки ще не завантажувались - робимо запит
    const currentReviews = this.serviceReviews();
    if (!currentReviews.has(serviceId)) {
      this.setLoadingState(serviceId, true);

      this.reviewService.getReviewsByService(serviceId).subscribe({
        next: (reviews) => {
          const newMap = new Map(this.serviceReviews());
          newMap.set(serviceId, reviews);
          this.serviceReviews.set(newMap);
          this.setLoadingState(serviceId, false);
        },
        error: (err) => {
          console.error('Помилка завантаження відгуків', err);
          this.setLoadingState(serviceId, false);
        }
      });
    }
  }

  private setLoadingState(serviceId: number, isLoading: boolean): void {
    const loadingSet = new Set(this.isLoadingReviews());
    if (isLoading) {
      loadingSet.add(serviceId);
    } else {
      loadingSet.delete(serviceId);
    }
    this.isLoadingReviews.set(loadingSet);
  }

  // Допоміжна функція для малювання зірочок
  getStars(rating: number): string {
    return '⭐'.repeat(rating);
  }
}
