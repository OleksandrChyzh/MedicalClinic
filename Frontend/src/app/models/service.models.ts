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
  /** З бекенду GetServiceDTO — для переходу на форму запису з попереднім вибором */
  directionId?: number;
  /** З бекенду GetServiceDTO (JSON: typeId) */
  typeId?: number;
}
