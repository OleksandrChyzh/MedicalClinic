export interface Review {
  id: number;
  userId: number;
  userName: string;
  doctorId?: number | null;
  doctorName?: string | null;
  serviceId?: number | null;
  serviceName?: string | null;
  rating: number;
  comment?: string | null;
  createdAt: string; // або Date
}

/** Тіло POST /api/Review (бекенд очікує userId; сервер підставляє автора з JWT) */
export interface AddReviewDTO {
  userId: number;
  doctorId?: number | null;
  serviceId?: number | null;
  rating: number;
  comment?: string | null;
}
