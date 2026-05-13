import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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

  // Сигнали списку записів на прийом
  public appointments = signal<GetAppointmentDTO[]>([]);
  public isLoading = signal<boolean>(false);

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

  ngOnInit(): void {
    this.loadAppointments();
    this.initForm();
    this.loadModalStaticData();
    this.loadUserProfile();
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
      this.loadServices(directionId, typeId);
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
      next: (services) => this.filteredServices.set(services),
      error: (err) => console.error('Помилка фільтрації послуг:', err)
    });
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
    this.isModalOpen.set(true);
  }

  public closeModal(): void {
    this.isModalOpen.set(false);
  }

  // Створення нового запису на прийом
  public onCreateAppointment(): void {
    if (this.appointmentForm.invalid) return;

    this.isSubmitting.set(true);
    const formValue = this.appointmentForm.value;
    const selectedSlot: FreeSlotDTO = formValue.timeSlot;

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
      case 'pending': case 'очікується': return 'status-pending';
      case 'cancelled': case 'скасовано': return 'status-cancelled';
      default: return 'status-default';
    }
  }
}
