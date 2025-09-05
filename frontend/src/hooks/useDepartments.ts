import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { departmentApi } from '@/api/department';
import type {
  Department,
  CreateDepartmentRequest,
  UpdateDepartmentRequest
} from '@/types/employee';

export const useDepartments = () => {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDepartments = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await departmentApi.getDepartments();
      setDepartments(response);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tải danh sách phòng ban';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const createDepartment = async (data: CreateDepartmentRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await departmentApi.createDepartment(data);
      toast.success('Tạo phòng ban thành công');
      await fetchDepartments(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tạo phòng ban';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const updateDepartment = async (id: number, data: UpdateDepartmentRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await departmentApi.updateDepartment(id, data);
      toast.success('Cập nhật phòng ban thành công');
      await fetchDepartments(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi cập nhật phòng ban';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const deleteDepartment = async (id: number): Promise<boolean> => {
    try {
      setLoading(true);
      await departmentApi.deleteDepartment(id);
      toast.success('Xóa phòng ban thành công');
      await fetchDepartments(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi xóa phòng ban';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDepartments();
  }, []);

  return {
    departments,
    loading,
    error,
    fetchDepartments,
    createDepartment,
    updateDepartment,
    deleteDepartment,
    refetch: fetchDepartments
  };
};

export const useDepartmentHierarchy = () => {
  const [hierarchy, setHierarchy] = useState<Department[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchHierarchy = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await departmentApi.getDepartmentHierarchy();
      setHierarchy(response);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tải cây phòng ban';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchHierarchy();
  }, []);

  return {
    hierarchy,
    loading,
    error,
    fetchHierarchy,
    refetch: fetchHierarchy
  };
};
