import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'; // Додано імпорти для форм
import { UserService } from '../../../../core/services/user.service';
import { UserProfile } from '../../../../models/user.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Обов'язково додаємо ReactiveFormsModule
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class ProfileComponent implements OnInit {
  private userService = inject(UserService);
  private fb = inject(FormBuilder); // Інжектимо FormBuilder для створення форми

  // Сигнали для зберігання стану
  profile = signal<UserProfile | null>(null);
  isLoading = signal(true);

  // Нові сигнали для режиму редагування
  isEditing = signal(false);
  isSaving = signal(false);

  editForm!: FormGroup;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile() {
    this.userService.getProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження профілю', err);
        this.isLoading.set(false);
      }
    });
  }

  // Вмикаємо режим редагування та заповнюємо форму поточними даними
  startEditing() {
    const currentData = this.profile();
    if (currentData) {
      this.editForm = this.fb.group({
        userName: [currentData.userName, [Validators.required, Validators.minLength(2)]],
        phoneNumber: [currentData.phoneNumber || ''],
        password: [''] // Пароль залишаємо пустим, щоб не перезаписати випадково
      });
      this.isEditing.set(true);
    }
  }

  // Скасовуємо редагування
  cancelEditing() {
    this.isEditing.set(false);
  }

  // Зберігаємо нові дані
  saveProfile() {
    if (this.editForm.invalid) return;

    this.isSaving.set(true);
    const formValues = this.editForm.value;

    // Відправляємо пароль на бекенд тільки якщо користувач його ввів
    const updateData = {
      userName: formValues.userName,
      phoneNumber: formValues.phoneNumber,
      password: formValues.password ? formValues.password : null
    };

    this.userService.updateProfile(updateData).subscribe({
      next: (updatedProfile) => {
        this.profile.set(updatedProfile); // Оновлюємо дані на екрані
        this.isEditing.set(false); // Вимикаємо форму
        this.isSaving.set(false);
      },
      error: (err) => {
        console.error('Помилка оновлення', err);
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
