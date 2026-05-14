import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DoctorService } from '../../../../core/services/doctor.service';
import { UserService } from '../../../../core/services/user.service';
import { DoctorProfile } from '../../../../models/doctor.model';

@Component({
  selector: 'app-doctor-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './doctor-profile.html',
  styleUrl: './doctor-profile.scss'
})
export class DoctorProfileComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private userService = inject(UserService);
  private fb = inject(FormBuilder);

  profile = signal<DoctorProfile | null>(null);
  isLoading = signal(true);
  isEditing = signal(false);
  isSaving = signal(false);
  errorMessage = signal<string | null>(null);

  editForm!: FormGroup;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.doctorService.getMyProfile().subscribe({
      next: data => {
        this.profile.set(data);
        this.isLoading.set(false);
      },
      error: err => {
        console.error('Помилка завантаження профілю лікаря', err);
        this.errorMessage.set('Не вдалося завантажити профіль лікаря.');
        this.isLoading.set(false);
      }
    });
  }

  startEditing(): void {
    const currentData = this.profile();
    if (!currentData) return;

    this.editForm = this.fb.group({
      userName: [currentData.user.userName, [Validators.required, Validators.minLength(2)]],
      phoneNumber: [currentData.user.phoneNumber || ''],
      password: ['']
    });
    this.isEditing.set(true);
  }

  cancelEditing(): void {
    this.isEditing.set(false);
  }

  saveProfile(): void {
    if (this.editForm.invalid) return;

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValues = this.editForm.value;
    const updateData = {
      userName: formValues.userName,
      phoneNumber: formValues.phoneNumber,
      password: formValues.password ? formValues.password : null
    };

    this.userService.updateProfile(updateData).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.loadProfile();
      },
      error: err => {
        console.error('Помилка оновлення профілю лікаря', err);
        this.errorMessage.set('Не вдалося зберегти зміни профілю.');
        this.isSaving.set(false);
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }
}
