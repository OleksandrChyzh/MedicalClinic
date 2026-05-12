export interface Doctor {
  id: number;
  fullName: string;
  experienceYears: number;
  description?: string;
  directionId: number;
  directionName: string;
  averageRating: number;
  reviewsCount: number;
  reviewIds: number[];
  contactPhone: string;
  contactEmail: string;
}

// Також нам знадобиться модель для напрямків (для фільтрації)
export interface Direction {
  id: number;
  name: string;
}
