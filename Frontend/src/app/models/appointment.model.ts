export interface AddAppointmentDTO {
  patientId: number;
  userId: number;
  doctorId: number;
  serviceId: number;
  appointmentDate: string; // ISO string
  durationMinutes: number;
}

export interface FreeSlotDTO {
  startTime: string;
  endTime: string;
}

export interface AvailableSlotsResponseDTO {
  doctorId: number;
  date: string;
  freeSlots: FreeSlotDTO[];
}

export interface GetAppointmentDTO {
  id: number;
  patientId: number;
  userId: number;
  userName: string;
  doctorId: number;
  serviceId: number;
  patientFullName: string;
  doctorFullName: string;
  serviceName: string;
  appointmentDate: string;
  status: string;
  durationMinutes: number;
  createdAt?: string;
}
