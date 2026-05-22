export interface AddMedicalRecordDTO {
  patientId: number;
  doctorId: number;
  serviceId?: number | null;
  result?: string | null;
  diagnosis: string;
  treatment?: string | null;
  recommendations?: string | null;
}

export interface GetMedicalRecordDTO {
  id: number;
  patientId: number;
  patientFullName: string;
  doctorId: number;
  doctorFullName: string;
  serviceId?: number | null;
  serviceName?: string | null;
  result?: string | null;
  diagnosis?: string | null;
  treatment?: string | null;
  recommendations?: string | null;
  createdAt: string;
}

export interface MedicalCardDTO {
  patientId: number;
  fullName: string;
  gender: string;
  dateOfBirth: string;
  records: GetMedicalRecordDTO[];
}
