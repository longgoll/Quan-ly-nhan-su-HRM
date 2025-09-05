import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Loader2 } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { useDepartments } from '@/hooks/useDepartments';
import type { 
  Department, 
  CreateDepartmentRequest, 
  UpdateDepartmentRequest
} from '@/types/employee';

interface DepartmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department?: Department | null;
  onSuccess: () => void;
}

type FormData = {
  name: string;
  description: string;
  parentId: number | '';
  managerId: number | '';
  isActive: boolean;
};

export const DepartmentDialog: React.FC<DepartmentDialogProps> = ({
  open,
  onOpenChange,
  department,
  onSuccess,
}) => {
  const [loading, setLoading] = useState(false);
  const { departments, createDepartment, updateDepartment } = useDepartments();

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    defaultValues: {
      name: '',
      description: '',
      parentId: '',
      managerId: '',
      isActive: true,
    },
  });

  const watchedParentId = watch('parentId');
  const watchedIsActive = watch('isActive');

  // Filter out current department and its children from parent options
  const availableParents = departments.filter(dept => 
    dept.id !== department?.id && 
    dept.parentId !== department?.id
  );

  useEffect(() => {
    if (department) {
      // Populate form for editing
      reset({
        name: department.name,
        description: department.description || '',
        parentId: department.parentId || '',
        managerId: department.managerId || '',
        isActive: department.isActive,
      });
    } else {
      // Reset form for creating
      reset({
        name: '',
        description: '',
        parentId: '',
        managerId: '',
        isActive: true,
      });
    }
  }, [department, reset]);

  const onSubmit = async (data: FormData) => {
    try {
      setLoading(true);

      const payload = {
        name: data.name,
        description: data.description || undefined,
        parentId: data.parentId || undefined,
        managerId: data.managerId || undefined,
      };

      let success = false;
      
      if (department) {
        // Update existing department
        success = await updateDepartment(department.id, payload as UpdateDepartmentRequest);
      } else {
        // Create new department
        success = await createDepartment(payload as CreateDepartmentRequest);
      }

      if (success) {
        onSuccess();
      }
    } catch (error) {
      console.error('Error saving department:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {department ? 'Chỉnh sửa phòng ban' : 'Thêm phòng ban mới'}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Tên phòng ban *</Label>
              <Input
                id="name"
                {...register('name', { required: 'Tên phòng ban là bắt buộc' })}
                placeholder="Nhập tên phòng ban"
              />
              {errors.name && (
                <p className="text-sm text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Mô tả</Label>
              <Textarea
                id="description"
                {...register('description')}
                placeholder="Nhập mô tả phòng ban"
                rows={3}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="parentId">Phòng ban cha</Label>
              <Select
                value={watchedParentId?.toString() || ''}
                onValueChange={(value) => setValue('parentId', value ? parseInt(value) : '')}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn phòng ban cha (không bắt buộc)" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Không có phòng ban cha</SelectItem>
                  {availableParents.map((dept) => (
                    <SelectItem key={dept.id} value={dept.id.toString()}>
                      {dept.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {department && (
              <div className="flex items-center space-x-2">
                <Switch
                  id="isActive"
                  checked={watchedIsActive}
                  onCheckedChange={(checked) => setValue('isActive', checked)}
                />
                <Label htmlFor="isActive">Phòng ban đang hoạt động</Label>
              </div>
            )}
          </div>

          <div className="flex justify-end space-x-2 pt-4 border-t">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={loading}
            >
              Hủy
            </Button>
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {department ? 'Cập nhật' : 'Tạo mới'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default DepartmentDialog;
