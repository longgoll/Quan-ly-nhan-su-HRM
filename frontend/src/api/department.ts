import { api } from '@/config/api';
import type {
  Department,
  CreateDepartmentRequest,
  UpdateDepartmentRequest
} from '@/types/employee';

export const departmentApi = {
  // Get all departments
  getDepartments: async (): Promise<Department[]> => {
    const response = await api.get('/departments');
    return response.data;
  },

  // Get single department by ID
  getDepartment: async (id: number): Promise<Department> => {
    const response = await api.get(`/departments/${id}`);
    return response.data;
  },

  // Create new department
  createDepartment: async (data: CreateDepartmentRequest): Promise<Department> => {
    const response = await api.post('/departments', data);
    return response.data;
  },

  // Update department
  updateDepartment: async (id: number, data: UpdateDepartmentRequest): Promise<Department> => {
    const response = await api.put(`/departments/${id}`, data);
    return response.data;
  },

  // Delete department
  deleteDepartment: async (id: number): Promise<void> => {
    await api.delete(`/departments/${id}`);
  },

  // Get department hierarchy
  getDepartmentHierarchy: async (): Promise<Department[]> => {
    const response = await api.get('/departments/hierarchy');
    return response.data;
  }
};
