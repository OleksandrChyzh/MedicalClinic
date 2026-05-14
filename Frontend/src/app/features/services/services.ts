import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common'; // Обов'язково для DatePipe
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs/operators';
import { ClinicService } from '../../core/services/clinic.service';
import { ReviewService } from '../../core/services/review.service';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { Direction, MedicalService, ServiceType } from '../../models/service.models';
import { Review } from '../../models/review.models';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './services.html',
  styleUrl: './services.scss'
})
export class ServicesComponent implements OnInit {
  private clinicService = inject(ClinicService);
  private reviewService = inject(ReviewService);
  private userService = inject(UserService);
  private router = inject(Router);
  readonly auth = inject(AuthService);

  readonly ratingStars = [1, 2, 3, 4, 5] as const;
  composeRating = signal<number>(5);
  composeComment = signal<string>('');
  isSubmittingReview = signal(false);
  reviewSubmitError = signal<string | null>(null);

  // Стан для відгуків
  expandedServices = signal<Set<number>>(new Set());
  serviceReviews = signal<Map<number, Review[]>>(new Map());
  isLoadingReviews = signal<Set<number>>(new Set());
  expandedReviewForms = signal<Set<number>>(new Set());

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

    if (currentExpanded.has(serviceId)) {
      currentExpanded.delete(serviceId);
      this.expandedServices.set(currentExpanded);
      this.closeReviewForm(serviceId);
      return;
    }

    const next = new Set<number>([serviceId]);
    this.expandedServices.set(next);
    this.composeRating.set(5);
    this.composeComment.set('');
    this.reviewSubmitError.set(null);
    this.expandedReviewForms.set(new Set());

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

  toggleReviewForm(serviceId: number): void {
    const forms = new Set(this.expandedReviewForms());

    if (forms.has(serviceId)) {
      forms.delete(serviceId);
    } else {
      forms.clear();
      forms.add(serviceId);
      this.composeRating.set(5);
      this.composeComment.set('');
      this.reviewSubmitError.set(null);
    }

    this.expandedReviewForms.set(forms);
  }

  private closeReviewForm(serviceId: number): void {
    const forms = new Set(this.expandedReviewForms());
    forms.delete(serviceId);
    this.expandedReviewForms.set(forms);
  }

  // Допоміжна функція для малювання зірочок
  getStars(rating: number): string {
    return '⭐'.repeat(rating);
  }

  submitServiceReview(serviceId: number): void {
    if (!this.auth.isLoggedIn()) return;
    const rating = this.composeRating();
    const comment = this.composeComment().trim();
    if (rating < 1 || rating > 5) return;

    this.isSubmittingReview.set(true);
    this.reviewSubmitError.set(null);

    this.userService
      .getProfile()
      .pipe(
        switchMap(profile =>
          this.reviewService.createReview({
            userId: profile.id,
            doctorId: null,
            serviceId,
            rating,
            comment: comment || null
          })
        ),
        finalize(() => this.isSubmittingReview.set(false))
      )
      .subscribe({
        next: () => {
          this.composeRating.set(5);
          this.composeComment.set('');
          this.closeReviewForm(serviceId);
          this.reloadServiceReviews(serviceId);
        },
        error: (err: unknown) => {
          console.error('Помилка відправки відгуку', err);
          const httpErr = err as { error?: string | { message?: string; title?: string } };
          const body = httpErr?.error;
          const msg =
            typeof body === 'string'
              ? body
              : body?.message || body?.title || 'Не вдалося надіслати відгук. Спробуйте пізніше.';
          this.reviewSubmitError.set(msg);
        }
      });
  }

  private reloadServiceReviews(serviceId: number): void {
    this.reviewService.getReviewsByService(serviceId).subscribe({
      next: reviews => {
        const newMap = new Map(this.serviceReviews());
        newMap.set(serviceId, reviews);
        this.serviceReviews.set(newMap);
      },
      error: err => console.error('Не вдалося оновити список відгуків', err)
    });
  }

  /** Запис: без логіну → /login; з логіном → сторінка записів відкриє модалку з цією послугою (query `bookService`). */
  bookAppointment(service: MedicalService): void {
    if (!this.auth.isLoggedIn()) {
      void this.router.navigate(['/login']);
      return;
    }
    void this.router.navigate(['/user/appointments'], {
      queryParams: { bookService: service.id }
    });
  }
}
