import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Review } from '../../models/review.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7272/api/Review';

  getReviewsByService(serviceId: number): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/service/${serviceId}`);
  }
}
