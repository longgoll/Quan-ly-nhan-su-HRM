import { api } from '@/config/api';
import type {
  Position,
  CreatePositionRequest,
  UpdatePositionRequest
} from '@/types/employee';

export const positionApi = {
  // Get all positions
  getPositions: async (): Promise<Position[]> => {
    const response = await api.get('/positions');
    return response.data;
  },

  // Get single position by ID
  getPosition: async (id: number): Promise<Position> => {
    const response = await api.get(`/positions/${id}`);
    return response.data;
  },

  // Create new position
  createPosition: async (data: CreatePositionRequest): Promise<Position> => {
    const response = await api.post('/positions', data);
    return response.data;
  },

  // Update position
  updatePosition: async (id: number, data: UpdatePositionRequest): Promise<Position> => {
    const response = await api.put(`/positions/${id}`, data);
    return response.data;
  },

  // Delete position
  deletePosition: async (id: number): Promise<void> => {
    await api.delete(`/positions/${id}`);
  },

  // Get positions by department
  getPositionsByDepartment: async (departmentId: number): Promise<Position[]> => {
    const response = await api.get(`/positions/department/${departmentId}`);
    return response.data;
  }
};
