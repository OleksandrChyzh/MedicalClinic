import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Direction, MedicalService, ServiceType } from '../../models/service.models';

@Injectable({
  providedIn: 'root'
})
export class ClinicService {
  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7272/api';

  getDirections(): Observable<Direction[]> {
    return this.http.get<Direction[]>(`${this.baseUrl}/Direction`);
  }

  getServiceTypes(): Observable<ServiceType[]> {
    return this.http.get<ServiceType[]>(`${this.baseUrl}/ServiceType`);
  }

  getServices(directionId?: number | null, typeId?: number | null): Observable<MedicalService[]> {
    let params = new HttpParams();

    if (directionId) {
      params = params.set('directionId', directionId);
    }
    if (typeId) {
      params = params.set('typeId', typeId);
    }

    return this.http.get<MedicalService[]>(`${this.baseUrl}/Service`, { params });
  }
}
