import { Component, OnInit, signal, inject, DestroyRef, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppointmentService } from '../../../../core/services/appointment.service';
import { DoctorService } from '../../../../core/services/doctor.service';
import { ClinicService } from '../../../../core/services/clinic.service';
import { UserService } from '../../../../core/services/user.service';
import { PatientService } from '../../../../core/services/patient.service';
import {
  GetAppointmentDTO,
  AddAppointmentDTO,
  FreeSlotDTO
} from '../../../../models/appointment.model';
import { Doctor } from '../../../../models/doctor.model';
import { Direction, MedicalService, ServiceType } from '../../../../models/service.models';
import { Patient } from '../../../../models/patient.model';
import { UserProfile } from '../../../../models/user.model';

/** Дані після «Записатися» на сторінці «Послуги» */
interface BookingPrefill {
  directionId: number;
  typeId: number | null;
  serviceId: number;
}

/** Дані після «Записатися» на сторінці «Лікарі» */
interface DoctorBookingPrefill {
  directionId: number;
  doctorId: number;
}

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, DatePipe, ReactiveFormsModule],
  templateUrl: './appointments.html',
  styleUrl: './appointments.scss'
})
export class AppointmentsComponent implements OnInit {
  // Ін'єкція сервісів
  private appointmentService = inject(AppointmentService);
  private doctorService = inject(DoctorService);
  private clinicService = inject(ClinicService);
  private userService = inject(UserService);
  private patientService = inject(PatientService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // Сигнали списку записів на прийом
  public appointments = signal<GetAppointmentDTO[]>([]);
  public isLoading = signal<boolean>(false);

  /** Сортування за датою прийому: asc — від ранішої до пізнішої, desc — навпаки */
  readonly dateSortOrder = signal<'asc' | 'desc'>('asc');
  /** Порожній рядок — усі статуси (без урахування регістру) */
  readonly statusFilter = signal<string>('');

  readonly uniqueStatuses = computed(() => {
    const seen = new Set<string>();
    const out: string[] = [];
    for (const a of this.appointments()) {
      const s = (a.status || '').trim();
      if (!s) continue;
      const key = s.toLowerCase();
      if (seen.has(key)) continue;
      seen.add(key);
      out.push(s);
    }
    return out.sort((a, b) => a.localeCompare(b, 'uk'));
  });

  readonly filteredAppointments = computed(() => {
    const all = this.appointments();
    const statusKey = this.statusFilter().trim().toLowerCase();
    const filtered = statusKey
      ? all.filter(a => (a.status || '').trim().toLowerCase() === statusKey)
      : [...all];

    const dir = this.dateSortOrder();
    filtered.sort((a, b) => {
      const ta = new Date(a.appointmentDate).getTime();
      const tb = new Date(b.appointmentDate).getTime();
      if (ta !== tb) {
        return dir === 'asc' ? ta - tb : tb - ta;
      }
      return b.id - a.id;
    });
    return filtered;
  });

  // Сигнали для керування станом модального вікна
  public isModalOpen = signal<boolean>(false);
  public isSubmitting = signal<boolean>(false);
  public isLoadingSlots = signal<boolean>(false);
  public appointmentForm!: FormGroup;

  // Поточний залогінений користувач
  public currentUser = signal<UserProfile | null>(null);

  // Сигнали для збереження списків з бекенду
  public patients = signal<Patient[]>([]);
  public directions = signal<Direction[]>([]);
  public serviceTypes = signal<ServiceType[]>([]);
  public filteredServices = signal<MedicalService[]>([]);
  public filteredDoctors = signal<Doctor[]>([]);
  public availableSlots = signal<FreeSlotDTO[]>([]);
  public treatmentBookingMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadAppointments();
    this.initForm();
    this.loadModalStaticData();
    this.loadUserProfile();

    // Перехід з «Послуги» (?bookService=) або «Лікарі» (?bookDoctor=)
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const doctorRaw = params.get('bookDoctor');
      if (doctorRaw) {
        const doctorId = +doctorRaw;
        if (!Number.isFinite(doctorId)) {
          this.clearBookingIntentQuery();
          return;
        }
        this.openBookingFlowFromDoctorId(doctorId);
        return;
      }

      const raw = params.get('bookService');
      if (!raw) {
        return;
      }
      const serviceId = +raw;
      if (!Number.isFinite(serviceId)) {
        this.clearBookingIntentQuery();
        return;
      }
      this.openBookingFlowFromServiceId(serviceId);
    });
  }

  private openBookingFlowFromDoctorId(doctorId: number): void {
    this.doctorService.getDoctorById(doctorId).subscribe({
      next: (d) => {
        if (d.directionId == null) {
          console.error('API не повернув directionId для лікаря', doctorId);
          this.clearBookingIntentQuery();
          return;
        }
        this.openModalWithDoctorPrefill({
          directionId: d.directionId,
          doctorId: d.id
        });
        this.clearBookingIntentQuery();
      },
      error: (err) => {
        console.error('Не вдалося завантажити лікаря для запису:', err);
        this.clearBookingIntentQuery();
      }
    });
  }

  /** Завантажує послугу з API, відкриває модалку з напрямком/типом/послугою, прибирає query з адресного рядка. */
  private openBookingFlowFromServiceId(serviceId: number): void {
    this.clinicService.getServiceById(serviceId).subscribe({
      next: (s) => {
        if (s.directionId == null) {
          console.error('API не повернув directionId для послуги', serviceId);
          this.clearBookingIntentQuery();
          return;
        }
        this.openModalWithPrefill({
          directionId: s.directionId,
          typeId: s.typeId ?? null,
          serviceId: s.id
        });
        this.clearBookingIntentQuery();
      },
      error: (err) => {
        console.error('Не вдалося завантажити послугу для запису:', err);
        this.clearBookingIntentQuery();
      }
    });
  }

  private clearBookingIntentQuery(): void {
    void this.router.navigate(['/user/appointments'], {
      queryParams: { bookService: null, bookDoctor: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  // Ініціалізація форми з урахуванням реактивних правил
  private initForm(): void {
    this.appointmentForm = this.fb.group({
      patientId: ['', Validators.required],
      directionId: ['', Validators.required],
      serviceTypeId: [''],
      serviceId: [{ value: '', disabled: true }, Validators.required],
      doctorId: [{ value: '', disabled: true }, Validators.required],
      date: [{ value: '', disabled: true }, Validators.required],
      timeSlot: [{ value: '', disabled: true }, Validators.required]
    });

    // Слідкуємо за зміною Напрямку (Медицини)
    this.appointmentForm.get('directionId')?.valueChanges.subscribe(directionId => {
      this.onDirectionChange(directionId);
    });

    // Слідкуємо за зміною Типу Послуги (Консультація, Аналізи і т.д.)
    this.appointmentForm.get('serviceTypeId')?.valueChanges.subscribe(typeId => {
      const directionId = this.appointmentForm.get('directionId')?.value;
      this.validateTreatmentSelection();
      this.loadServices(directionId, typeId);
    });

    this.appointmentForm.get('serviceId')?.valueChanges.subscribe(() => {
      this.validateTreatmentSelection();
    });

    // Слідкуємо за зміною Лікаря — Активуємо/деактивуємо дату та завантажуємо слоти
    this.appointmentForm.get('doctorId')?.valueChanges.subscribe(doctorId => {
      const dateCtrl = this.appointmentForm.get('date');
      if (doctorId) {
        dateCtrl?.enable({ emitEvent: false }); // Розблоковуємо вибір дати, бо лікаря вже обрано!
      } else {
        dateCtrl?.disable({ emitEvent: false });
      }
      this.loadSlots();
    });

    // Слідкуємо за зміною Дати
    this.appointmentForm.get('date')?.valueChanges.subscribe(() => {
      this.loadSlots();
    });
  }

  // Метод отримання даних профілю користувача
  private loadUserProfile(): void {
    this.userService.getProfile().subscribe({
      next: (profile) => this.currentUser.set(profile),
      error: (err) => console.error('Помилка отримання профілю користувача:', err)
    });
  }

  // Метод повернення твоїх поточних замовлень
  public loadAppointments(): void {
    this.isLoading.set(true);
    this.appointmentService.getMyAppointments().subscribe({
      next: (data) => {
        this.appointments.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження записів:', err);
        this.isLoading.set(false);
      }
    });
  }

  onDateSortChange(ev: Event): void {
    const v = (ev.target as HTMLSelectElement).value as 'asc' | 'desc';
    this.dateSortOrder.set(v);
  }

  onStatusFilterChange(ev: Event): void {
    this.statusFilter.set((ev.target as HTMLSelectElement).value);
  }

  resetListFilters(): void {
    this.dateSortOrder.set('asc');
    this.statusFilter.set('');
  }

  public canDeleteAppointment(appointment: GetAppointmentDTO): boolean {
    const status = (appointment.status || '').toUpperCase();
    return status !== 'COMPLETED';
  }

  public deleteAppointment(appointment: GetAppointmentDTO): void {
    if (!this.canDeleteAppointment(appointment)) return;

    const confirmed = confirm('Ви дійсно хочете видалити цей запис?');
    if (!confirmed) return;

    this.appointmentService.deleteAppointment(appointment.id).subscribe({
      next: () => this.loadAppointments(),
      error: (err) => {
        console.error('Помилка видалення запису:', err);
        alert('Не вдалося видалити запис. Спробуйте ще раз.');
      }
    });
  }

  // Попереднє завантаження статичних списків та пацієнтів для модалки
  private loadModalStaticData(): void {
    this.clinicService.getDirections().subscribe(data => this.directions.set(data));
    this.clinicService.getServiceTypes().subscribe(data => this.serviceTypes.set(data));

    // Завантажуємо пацієнтів поточного користувача через сервіс пацієнтів
    this.patientService.getMyPatients().subscribe({
      next: (data) => this.patients.set(data),
      error: (err) => console.error('Помилка завантаження пацієнтів:', err)
    });
  }

  // Логіка зміни напрямку (Дерматологія, Кардіологія тощо)
  private onDirectionChange(directionId: any): void {
    const serviceCtrl = this.appointmentForm.get('serviceId');
    const doctorCtrl = this.appointmentForm.get('doctorId');
    const typeId = this.appointmentForm.get('serviceTypeId')?.value;

    this.treatmentBookingMessage.set(null);
    this.resetFromDate();

    if (!directionId) {
      serviceCtrl?.disable({ emitEvent: false });
      doctorCtrl?.disable({ emitEvent: false });
      this.filteredServices.set([]);
      this.filteredDoctors.set([]);
      return;
    }

    // 1. Завантажуємо послуги, що належать ТІЛЬКИ цьому напрямку
    this.loadServices(directionId, typeId);
    serviceCtrl?.enable({ emitEvent: false });
    serviceCtrl?.setValue('', { emitEvent: false });

    // 2. Завантажуємо лікарів, що належать ТІЛЬКИ цьому напрямку
    this.doctorService.getAllDoctors(+directionId).subscribe({
      next: (doctors) => {
        this.filteredDoctors.set(doctors);
        doctorCtrl?.enable({ emitEvent: false });
        doctorCtrl?.setValue('', { emitEvent: false });
      },
      error: (err) => console.error('Помилка фільтрації лікарів:', err)
    });
  }

  private loadServices(directionId: number | null, typeId: number | null): void {
    if (!directionId) return;
    this.clinicService.getServices(directionId, typeId).subscribe({
      next: (services) => {
        this.filteredServices.set(services);
        this.validateTreatmentSelection();
      },
      error: (err) => console.error('Помилка фільтрації послуг:', err)
    });
  }

  public isTreatmentBookingBlocked(): boolean {
    return this.treatmentBookingMessage() !== null;
  }

  private validateTreatmentSelection(): void {
    const typeId = this.appointmentForm.get('serviceTypeId')?.value;
    const serviceId = this.appointmentForm.get('serviceId')?.value;
    const selectedType = typeId ? this.serviceTypes().find(type => type.id === +typeId) : null;
    const selectedService = serviceId ? this.filteredServices().find(service => service.id === +serviceId) : null;
    const serviceType = selectedService?.typeId
      ? this.serviceTypes().find(type => type.id === selectedService.typeId)
      : null;
    const isTreatment = this.isTreatmentServiceType(selectedType?.name) || this.isTreatmentServiceType(serviceType?.name);

    if (!isTreatment) {
      this.treatmentBookingMessage.set(null);
      return;
    }

    this.treatmentBookingMessage.set('Запис на лікування створює лікар після консультації або діагностики. Будь ласка, спочатку запишіться на консультацію чи діагностику.');
    this.appointmentForm.get('serviceId')?.setValue('', { emitEvent: false });
    this.appointmentForm.get('doctorId')?.setValue('', { emitEvent: false });
    this.resetFromDate();
  }

  private isTreatmentServiceType(name?: string | null): boolean {
    if (!name) return false;

    const normalized = name.trim().toLowerCase();
    return normalized.includes('лікуван') || normalized.includes('лiкуван') || normalized.includes('treatment');
  }

  // Завантаження вільних часових вікон з бекенду
  private loadSlots(): void {
    const doctorId = this.appointmentForm.get('doctorId')?.value;
    const date = this.appointmentForm.get('date')?.value;
    const timeSlotCtrl = this.appointmentForm.get('timeSlot');

    if (!doctorId || !date) {
      this.availableSlots.set([]);
      timeSlotCtrl?.disable({ emitEvent: false });
      return;
    }

    this.isLoadingSlots.set(true);
    this.appointmentService.getAvailableSlots(+doctorId, date).subscribe({
      next: (response) => {
        this.availableSlots.set(response.freeSlots || []);
        timeSlotCtrl?.enable({ emitEvent: false });
        timeSlotCtrl?.setValue('', { emitEvent: false });
        this.isLoadingSlots.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження слотів:', err);
        this.isLoadingSlots.set(false);
      }
    });
  }

  // Допоміжне тихе скидання полів при зміні батьківських параметрів
  private resetFromDate(): void {
    const dateCtrl = this.appointmentForm.get('date');
    const timeSlotCtrl = this.appointmentForm.get('timeSlot');

    dateCtrl?.setValue('', { emitEvent: false });
    timeSlotCtrl?.setValue('', { emitEvent: false });

    dateCtrl?.disable({ emitEvent: false });
    timeSlotCtrl?.disable({ emitEvent: false });

    this.availableSlots.set([]);
  }

  // Керування вікном
  public openModal(): void {
    this.appointmentForm.reset({
      patientId: '',
      directionId: '',
      serviceTypeId: '',
      serviceId: '',
      doctorId: '',
      date: '',
      timeSlot: ''
    });
    this.treatmentBookingMessage.set(null);
    this.isModalOpen.set(true);
  }

  /** Відкрити модалку запису з уже обраними напрямком, типом (за потреби) та послугою (наприклад, зі сторінки «Послуги»). */
  public openModalWithPrefill(prefill: BookingPrefill): void {
    this.openModal();

    const typeStr = prefill.typeId != null ? String(prefill.typeId) : '';

    this.appointmentForm.patchValue(
      {
        directionId: String(prefill.directionId),
        serviceTypeId: typeStr
      },
      { emitEvent: false }
    );

    const serviceCtrl = this.appointmentForm.get('serviceId');
    const doctorCtrl = this.appointmentForm.get('doctorId');
    this.resetFromDate();

    const dirId = prefill.directionId;
    const typeId = prefill.typeId;

    this.clinicService.getServices(dirId, typeId).subscribe({
      next: (services) => {
        this.filteredServices.set(services);
        serviceCtrl?.enable({ emitEvent: false });
        serviceCtrl?.setValue(String(prefill.serviceId), { emitEvent: false });
        this.validateTreatmentSelection();
      },
      error: (err) => console.error('Помилка завантаження послуг (prefill):', err)
    });

    this.doctorService.getAllDoctors(dirId).subscribe({
      next: (doctors) => {
        this.filteredDoctors.set(doctors);
        doctorCtrl?.enable({ emitEvent: false });
        doctorCtrl?.setValue('', { emitEvent: false });
      },
      error: (err) => console.error('Помилка завантаження лікарів (prefill):', err)
    });
  }

  /** Модалка з напрямком і лікарем (сторінка «Лікарі»); послугу користувач обирає сам — поле лишається порожнім, але доступним. */
  public openModalWithDoctorPrefill(prefill: DoctorBookingPrefill): void {
    this.openModal();

    this.appointmentForm.patchValue(
      {
        directionId: String(prefill.directionId),
        serviceTypeId: ''
      },
      { emitEvent: false }
    );

    const serviceCtrl = this.appointmentForm.get('serviceId');
    const doctorCtrl = this.appointmentForm.get('doctorId');
    this.resetFromDate();

    const dirId = prefill.directionId;

    this.clinicService.getServices(dirId, null).subscribe({
      next: (services) => {
        this.filteredServices.set(services);
        serviceCtrl?.enable({ emitEvent: false });
        serviceCtrl?.setValue('', { emitEvent: false });
      },
      error: (err) => console.error('Помилка завантаження послуг (prefill лікар):', err)
    });

    this.doctorService.getAllDoctors(dirId).subscribe({
      next: (doctors) => {
        this.filteredDoctors.set(doctors);
        doctorCtrl?.enable({ emitEvent: false });
        doctorCtrl?.setValue(String(prefill.doctorId), { emitEvent: true });
      },
      error: (err) => console.error('Помилка завантаження лікарів (prefill лікар):', err)
    });
  }

  public closeModal(): void {
    this.isModalOpen.set(false);
  }

  // Створення нового запису на прийом
  public onCreateAppointment(): void {
    this.validateTreatmentSelection();
    if (this.appointmentForm.invalid || this.isTreatmentBookingBlocked()) return;

    this.isSubmitting.set(true);
    const formValue = this.appointmentForm.value;
    const selectedSlot = this.availableSlots().find(slot => slot.startTime === formValue.timeSlot);
    if (!selectedSlot) {
      this.isSubmitting.set(false);
      return;
    }

    // Склеюємо Дату візиту та StartTime обраного слоту в ISO формат
    const fullIsoDateTime = `${formValue.date}T${selectedSlot.startTime}`;

    // Об'єкт DTO для відправки на бекенд
    const dto: AddAppointmentDTO = {
      patientId: +formValue.patientId,
      userId: this.currentUser()?.id || 0,
      doctorId: +formValue.doctorId,
      serviceId: +formValue.serviceId,
      appointmentDate: fullIsoDateTime,
      durationMinutes: 30
    };

    this.appointmentService.createAppointment(dto).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closeModal();
        this.loadAppointments();
      },
      error: (err) => {
        console.error('Помилка створення запису:', err);
        this.isSubmitting.set(false);
        alert('Не вдалося створити запис. Оберіть інший час.');
      }
    });
  }

  // Динамічний колір статусу
  public getStatusClass(status: string): string {
    if (!status) return 'status-default';
    switch (status.toLowerCase()) {
      case 'confirmed': case 'підтверджено': return 'status-confirmed';
      case 'created': case 'pending': case 'очікується': return 'status-pending';
      case 'cancelled': case 'скасовано': return 'status-cancelled';
      default: return 'status-default';
    }
  }
}
