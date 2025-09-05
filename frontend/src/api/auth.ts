import axios, { type AxiosResponse } from 'axios';
import type { LoginRequest, RegisterRequest, AuthResponse, ApiResponse, PendingUser, User } from '@/types/auth';
import { config } from '@/config/env';

const api = axios.create({
  baseURL: config.API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor để thêm token vào headers
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor để xử lý errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authAPI = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response: AxiosResponse<AuthResponse> = await api.post('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<ApiResponse<User>> => {
    const response: AxiosResponse<ApiResponse<User>> = await api.post('/auth/register', data);
    return response.data;
  },

  getCurrentUser: async (): Promise<ApiResponse<User>> => {
    const response: AxiosResponse<ApiResponse<User>> = await api.get('/auth/me');
    return response.data;
  },

  getPendingUsers: async (): Promise<ApiResponse<PendingUser[]>> => {
    const response: AxiosResponse<ApiResponse<PendingUser[]>> = await api.get('/auth/pending-users');
    return response.data;
  },

  approveUser: async (id: number): Promise<ApiResponse<null>> => {
    const response: AxiosResponse<ApiResponse<null>> = await api.post(`/auth/approve-user/${id}`);
    return response.data;
  },

  rejectUser: async (id: number): Promise<ApiResponse<null>> => {
    const response: AxiosResponse<ApiResponse<null>> = await api.post(`/auth/reject-user/${id}`);
    return response.data;
  },
};

export default api;
