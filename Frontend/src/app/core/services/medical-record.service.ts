import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddMedicalRecordDTO, GetMedicalRecordDTO, MedicalCardDTO } from '../../models/medical-record.model';

@Injectable({ providedIn: 'root' })
export class MedicalRecordService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7272/api/MedicalRecord';

  createRecord(dto: AddMedicalRecordDTO): Observable<GetMedicalRecordDTO> {
    return this.http.post<GetMedicalRecordDTO>(this.apiUrl, dto);
  }

  getMedicalCard(patientId: number): Observable<MedicalCardDTO> {
    return this.http.get<MedicalCardDTO>(`${this.apiUrl}/card/${patientId}`);
  }

  getRecordById(id: number): Observable<GetMedicalRecordDTO> {
    return this.http.get<GetMedicalRecordDTO>(`${this.apiUrl}/${id}`);
  }
}
