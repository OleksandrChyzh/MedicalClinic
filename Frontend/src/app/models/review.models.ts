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
