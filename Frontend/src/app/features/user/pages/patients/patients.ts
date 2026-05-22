import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PatientService } from '../../../../core/services/patient.service';
import { MedicalRecordService } from '../../../../core/services/medical-record.service';
import { Patient } from '../../../../models/patient.model';
import { MedicalCardDTO } from '../../../../models/medical-record.model';

@Component({
  selector: 'patients',
  standalone: true,
  imports: [CommonModule, DatePipe, ReactiveFormsModule],
  templateUrl: './patients.html',
  styleUrl: './patients.scss'
})
export class PatientsComponent implements OnInit {
  private patientService = inject(PatientService);
  private medicalRecordService = inject(MedicalRecordService);
  private fb = inject(FormBuilder);

  // Сигнали стану
  patients = signal<Patient[]>([]);
  isLoading = signal(true);

  // Стан для форми
  isAdding = signal(false);
  isSaving = signal(false);
  addForm!: FormGroup;

  // Медична картка
  isMedCardOpen = signal(false);
  isLoadingCard = signal(false);
  medCard = signal<MedicalCardDTO | null>(null);
  medCardError = signal<string | null>(null);

  ngOnInit(): void {
    this.loadPatients();
    this.initForm();
  }

  initForm() {
    this.addForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      birthDate: ['', Validators.required],
      gender: ['', Validators.required]
    });
  }

  loadPatients() {
    this.isLoading.set(true);
    this.patientService.getMyPatients().subscribe({
      next: (data) => {
        this.patients.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження пацієнтів', err);
        this.isLoading.set(false);
      }
    });
  }

  toggleAddForm() {
    this.isAdding.set(!this.isAdding());
    if (!this.isAdding()) {
      this.addForm.reset();
    }
  }

  onSubmit() {
    if (this.addForm.invalid) return;

    this.isSaving.set(true);

    const newPatientData = {
      ...this.addForm.value,
      userId: 0 // Заглушка, щоб пройти валідацію DTO. Бекенд сам підставить правильний ID.
    };

    this.patientService.createPatient(newPatientData).subscribe({
      next: (createdPatient) => {
        // Додаємо нового пацієнта до поточного списку без перезавантаження сторінки
        this.patients.update(current => [...current, createdPatient]);
        this.isSaving.set(false);
        this.toggleAddForm();
      },
      error: (err) => {
        console.error('Помилка створення пацієнта', err);
        this.isSaving.set(false);
      }
    });
  }

  deletePatient(id: number) {
    if (confirm('Ви впевнені, що хочете видалити цього пацієнта? Цю дію неможливо скасувати.')) {
      this.patientService.deletePatient(id).subscribe({
        next: () => {
          this.patients.update(current => current.filter(p => p.id !== id));
        },
        error: (err) => console.error('Помилка видалення', err)
      });
    }
  }

  openMedicalCard(patientId: number): void {
    this.medCard.set(null);
    this.medCardError.set(null);
    this.isLoadingCard.set(true);
    this.isMedCardOpen.set(true);

    this.medicalRecordService.getMedicalCard(patientId).subscribe({
      next: card => {
        this.medCard.set(card);
        this.isLoadingCard.set(false);
      },
      error: err => {
        console.error('Помилка завантаження медичної картки', err);
        this.medCardError.set('Не вдалося завантажити медичну картку.');
        this.isLoadingCard.set(false);
      }
    });
  }

  closeMedicalCard(): void {
    this.isMedCardOpen.set(false);
    this.medCard.set(null);
  }
}
