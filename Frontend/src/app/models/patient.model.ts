export interface Patient {
  id: number;
  userId: number;
  firstName: string;
  lastName: string;
  fullName: string;
  birthDate: string; // Сервер повертає дату у вигляді рядка ISO
  age: number;
  gender: string;
  email?: string;
  phoneNumber?: string;
  appointmentsCount: number;
  recordsCount: number;
}

export interface AddPatientDto {
  userId: number;
  firstName: string;
  lastName: string;
  birthDate: string;
  gender: string; // 'Male' або 'Female'
}
