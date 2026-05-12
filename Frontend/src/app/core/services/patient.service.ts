import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Patient, AddPatientDto } from '../../models/patient.model';

@Injectable({
  providedIn: 'root'
})
export class PatientService {
  private http = inject(HttpClient);

  // Використовуємо той самий порт, що і для профілю
  private apiUrl = 'https://localhost:7272/api/patient';

  // Отримати список пацієнтів поточного юзера
  getMyPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(`${this.apiUrl}/my`);
  }

  // Створити нового пацієнта
  createPatient(data: AddPatientDto): Observable<Patient> {
    return this.http.post<Patient>(this.apiUrl, data);
  }

  // Видалити пацієнта
  deletePatient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
