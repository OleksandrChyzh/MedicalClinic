import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddAppointmentDTO, AvailableSlotsResponseDTO, GetAppointmentDTO } from '../../models/appointment.model';
import { MedicalService } from '../../models/service.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7272/api/Appointment';

  createAppointment(dto: AddAppointmentDTO): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  createAppointmentByDoctor(dto: AddAppointmentDTO): Observable<GetAppointmentDTO> {
    return this.http.post<GetAppointmentDTO>(`${this.apiUrl}/doctor`, dto);
  }

  getAvailableSlots(doctorId: number, date: string): Observable<AvailableSlotsResponseDTO> {
    return this.http.get<AvailableSlotsResponseDTO>(`${this.apiUrl}/doctor/${doctorId}/available-slots`, {
      params: { date }
    });
  }

  getMyAppointments(): Observable<GetAppointmentDTO[]> {
    return this.http.get<GetAppointmentDTO[]>(`${this.apiUrl}/my`);
  }

  getDoctorAppointments(): Observable<GetAppointmentDTO[]> {
    return this.http.get<GetAppointmentDTO[]>(`${this.apiUrl}/doctor-schedule`);
  }

  changeStatus(id: number, status: 'CREATED' | 'CONFIRMED' | 'CANCELLED' | 'COMPLETED'): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, null, {
      params: { status }
    });
  }

  deleteAppointment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // 🔴 ДОБАВИВ ЦЕЙ МЕТОД
  getAllServices(): Observable<MedicalService[]> {
    return this.http.get<MedicalService[]>('https://localhost:7272/api/Services');
  }
}
