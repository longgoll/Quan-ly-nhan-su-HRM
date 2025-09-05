import { api } from '@/config/api';
import type {
  Employee,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  EmployeeQueryParams,
  PaginatedResponse
} from '@/types/employee';

export const employeeApi = {
  // Get all employees with pagination and filters
  getEmployees: async (params?: EmployeeQueryParams): Promise<PaginatedResponse<Employee>> => {
    const response = await api.get('/employees', { params });
    return response.data;
  },

  // Get single employee by ID
  getEmployee: async (id: number): Promise<Employee> => {
    const response = await api.get(`/employees/${id}`);
    return response.data;
  },

  // Create new employee
  createEmployee: async (data: CreateEmployeeRequest): Promise<Employee> => {
    const response = await api.post('/employees', data);
    return response.data;
  },

  // Update employee
  updateEmployee: async (id: number, data: UpdateEmployeeRequest): Promise<Employee> => {
    const response = await api.put(`/employees/${id}`, data);
    return response.data;
  },

  // Delete employee
  deleteEmployee: async (id: number): Promise<void> => {
    await api.delete(`/employees/${id}`);
  },

  // Get employees by department
  getEmployeesByDepartment: async (departmentId: number): Promise<Employee[]> => {
    const response = await api.get(`/employees/department/${departmentId}`);
    return response.data;
  },

  // Search employees
  searchEmployees: async (query: string): Promise<Employee[]> => {
    const response = await api.get('/employees/search', {
      params: { q: query }
    });
    return response.data;
  }
};
