import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { CalendarIcon, Loader2 } from 'lucide-react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
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
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Calendar } from '@/components/ui/calendar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useEmployees } from '@/hooks/useEmployees';
import { useDepartments } from '@/hooks/useDepartments';
import { usePositions } from '@/hooks/usePositions';
import type { 
  Employee, 
  CreateEmployeeRequest, 
  UpdateEmployeeRequest,
  MaritalStatus,
  EmployeeStatus 
} from '@/types/employee';

interface EmployeeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  employee?: Employee | null;
  onSuccess: () => void;
}

type FormData = {
  fullName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: Date | undefined;
  address: string;
  idNumber: string;
  maritalStatus: MaritalStatus | '';
  departmentId: number | '';
  positionId: number | '';
  managerId: number | '';
  hireDate: Date | undefined;
  status: EmployeeStatus;
};

const maritalStatusOptions = [
  { value: 'Single', label: 'Độc thân' },
  { value: 'Married', label: 'Đã kết hôn' },
  { value: 'Divorced', label: 'Đã ly hôn' },
  { value: 'Widowed', label: 'Góa phụ' },
];

const statusOptions = [
  { value: 'Active', label: 'Đang làm việc' },
  { value: 'Inactive', label: 'Tạm nghỉ' },
  { value: 'Terminated', label: 'Đã nghỉ việc' },
  { value: 'OnLeave', label: 'Đang nghỉ phép' },
];

export const EmployeeDialog: React.FC<EmployeeDialogProps> = ({
  open,
  onOpenChange,
  employee,
  onSuccess,
}) => {
  const [loading, setLoading] = useState(false);
  const { createEmployee, updateEmployee } = useEmployees();
  const { departments } = useDepartments();
  const { positions } = usePositions();

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    defaultValues: {
      fullName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: undefined,
      address: '',
      idNumber: '',
      maritalStatus: '',
      departmentId: '',
      positionId: '',
      managerId: '',
      hireDate: new Date(),
      status: 'Active',
    },
  });

  const watchedDepartmentId = watch('departmentId');
  const watchedDateOfBirth = watch('dateOfBirth');
  const watchedHireDate = watch('hireDate');

  // Filter positions by selected department
  const filteredPositions = positions.filter(pos => 
    !watchedDepartmentId || pos.departmentId === watchedDepartmentId
  );

  // Filter potential managers (employees in same department, excluding current employee)
  // This would require an additional API call to get employees by department
  // For now, we'll keep it simple

  useEffect(() => {
    if (employee) {
      // Populate form for editing
      reset({
        fullName: employee.fullName,
        email: employee.email || '',
        phoneNumber: employee.phoneNumber || '',
        dateOfBirth: employee.dateOfBirth ? new Date(employee.dateOfBirth) : undefined,
        address: employee.address || '',
        idNumber: employee.idNumber || '',
        maritalStatus: employee.maritalStatus || '',
        departmentId: employee.departmentId || '',
        positionId: employee.positionId || '',
        managerId: employee.managerId || '',
        hireDate: new Date(employee.hireDate),
        status: employee.status,
      });
    } else {
      // Reset form for creating
      reset({
        fullName: '',
        email: '',
        phoneNumber: '',
        dateOfBirth: undefined,
        address: '',
        idNumber: '',
        maritalStatus: '',
        departmentId: '',
        positionId: '',
        managerId: '',
        hireDate: new Date(),
        status: 'Active',
      });
    }
  }, [employee, reset]);

  const onSubmit = async (data: FormData) => {
    try {
      setLoading(true);

      const payload = {
        fullName: data.fullName,
        email: data.email || undefined,
        phoneNumber: data.phoneNumber || undefined,
        dateOfBirth: data.dateOfBirth ? format(data.dateOfBirth, 'yyyy-MM-dd') : undefined,
        address: data.address || undefined,
        idNumber: data.idNumber || undefined,
        maritalStatus: data.maritalStatus || undefined,
        departmentId: data.departmentId || undefined,
        positionId: data.positionId || undefined,
        managerId: data.managerId || undefined,
        hireDate: format(data.hireDate!, 'yyyy-MM-dd'),
      };

      let success = false;
      
      if (employee) {
        // Update existing employee
        success = await updateEmployee(employee.id, {
          ...payload,
          status: data.status,
        } as UpdateEmployeeRequest);
      } else {
        // Create new employee
        success = await createEmployee(payload as CreateEmployeeRequest);
      }

      if (success) {
        onSuccess();
      }
    } catch (error) {
      console.error('Error saving employee:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {employee ? 'Chỉnh sửa nhân viên' : 'Thêm nhân viên mới'}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <Tabs defaultValue="personal" className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="personal">Thông tin cá nhân</TabsTrigger>
              <TabsTrigger value="work">Thông tin công việc</TabsTrigger>
            </TabsList>

            <TabsContent value="personal" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="fullName">Họ và tên *</Label>
                  <Input
                    id="fullName"
                    {...register('fullName', { required: 'Họ và tên là bắt buộc' })}
                    placeholder="Nhập họ và tên"
                  />
                  {errors.fullName && (
                    <p className="text-sm text-destructive">{errors.fullName.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    {...register('email')}
                    placeholder="Nhập email"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="phoneNumber">Số điện thoại</Label>
                  <Input
                    id="phoneNumber"
                    {...register('phoneNumber')}
                    placeholder="Nhập số điện thoại"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="idNumber">CMND/CCCD</Label>
                  <Input
                    id="idNumber"
                    {...register('idNumber')}
                    placeholder="Nhập số CMND/CCCD"
                  />
                </div>

                <div className="space-y-2">
                  <Label>Ngày sinh</Label>
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="w-full justify-start text-left font-normal"
                      >
                        <CalendarIcon className="mr-2 h-4 w-4" />
                        {watchedDateOfBirth ? (
                          format(watchedDateOfBirth, 'dd/MM/yyyy', { locale: vi })
                        ) : (
                          <span>Chọn ngày sinh</span>
                        )}
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-auto p-0">
                      <Calendar
                        mode="single"
                        selected={watchedDateOfBirth}
                        onSelect={(date) => setValue('dateOfBirth', date)}
                        disabled={(date) =>
                          date > new Date() || date < new Date('1900-01-01')
                        }
                        initialFocus
                      />
                    </PopoverContent>
                  </Popover>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="maritalStatus">Tình trạng hôn nhân</Label>
                  <Select
                    value={watch('maritalStatus') as string}
                    onValueChange={(value) => setValue('maritalStatus', value as MaritalStatus)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn tình trạng hôn nhân" />
                    </SelectTrigger>
                    <SelectContent>
                      {maritalStatusOptions.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                          {option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="address">Địa chỉ</Label>
                <Textarea
                  id="address"
                  {...register('address')}
                  placeholder="Nhập địa chỉ"
                  rows={3}
                />
              </div>
            </TabsContent>

            <TabsContent value="work" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="departmentId">Phòng ban</Label>
                  <Select
                    value={watch('departmentId')?.toString() || ''}
                    onValueChange={(value) => {
                      setValue('departmentId', value ? parseInt(value) : '');
                      setValue('positionId', ''); // Reset position when department changes
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn phòng ban" />
                    </SelectTrigger>
                    <SelectContent>
                      {departments.map((dept) => (
                        <SelectItem key={dept.id} value={dept.id.toString()}>
                          {dept.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="positionId">Vị trí</Label>
                  <Select
                    value={watch('positionId')?.toString() || ''}
                    onValueChange={(value) => setValue('positionId', value ? parseInt(value) : '')}
                    disabled={!watchedDepartmentId}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn vị trí" />
                    </SelectTrigger>
                    <SelectContent>
                      {filteredPositions.map((pos) => (
                        <SelectItem key={pos.id} value={pos.id.toString()}>
                          {pos.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>Ngày vào làm *</Label>
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="w-full justify-start text-left font-normal"
                      >
                        <CalendarIcon className="mr-2 h-4 w-4" />
                        {watchedHireDate ? (
                          format(watchedHireDate, 'dd/MM/yyyy', { locale: vi })
                        ) : (
                          <span>Chọn ngày vào làm</span>
                        )}
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-auto p-0">
                      <Calendar
                        mode="single"
                        selected={watchedHireDate}
                        onSelect={(date) => setValue('hireDate', date)}
                        disabled={(date) => date > new Date()}
                        initialFocus
                      />
                    </PopoverContent>
                  </Popover>
                </div>

                {employee && (
                  <div className="space-y-2">
                    <Label htmlFor="status">Trạng thái</Label>
                    <Select
                      value={watch('status')}
                      onValueChange={(value) => setValue('status', value as EmployeeStatus)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Chọn trạng thái" />
                      </SelectTrigger>
                      <SelectContent>
                        {statusOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                )}
              </div>
            </TabsContent>
          </Tabs>

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
              {employee ? 'Cập nhật' : 'Tạo mới'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default EmployeeDialog;
