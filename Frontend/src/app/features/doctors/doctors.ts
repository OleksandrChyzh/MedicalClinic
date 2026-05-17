import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

// Сервіси
import { DoctorService } from '../../core/services/doctor.service';
import { ReviewService } from '../../core/services/review.service';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { ClinicService } from '../../core/services/clinic.service';
import { ScheduleService } from '../../core/services/schedule.service';

// Моделі
import { Doctor } from '../../models/doctor.model';
import { Review } from '../../models/review.models';
import { Direction } from '../../models/service.models';

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
  expandedReviewForms = signal<Set<number>>(new Set());

  // Обчислюваний список напрямків для фільтра (беремо унікальні з отриманих лікарів)
  directions = computed(() => {
    const all = this.doctors().map(d => ({ id: d.directionId, name: d.directionName }));
    return Array.from(new Map(all.map(item => [item.id, item])).values());
  });

  ngOnInit(): void {
    this.loadDoctors();
    if (this.isAdmin()) {
      this.clinicService.getDirections().subscribe(data => this.adminDirections.set(data));
    }
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
      this.closeReviewForm(doctorId);
      return;
    }

    const next = new Set<number>([doctorId]);
    this.expandedDoctors.set(next);
    this.composeRating.set(5);
    this.composeComment.set('');
    this.reviewSubmitError.set(null);
    this.expandedReviewForms.set(new Set());

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

  toggleReviewForm(doctorId: number): void {
    const forms = new Set(this.expandedReviewForms());

    if (forms.has(doctorId)) {
      forms.delete(doctorId);
    } else {
      forms.clear();
      forms.add(doctorId);
      this.composeRating.set(5);
      this.composeComment.set('');
      this.reviewSubmitError.set(null);
    }

    this.expandedReviewForms.set(forms);
  }

  private closeReviewForm(doctorId: number): void {
    const forms = new Set(this.expandedReviewForms());
    forms.delete(doctorId);
    this.expandedReviewForms.set(forms);
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
          this.closeReviewForm(doctorId);
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

  // --- АДМІН-ФУНКЦІЇ ---
  isAdmin = computed(() => this.auth.getUserRoles().includes('Admin'));
  clinicService = inject(ClinicService);
  scheduleService = inject(ScheduleService);
  adminDirections = signal<Direction[]>([]);
  schedule = signal<any[]>([]);
  isFormOpen = signal(false);
  editingDoctorId = signal<number | null>(null);
  errorMessage = signal<string | null>(null);
  readonly weekDays = ['Понеділок', 'Вівторок', 'Середа', 'Четвер', "П'ятниця", 'Субота', 'Неділя'];
  form: any = this.createEmptyForm();
  activeMenu = signal<number | null>(null);

  startCreate(): void {
    this.editingDoctorId.set(null);
    this.form = this.createEmptyForm();
    this.schedule.set(this.defaultSchedule());
    this.errorMessage.set(null);
    this.isFormOpen.set(true);
  }

  toggleMenu(id: number): void {
    this.activeMenu.set(this.activeMenu() === id ? null : id);
  }

  closeMenu(): void {
    this.activeMenu.set(null);
  }

  getMenuPosition(id: number): { top: string; right: string } {
    const button = document.querySelector(`[data-menu-id="${id}"]`) as HTMLElement;
    if (!button) return { top: '0', right: '0' };
    const rect = button.getBoundingClientRect();
    return {
      top: `${rect.bottom + 4}px`,
      right: `${window.innerWidth - rect.right}px`
    };
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
    this.errorMessage.set(null);
    this.isFormOpen.set(true);
  }

  saveDoctor(): void {
    this.errorMessage.set(null);
    const id = this.editingDoctorId();
    if (id) {
      const dto = { ...this.form, id };
      this.doctorService.updateDoctor(id, dto).pipe(switchMap(() => this.saveSchedule(id))).subscribe({
        next: () => { this.cancelForm(); this.loadDoctors(); },
        error: (err) => this.handleError(err)
      });
      return;
    }

    this.doctorService.createDoctor(this.form).pipe(
      switchMap(created => this.saveSchedule(created.id))
    ).subscribe({
      next: () => { this.cancelForm(); this.loadDoctors(); },
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
    this.doctorService.deleteDoctor(id).subscribe(() => this.loadDoctors());
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
