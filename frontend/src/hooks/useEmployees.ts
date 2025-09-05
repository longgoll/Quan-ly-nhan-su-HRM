import { useState, useEffect, useCallback } from 'react';
import { toast } from 'sonner';
import { employeeApi } from '@/api/employee';
import type {
  Employee,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  EmployeeQueryParams,
  PaginatedResponse
} from '@/types/employee';

export const useEmployees = () => {
  const [employees, setEmployees] = useState<PaginatedResponse<Employee>>({
    data: [],
    page: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 0
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchEmployees = useCallback(async (params?: EmployeeQueryParams) => {
    try {
      setLoading(true);
      setError(null);
      const response = await employeeApi.getEmployees(params);
      setEmployees(response);
    } catch (err: unknown) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Có lỗi xảy ra khi tải danh sách nhân viên';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createEmployee = useCallback(async (data: CreateEmployeeRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await employeeApi.createEmployee(data);
      toast.success('Tạo nhân viên thành công');
      return true;
    } catch (err: unknown) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Có lỗi xảy ra khi tạo nhân viên';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const updateEmployee = useCallback(async (id: number, data: UpdateEmployeeRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await employeeApi.updateEmployee(id, data);
      toast.success('Cập nhật nhân viên thành công');
      return true;
    } catch (err: unknown) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Có lỗi xảy ra khi cập nhật nhân viên';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteEmployee = useCallback(async (id: number): Promise<boolean> => {
    try {
      setLoading(true);
      await employeeApi.deleteEmployee(id);
      toast.success('Xóa nhân viên thành công');
      return true;
    } catch (err: unknown) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Có lỗi xảy ra khi xóa nhân viên';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    employees,
    loading,
    error,
    fetchEmployees,
    createEmployee,
    updateEmployee,
    deleteEmployee,
    refetch: fetchEmployees
  };
};

export const useEmployee = (id?: number) => {
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchEmployee = async (employeeId: number) => {
    try {
      setLoading(true);
      setError(null);
      const response = await employeeApi.getEmployee(employeeId);
      setEmployee(response);
    } catch (err: unknown) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Có lỗi xảy ra khi tải thông tin nhân viên';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (id) {
      fetchEmployee(id);
    }
  }, [id]);

  return {
    employee,
    loading,
    error,
    fetchEmployee,
    refetch: () => id && fetchEmployee(id)
  };
};
