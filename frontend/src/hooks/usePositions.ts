import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { positionApi } from '@/api/position';
import type {
  Position,
  CreatePositionRequest,
  UpdatePositionRequest
} from '@/types/employee';

export const usePositions = () => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPositions = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await positionApi.getPositions();
      setPositions(response);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tải danh sách vị trí';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const createPosition = async (data: CreatePositionRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await positionApi.createPosition(data);
      toast.success('Tạo vị trí thành công');
      await fetchPositions(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tạo vị trí';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const updatePosition = async (id: number, data: UpdatePositionRequest): Promise<boolean> => {
    try {
      setLoading(true);
      await positionApi.updatePosition(id, data);
      toast.success('Cập nhật vị trí thành công');
      await fetchPositions(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi cập nhật vị trí';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const deletePosition = async (id: number): Promise<boolean> => {
    try {
      setLoading(true);
      await positionApi.deletePosition(id);
      toast.success('Xóa vị trí thành công');
      await fetchPositions(); // Refresh list
      return true;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi xóa vị trí';
      setError(errorMessage);
      toast.error(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPositions();
  }, []);

  return {
    positions,
    loading,
    error,
    fetchPositions,
    createPosition,
    updatePosition,
    deletePosition,
    refetch: fetchPositions
  };
};

export const usePositionsByDepartment = (departmentId?: number) => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPositions = async (deptId: number) => {
    try {
      setLoading(true);
      setError(null);
      const response = await positionApi.getPositionsByDepartment(deptId);
      setPositions(response);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tải danh sách vị trí';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (departmentId) {
      fetchPositions(departmentId);
    }
  }, [departmentId]);

  return {
    positions,
    loading,
    error,
    fetchPositions,
    refetch: () => departmentId && fetchPositions(departmentId)
  };
};
