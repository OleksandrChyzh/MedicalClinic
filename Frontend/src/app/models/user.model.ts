export interface UserProfile {
  id: number;
  email: string;
  userName: string;
  phoneNumber: string | null;
}

export interface UpdateUserDto {
  password?: string | null;
  userName?: string | null;
  phoneNumber?: string | null;
}
