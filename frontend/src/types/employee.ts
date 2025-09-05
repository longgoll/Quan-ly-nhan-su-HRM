export interface Employee {
  id: number;
  employeeCode: string;
  fullName: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  address?: string;
  idNumber?: string;
  maritalStatus?: MaritalStatus;
  departmentId?: number;
  positionId?: number;
  managerId?: number;
  hireDate: string;
  status: EmployeeStatus;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  
  // Navigation properties
  department?: Department;
  position?: Position;
  manager?: Employee;
}

export const MaritalStatus = {
  Single: 'Single',
  Married: 'Married',
  Divorced: 'Divorced',
  Widowed: 'Widowed'
} as const;

export type MaritalStatus = typeof MaritalStatus[keyof typeof MaritalStatus];

export const EmployeeStatus = {
  Active: 'Active',
  Inactive: 'Inactive',
  Terminated: 'Terminated',
  OnLeave: 'OnLeave'
} as const;

export type EmployeeStatus = typeof EmployeeStatus[keyof typeof EmployeeStatus];

export interface Department {
  id: number;
  name: string;
  code?: string;
  description?: string;
  parentDepartmentId?: number;
  parentDepartmentName?: string;
  managerId?: number;
  managerName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  employeeCount: number;
  subDepartments: Department[];
  
  // Navigation properties (for backward compatibility)
  parentId?: number;
  parent?: Department;
  manager?: Employee;
  children?: Department[];
}

export interface Position {
  id: number;
  title: string;
  code?: string;
  description?: string;
  requirements?: string;
  departmentId?: number;
  departmentName?: string;
  level: number;
  minSalary?: number;
  maxSalary?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  employeeCount: number;
  
  // Navigation properties
  department?: Department;
}

export interface CreateEmployeeRequest {
  fullName: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  address?: string;
  idNumber?: string;
  maritalStatus?: MaritalStatus;
  departmentId?: number;
  positionId?: number;
  managerId?: number;
  hireDate: string;
}

export interface UpdateEmployeeRequest {
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  address?: string;
  idNumber?: string;
  maritalStatus?: MaritalStatus;
  departmentId?: number;
  positionId?: number;
  managerId?: number;
  status?: EmployeeStatus;
}

export interface CreateDepartmentRequest {
  name: string;
  description?: string;
  parentDepartmentId?: number;
  managerId?: number;
}

export interface UpdateDepartmentRequest {
  name?: string;
  description?: string;
  parentDepartmentId?: number;
  managerId?: number;
}

export interface CreatePositionRequest {
  title: string;
  code?: string;
  description?: string;
  requirements?: string;
  departmentId?: number;
  level: number;
  minSalary?: number;
  maxSalary?: number;
}

export interface UpdatePositionRequest {
  title: string;
  code?: string;
  description?: string;
  requirements?: string;
  departmentId?: number;
  level: number;
  minSalary?: number;
  maxSalary?: number;
  isActive: boolean;
}

export interface EmployeeQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  departmentId?: number;
  positionId?: number;
  status?: EmployeeStatus;
  managerId?: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
