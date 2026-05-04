export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  userName: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  token: string;
  roles: string[];
}
