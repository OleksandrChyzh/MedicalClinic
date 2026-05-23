import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppointmentService } from '../../../../core/services/appointment.service';
import { ClinicService } from '../../../../core/services/clinic.service';
import { DoctorService } from '../../../../core/services/doctor.service';
import { MedicalRecordService } from '../../../../core/services/medical-record.service';
import { AddAppointmentDTO, FreeSlotDTO, GetAppointmentDTO } from '../../../../models/appointment.model';
import { Doctor } from '../../../../models/doctor.model';
import { Direction, MedicalService, ServiceType } from '../../../../models/service.models';
import { AddMedicalRecordDTO, MedicalCardDTO } from '../../../../models/medical-record.model';

type AppointmentStatus = 'CREATED' | 'CONFIRMED' | 'CANCELLED' | 'COMPLETED';

interface DoctorPatientOption {
  patientId: number;
  userId: number;
  patientFullName: string;
  userName: string;
}

@Component({
  selector: 'app-doctor-appointments',
  standalone: true,
  imports: [CommonModule, DatePipe, ReactiveFormsModule],
  templateUrl: './doctor-appointments.html',
  styleUrl: './doctor-appointments.scss'
})
export class DoctorAppointmentsComponent implements OnInit {
  private appointmentService = inject(AppointmentService);
  private clinicService = inject(ClinicService);
  private doctorService = inject(DoctorService);
  private medicalRecordService = inject(MedicalRecordService);
  private fb = inject(FormBuilder);

  appointments = signal<GetAppointmentDTO[]>([]);
  directions = signal<Direction[]>([]);
  serviceTypes = signal<ServiceType[]>([]);
  filteredServices = signal<MedicalService[]>([]);
  filteredDoctors = signal<Doctor[]>([]);
  availableSlots = signal<FreeSlotDTO[]>([]);

  isLoading = signal(false);
  isSubmitting = signal(false);
  isLoadingSlots = signal(false);
  isModalOpen = signal(false);
  errorMessage = signal<string | null>(null);
  selectedDate = signal<string>('');
  statusFilter = signal<string>('');

  isSubmittingRecord = signal(false);
  medRecordError = signal<string | null>(null);
  medRecordSuccess = signal<string | null>(null);
  selectedAppForRecord = signal<GetAppointmentDTO | null>(null);
  currentDoctorId = signal<number | null>(null);
  medRecordForm!: FormGroup;

  isMedCardOpen = signal(false);
  isLoadingMedCard = signal(false);
  medCard = signal<MedicalCardDTO | null>(null);
  medCardError = signal<string | null>(null);
  isAddRecordFormVisible = signal(false);

  // Обмеження дати для input type="date"
  readonly minDate = computed(() => {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  });

  readonly maxDate = computed(() => {
    const max = new Date();
    max.setMonth(max.getMonth() + 1);
    const year = max.getFullYear();
    const month = String(max.getMonth() + 1).padStart(2, '0');
    const day = String(max.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  });

  appointmentForm!: FormGroup;

  patientOptions = computed<DoctorPatientOption[]>(() => {
    const map = new Map<number, DoctorPatientOption>();

    for (const appointment of this.appointments()) {
      if (!map.has(appointment.patientId)) {
        map.set(appointment.patientId, {
          patientId: appointment.patientId,
          userId: appointment.userId,
          patientFullName: appointment.patientFullName,
          userName: appointment.userName
        });
      }
    }

    return [...map.values()].sort((a, b) => a.patientFullName.localeCompare(b.patientFullName, 'uk'));
  });

  uniqueDates = computed(() => {
    const dates = new Set(this.appointments().map(app => this.toDateInputValue(app.appointmentDate)));
    return [...dates].sort();
  });

  filteredAppointments = computed(() => {
    const date = this.selectedDate();
    const status = this.statusFilter();

    return this.appointments()
      .filter(app => !date || this.toDateInputValue(app.appointmentDate) === date)
      .filter(app => !status || app.status === status)
      .sort((a, b) => new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime());
  });

  ngOnInit(): void {
    this.initForm();
    this.initMedRecordForm();
    this.loadAppointments();
    this.loadModalStaticData();
    this.loadDoctorProfile();
  }

  loadAppointments(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.appointmentService.getDoctorAppointments().subscribe({
      next: data => {
        this.appointments.set(data);
        this.isLoading.set(false);
      },
      error: err => {
        console.error('Помилка завантаження записів лікаря', err);
        this.errorMessage.set('Не вдалося завантажити записи.');
        this.isLoading.set(false);
      }
    });
  }

  loadModalStaticData(): void {
    this.clinicService.getDirections().subscribe({
      next: directions => this.directions.set(directions),
      error: err => console.error('Помилка завантаження напрямків', err)
    });

    this.clinicService.getServiceTypes().subscribe({
      next: types => this.serviceTypes.set(types),
      error: err => console.error('Помилка завантаження типів послуг', err)
    });
  }

  changeStatus(appointment: GetAppointmentDTO, status: AppointmentStatus): void {
    this.appointmentService.changeStatus(appointment.id, status).subscribe({
      next: () => this.loadAppointments(),
      error: err => {
        console.error('Помилка зміни статусу запису', err);
        alert('Не вдалося змінити статус запису.');
      }
    });
  }

  canComplete(appointment: GetAppointmentDTO): boolean {
    const endTime = new Date(appointment.appointmentDate).getTime() + appointment.durationMinutes * 60_000;
    return Date.now() >= endTime && appointment.status === 'CONFIRMED';
  }

  openNextAppointmentModal(source?: GetAppointmentDTO): void {
    this.appointmentForm.reset({
      patientId: source?.patientId ? String(source.patientId) : '',
      directionId: '',
      serviceTypeId: '',
      serviceId: '',
      doctorId: '',
      date: '',
      timeSlot: '',
      durationMinutes: 30
    });
    this.availableSlots.set([]);
    this.filteredServices.set([]);
    this.filteredDoctors.set([]);
    this.appointmentForm.get('serviceId')?.disable({ emitEvent: false });
    this.appointmentForm.get('doctorId')?.disable({ emitEvent: false });
    this.appointmentForm.get('date')?.disable({ emitEvent: false });
    this.appointmentForm.get('timeSlot')?.disable({ emitEvent: false });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  onDateFilterChange(event: Event): void {
    this.selectedDate.set((event.target as HTMLInputElement).value);
  }

  onStatusFilterChange(event: Event): void {
    this.statusFilter.set((event.target as HTMLSelectElement).value);
  }

  resetFilters(): void {
    this.selectedDate.set('');
    this.statusFilter.set('');
  }

  loadSlots(): void {
    const doctorId = this.appointmentForm.get('doctorId')?.value;
    const date = this.appointmentForm.get('date')?.value;
    const durationCtrl = this.appointmentForm.get('durationMinutes');
    const timeSlotCtrl = this.appointmentForm.get('timeSlot');

    if (!date || !doctorId) {
      this.availableSlots.set([]);
      timeSlotCtrl?.setValue('', { emitEvent: false });
      timeSlotCtrl?.disable({ emitEvent: false });
      return;
    }

    this.isLoadingSlots.set(true);
    this.appointmentService.getAvailableSlots(+doctorId, date).subscribe({
      next: response => {
        this.availableSlots.set(response.freeSlots || []);
        timeSlotCtrl?.enable({ emitEvent: false });
        timeSlotCtrl?.setValue('', { emitEvent: false });
        durationCtrl?.setValue(30);
        this.isLoadingSlots.set(false);
      },
      error: err => {
        console.error('Помилка завантаження слотів', err);
        this.availableSlots.set([]);
        this.isLoadingSlots.set(false);
      }
    });
  }

  createNextAppointment(): void {
    if (this.appointmentForm.invalid) return;

    const formValue = this.appointmentForm.value;
    const selectedPatient = this.patientOptions().find(patient => patient.patientId === +formValue.patientId);
    const selectedSlot = this.availableSlots().find(slot => slot.startTime === formValue.timeSlot);

    if (!selectedPatient || !selectedSlot) return;

    const dto: AddAppointmentDTO = {
      patientId: selectedPatient.patientId,
      userId: selectedPatient.userId,
      doctorId: +formValue.doctorId,
      serviceId: +formValue.serviceId,
      appointmentDate: `${formValue.date}T${selectedSlot.startTime}`,
      durationMinutes: +formValue.durationMinutes || 30
    };

    this.isSubmitting.set(true);
    this.appointmentService.createAppointmentByDoctor(dto).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closeModal();
        this.loadAppointments();
      },
      error: err => {
        console.error('Помилка створення наступного запису', err);
        this.isSubmitting.set(false);
        alert('Не вдалося створити наступний запис. Перевірте дату та час.');
      }
    });
  }

  getStatusClass(status: string): string {
    switch ((status || '').toUpperCase()) {
      case 'CREATED': return 'status-created';
      case 'CONFIRMED': return 'status-confirmed';
      case 'CANCELLED': return 'status-cancelled';
      case 'COMPLETED': return 'status-completed';
      default: return 'status-default';
    }
  }

  getStatusLabel(status: string): string {
    switch ((status || '').toUpperCase()) {
      case 'CREATED': return 'Нове замовлення';
      case 'CONFIRMED': return 'Прийнято';
      case 'CANCELLED': return 'Відхилено';
      case 'COMPLETED': return 'Завершено';
      default: return status;
    }
  }

  private loadDoctorProfile(): void {
    this.doctorService.getMyProfile().subscribe({
      next: profile => this.currentDoctorId.set(profile.doctor.id),
      error: err => console.error('Помилка завантаження профілю лікаря', err)
    });
  }

  private initMedRecordForm(): void {
    this.medRecordForm = this.fb.group({
      diagnosis: ['', [Validators.required, Validators.maxLength(500)]],
      result: ['', Validators.maxLength(2000)],
      treatment: ['', Validators.maxLength(2000)],
      recommendations: ['', Validators.maxLength(2000)]
    });
  }


  openMedCard(app: GetAppointmentDTO): void {
    this.medCard.set(null);
    this.medCardError.set(null);
    this.isLoadingMedCard.set(true);
    this.isMedCardOpen.set(true);
    this.isAddRecordFormVisible.set(false);
    this.medRecordSuccess.set(null);

    if (app.status === 'COMPLETED') {
      this.selectedAppForRecord.set(app);
      this.medRecordForm.reset();
      this.medRecordError.set(null);
    } else {
      this.selectedAppForRecord.set(null);
    }

    this.medicalRecordService.getMedicalCard(app.patientId).subscribe({
      next: card => {
        this.medCard.set(card);
        this.isLoadingMedCard.set(false);
      },
      error: err => {
        console.error('Помилка завантаження медичної картки', err);
        this.medCardError.set('Не вдалося завантажити медичну картку.');
        this.isLoadingMedCard.set(false);
      }
    });
  }

  closeMedCard(): void {
    this.isMedCardOpen.set(false);
    this.medCard.set(null);
    this.selectedAppForRecord.set(null);
    this.isAddRecordFormVisible.set(false);
    this.medRecordSuccess.set(null);
  }

  toggleAddRecordForm(): void {
    this.isAddRecordFormVisible.update(v => !v);
    if (!this.isAddRecordFormVisible()) {
      this.medRecordForm.reset();
      this.medRecordError.set(null);
    }
  }

  submitMedicalRecord(): void {
    if (this.medRecordForm.invalid) return;
    const app = this.selectedAppForRecord();
    const doctorId = this.currentDoctorId();
    if (!app || !doctorId) return;

    const dto: AddMedicalRecordDTO = {
      patientId: app.patientId,
      doctorId,
      serviceId: app.serviceId ?? null,
      diagnosis: this.medRecordForm.value.diagnosis,
      result: this.medRecordForm.value.result || null,
      treatment: this.medRecordForm.value.treatment || null,
      recommendations: this.medRecordForm.value.recommendations || null
    };

    this.isSubmittingRecord.set(true);
    this.medRecordError.set(null);
    this.medicalRecordService.createRecord(dto).subscribe({
      next: () => {
        this.isSubmittingRecord.set(false);
        this.medRecordSuccess.set('Медичний запис успішно збережено!');
        this.medRecordForm.reset();
        this.isAddRecordFormVisible.set(false);
        this.medicalRecordService.getMedicalCard(app.patientId).subscribe({
          next: updated => this.medCard.set(updated)
        });
        setTimeout(() => this.medRecordSuccess.set(null), 3000);
      },
      error: err => {
        console.error('Помилка збереження медичного запису', err);
        this.medRecordError.set('Не вдалося зберегти запис. Спробуйте ще раз.');
        this.isSubmittingRecord.set(false);
      }
    });
  }

  private initForm(): void {
    this.appointmentForm = this.fb.group({
      patientId: ['', Validators.required],
      directionId: ['', Validators.required],
      serviceTypeId: [''],
      serviceId: [{ value: '', disabled: true }, Validators.required],
      doctorId: [{ value: '', disabled: true }, Validators.required],
      date: [{ value: '', disabled: true }, Validators.required],
      timeSlot: [{ value: '', disabled: true }, Validators.required],
      durationMinutes: [30, [Validators.required, Validators.min(15), Validators.max(480)]]
    });

    this.appointmentForm.get('directionId')?.valueChanges.subscribe(directionId => {
      this.onDirectionChange(directionId);
    });

    this.appointmentForm.get('serviceTypeId')?.valueChanges.subscribe(typeId => {
      const directionId = this.appointmentForm.get('directionId')?.value;
      this.loadServices(directionId, typeId);
    });

    this.appointmentForm.get('doctorId')?.valueChanges.subscribe(doctorId => {
      const dateCtrl = this.appointmentForm.get('date');
      if (doctorId) {
        dateCtrl?.enable({ emitEvent: false });
      } else {
        dateCtrl?.disable({ emitEvent: false });
      }
      this.loadSlots();
    });

    this.appointmentForm.get('date')?.valueChanges.subscribe(() => this.loadSlots());
  }

  private onDirectionChange(directionId: string): void {
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

    this.loadServices(directionId, typeId);
    serviceCtrl?.enable({ emitEvent: false });
    serviceCtrl?.setValue('', { emitEvent: false });

    this.doctorService.getAllDoctors(+directionId).subscribe({
      next: doctors => {
        this.filteredDoctors.set(doctors);
        doctorCtrl?.enable({ emitEvent: false });
        doctorCtrl?.setValue('', { emitEvent: false });
      },
      error: err => console.error('Помилка фільтрації лікарів', err)
    });
  }

  private loadServices(directionId: string | number | null, typeId: string | number | null): void {
    if (!directionId) return;

    this.clinicService.getServices(+directionId, typeId ? +typeId : null).subscribe({
      next: services => this.filteredServices.set(services),
      error: err => console.error('Помилка фільтрації послуг', err)
    });
  }

  private resetFromDate(): void {
    const dateCtrl = this.appointmentForm.get('date');
    const timeSlotCtrl = this.appointmentForm.get('timeSlot');

    dateCtrl?.setValue('', { emitEvent: false });
    timeSlotCtrl?.setValue('', { emitEvent: false });
    dateCtrl?.disable({ emitEvent: false });
    timeSlotCtrl?.disable({ emitEvent: false });
    this.availableSlots.set([]);
  }

  private toDateInputValue(value: string): string {
    const date = new Date(value);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
