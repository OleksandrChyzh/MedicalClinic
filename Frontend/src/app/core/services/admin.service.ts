import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AdminUser {
  id: number;
  email: string;
  userName: string;
  phoneNumber?: string | null;
  isBlocked: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7272/api';

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.baseUrl}/User`);
  }

  setUserBlocked(id: number, blocked: boolean): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/User/${id}/block`, null, {
      params: { blocked }
    });
  }
}
