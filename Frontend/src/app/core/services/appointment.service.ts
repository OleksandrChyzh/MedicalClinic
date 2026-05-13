import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddAppointmentDTO, AvailableSlotsResponseDTO, GetAppointmentDTO } from '../../models/appointment.model';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private http = inject(HttpClient);
  // Оскільки environments немає, використовуємо пряму адресу (за потреби зміни порт)
  private apiUrl = 'https://localhost:7272/api/Appointment';

  createAppointment(dto: AddAppointmentDTO): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  getAvailableSlots(doctorId: number, date: string): Observable<AvailableSlotsResponseDTO> {
    return this.http.get<AvailableSlotsResponseDTO>(`${this.apiUrl}/doctor/${doctorId}/available-slots`, {
      params: { date }
    });
  }

  getMyAppointments(): Observable<GetAppointmentDTO[]> {
    return this.http.get<GetAppointmentDTO[]>(`${this.apiUrl}/my`);
  }
}
