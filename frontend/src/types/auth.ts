export interface User {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  role: UserRole;
  isActive: boolean;
  isApproved: boolean;
  createdAt: string;
  updatedAt: string;
  approvedById?: number;
}

export const UserRole = {
  Employee: 'Employee',
  Manager: 'Manager',
  HRManager: 'HRManager',
  Admin: 'Admin'
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  data: {
    token: string;
    user: User;
  };
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface PendingUser {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  createdAt: string;
}
