import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AiTriageRequest {
  symptomText: string;
}

export interface AiTriageResult {
  isRecognized: boolean;
  predictedSpecialty: string | null;
  predictedUrgency: string | null;
  confidence: number;
  humanMessage: string;
}

@Injectable({ providedIn: 'root' })
export class AiTriageService {
  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7272/api';

  analyze(request: AiTriageRequest): Observable<AiTriageResult> {
    return this.http.post<AiTriageResult>(`${this.baseUrl}/AiTriage/analyze`, request);
  }
}
