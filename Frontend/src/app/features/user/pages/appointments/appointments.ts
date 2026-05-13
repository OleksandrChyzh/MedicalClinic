import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AppointmentService } from '../../../../core/services/appointment.service';
import { GetAppointmentDTO } from '../../../../models/appointment.model';

@Component({
  selector: 'appointments',
  standalone: true,
  // Додаємо CommonModule, щоб працювали [ngClass] та @if/@for
  imports: [CommonModule, DatePipe],
  templateUrl: './appointments.html',
  styleUrl: './appointments.scss'
})
export class AppointmentsComponent implements OnInit {
  private appointmentService = inject(AppointmentService);

  // Переконайся, що ці сигнали PUBLIC (за замовчуванням вони такі, якщо не писати private)
  public appointments = signal<GetAppointmentDTO[]>([]);
  public isLoading = signal<boolean>(false);

  ngOnInit(): void {
    this.loadAppointments();
  }

  public loadAppointments(): void {
    this.isLoading.set(true);
    this.appointmentService.getMyAppointments().subscribe({
      next: (data) => {
        this.appointments.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Помилка завантаження:', err);
        this.isLoading.set(false);
      }
    });
  }

  // Метод, який IDE "не бачив"
  public getStatusClass(status: string): string {
    if (!status) return 'status-default';

    switch (status.toLowerCase()) {
      case 'confirmed':
      case 'підтверджено':
        return 'status-confirmed';
      case 'pending':
      case 'очікується':
        return 'status-pending';
      case 'cancelled':
      case 'скасовано':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }
}
