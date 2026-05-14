import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Schedule } from '../../models/schedule.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7272/api/schedule';

  getMySchedule(): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/my`);
  }

  getDoctorSchedule(doctorId: number): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/doctor/${doctorId}`);
  }
}
