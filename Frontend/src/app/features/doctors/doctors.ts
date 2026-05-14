import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs/operators';

// Сервіси
import { DoctorService } from '../../core/services/doctor.service';
import { ReviewService } from '../../core/services/review.service';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';

// Моделі
import { Doctor } from '../../models/doctor.model';
import { Review } from '../../models/review.models';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctors.html',
  styleUrl: './doctors.scss'
})
export class DoctorsComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private reviewService = inject(ReviewService);
  private userService = inject(UserService);
  private router = inject(Router);
  readonly auth = inject(AuthService);

  /** Вибір зірок у формі нового відгуку */
  readonly ratingStars = [1, 2, 3, 4, 5] as const;
  composeRating = signal<number>(5);
  composeComment = signal<string>('');
  isSubmittingReview = signal(false);
  reviewSubmitError = signal<string | null>(null);

  // --- СТАН ДЛЯ ЛІКАРІВ ---
  doctors = signal<Doctor[]>([]);
  isLoading = signal(true);

  // Фільтрація
  selectedDirectionId = signal<number | null>(null);

  // --- СТАН ДЛЯ ВІДГУКІВ ---
  expandedDoctors = signal<Set<number>>(new Set());
  doctorReviews = signal<Map<number, Review[]>>(new Map());
  isLoadingReviews = signal<Set<number>>(new Set());

  // Обчислюваний список напрямків для фільтра (беремо унікальні з отриманих лікарів)
  directions = computed(() => {
    const all = this.doctors().map(d => ({ id: d.directionId, name: d.directionName }));
    return Array.from(new Map(all.map(item => [item.id, item])).values());
  });

  ngOnInit(): void {
    this.loadDoctors();
  }

  // Завантаження основного списку лікарів
  loadDoctors(): void {
    this.isLoading.set(true);
    this.doctorService.getAllDoctors(this.selectedDirectionId() || undefined).subscribe({
      next: (data) => {
        this.doctors.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження лікарів', err);
        this.isLoading.set(false);
      }
    });
  }

  // Обробка фільтрації
  onFilterChange(): void {
    this.loadDoctors();
  }

  resetFilter(): void {
    this.selectedDirectionId.set(null);
    this.loadDoctors();
  }

  // --- ЛОГІКА ВІДГУКІВ ---

  toggleReviews(doctorId: number): void {
    const currentExpanded = new Set(this.expandedDoctors());

    if (currentExpanded.has(doctorId)) {
      currentExpanded.delete(doctorId);
      this.expandedDoctors.set(currentExpanded);
      return;
    }

    const next = new Set<number>([doctorId]);
    this.expandedDoctors.set(next);
    this.composeRating.set(5);
    this.composeComment.set('');
    this.reviewSubmitError.set(null);

    const currentReviewsMap = this.doctorReviews();
    if (!currentReviewsMap.has(doctorId)) {
      this.setLoadingState(doctorId, true);

      this.reviewService.getReviewsByDoctor(doctorId).subscribe({
        next: (reviews) => {
          const newMap = new Map(this.doctorReviews());
          newMap.set(doctorId, reviews);
          this.doctorReviews.set(newMap);
          this.setLoadingState(doctorId, false);
        },
        error: (err) => {
          console.error('Помилка завантаження відгуків лікаря', err);
          this.setLoadingState(doctorId, false);
        }
      });
    }
  }

  private setLoadingState(doctorId: number, isLoading: boolean): void {
    const loadingSet = new Set(this.isLoadingReviews());
    if (isLoading) {
      loadingSet.add(doctorId);
    } else {
      loadingSet.delete(doctorId);
    }
    this.isLoadingReviews.set(loadingSet);
  }

  // Допоміжний метод для зірочок
  getStars(rating: number): string {
    // Можна використовувати '⭐'.repeat(rating) або іконки
    return '★'.repeat(rating) + '☆'.repeat(5 - rating);
  }

  submitDoctorReview(doctorId: number): void {
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
            doctorId,
            serviceId: null,
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
          this.reloadDoctorReviews(doctorId);
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

  private reloadDoctorReviews(doctorId: number): void {
    this.reviewService.getReviewsByDoctor(doctorId).subscribe({
      next: reviews => {
        const newMap = new Map(this.doctorReviews());
        newMap.set(doctorId, reviews);
        this.doctorReviews.set(newMap);
      },
      error: err => console.error('Не вдалося оновити список відгуків', err)
    });
  }

  /** Без авторизації → логін; з авторизацією → форма нового запису з напрямком і лікарем (як на сторінці послуг). */
  bookWithDoctor(doc: Doctor): void {
    if (!this.auth.isLoggedIn()) {
      void this.router.navigate(['/login']);
      return;
    }
    void this.router.navigate(['/user/appointments'], {
      queryParams: { bookDoctor: doc.id }
    });
  }
}
