import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile, UpdateUserDto } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);

  // Вкажи ТУТ свій реальний порт, на якому запущений бекенд (наприклад, 5001, 7000 тощо)
  private apiUrl = 'https://localhost:7272/api/user/profile';

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(this.apiUrl);
  }

  updateProfile(data: UpdateUserDto): Observable<UserProfile> {
    return this.http.put<UserProfile>(this.apiUrl, data);
  }
}
