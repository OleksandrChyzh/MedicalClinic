export interface Direction {
  id: number;
  name: string;
  description?: string;
}

export interface ServiceType {
  id: number;
  name: string;
  description?: string;
}

export interface MedicalService {
  id: number;
  name: string;
  description?: string;
  price: number;
  averageRating: number;
  reviewsCount: number;
}
