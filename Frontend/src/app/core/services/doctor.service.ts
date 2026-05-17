import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Doctor, DoctorProfile } from '../../models/doctor.model';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7272/api/doctor';

  getAllDoctors(directionId?: number): Observable<Doctor[]> {
    let params = new HttpParams();
    if (directionId) {
      params = params.set('directionId', directionId.toString());
    }
    return this.http.get<Doctor[]>(this.apiUrl, { params });
  }

  getDoctorById(id: number): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.apiUrl}/${id}`);
  }

  getMyProfile(): Observable<DoctorProfile> {
    return this.http.get<DoctorProfile>(`${this.apiUrl}/profile`);
  }

  createDoctor(dto: any): Observable<Doctor> {
    return this.http.post<Doctor>(this.apiUrl, dto);
  }

  updateDoctor(id: number, dto: any): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  deleteDoctor(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
