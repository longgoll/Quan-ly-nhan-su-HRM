import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { CalendarIcon, Loader2 } from 'lucide-react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import {
  Dialog,
  DialogContent,
  DialogDescription,
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
  EmployeeStatus,
  Gender
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
  maritalStatus: MaritalStatus | 'none';
  departmentId: number | 'none';
  positionId: number | 'none';
  managerId: number | 'none';
  hireDate: Date | undefined;
  status: EmployeeStatus;
  gender: Gender | 'none';
};

const maritalStatusOptions = [
  { value: 1, label: 'Độc thân' },
  { value: 2, label: 'Đã kết hôn' },
  { value: 3, label: 'Đã ly hôn' },
  { value: 4, label: 'Góa phụ' },
];

const genderOptions = [
  { value: 1, label: 'Nam' },
  { value: 2, label: 'Nữ' },
  { value: 3, label: 'Khác' },
];

const statusOptions = [
  { value: 1, label: 'Đang làm việc' },
  { value: 2, label: 'Tạm nghỉ' },
  { value: 3, label: 'Đã nghỉ việc' },
  { value: 4, label: 'Đang nghỉ phép' },
];

export const EmployeeDialog: React.FC<EmployeeDialogProps> = ({
  open,
  onOpenChange,
  employee,
  onSuccess,
}) => {
  const [loading, setLoading] = useState(false);
  const { createEmployee, updateEmployee } = useEmployees();
  const { departments, fetchDepartments } = useDepartments();
  const { positions, fetchPositions } = usePositions();

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
      maritalStatus: 'none',
      departmentId: 'none',
      positionId: 'none',
      managerId: 'none',
      hireDate: new Date(),
      status: 1, // Active
      gender: 'none',
    },
  });

  const watchedDepartmentId = watch('departmentId');
  const watchedDateOfBirth = watch('dateOfBirth');
  const watchedHireDate = watch('hireDate');

  // Filter positions by selected department
  const filteredPositions = positions?.filter(pos => 
    watchedDepartmentId && watchedDepartmentId !== 'none' && pos.departmentId === watchedDepartmentId
  ) || [];

  // Fetch departments and positions when component mounts
  useEffect(() => {
    fetchDepartments();
    fetchPositions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Intentionally excluding functions to prevent infinite loop

  // Filter potential managers (employees in same department, excluding current employee)
  // This would require an additional API call to get employees by department
  // For now, we'll keep it simple

  useEffect(() => {
    if (employee) {
      // Populate form for editing
      reset({
        fullName: employee.fullName,
        email: employee.personalEmail || '',
        phoneNumber: employee.personalPhoneNumber || '',
        dateOfBirth: employee.dateOfBirth ? new Date(employee.dateOfBirth) : undefined,
        address: employee.address || '',
        idNumber: employee.identityNumber || '',
        maritalStatus: employee.maritalStatus || 'none',
        departmentId: employee.department?.id || 'none',
        positionId: employee.position?.id || 'none',
        managerId: employee.directManager?.id || 'none',
        hireDate: new Date(employee.hireDate),
        status: employee.status,
        gender: employee.gender || 'none',
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
        maritalStatus: 'none',
        departmentId: 'none',
        positionId: 'none',
        managerId: 'none',
        hireDate: new Date(),
        status: 1, // Active
        gender: 'none',
      });
    }
  }, [employee, reset]);

  const onSubmit = async (data: FormData) => {
    try {
      setLoading(true);

      if (!data.hireDate) {
        throw new Error('Hire date is required');
      }

      if (data.gender === 'none') {
        throw new Error('Gender is required');
      }

      // Split fullName into firstName and lastName
      const nameParts = data.fullName.trim().split(' ');
      const firstName = nameParts[0] || '';
      const lastName = nameParts.slice(1).join(' ') || '';

      if (!firstName || !lastName) {
        throw new Error('Please provide both first and last name');
      }

      const payload: CreateEmployeeRequest = {
        userId: 1, // TODO: Get actual user ID - for now using placeholder
        firstName,
        lastName,
        dateOfBirth: data.dateOfBirth ? format(data.dateOfBirth, 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
        gender: data.gender,
        identityNumber: data.idNumber || undefined,
        address: data.address || undefined,
        personalPhoneNumber: data.phoneNumber || undefined,
        personalEmail: data.email || undefined,
        maritalStatus: data.maritalStatus !== 'none' ? data.maritalStatus : undefined,
        departmentId: data.departmentId !== 'none' ? data.departmentId : undefined,
        positionId: data.positionId !== 'none' ? data.positionId : undefined,
        directManagerId: data.managerId !== 'none' ? data.managerId : undefined,
        hireDate: format(data.hireDate, 'yyyy-MM-dd'),
      };

      let success = false;
      
      if (employee) {
        // Update existing employee - this needs different handling
        const updatePayload = {
          firstName: data.fullName.trim().split(' ')[0] || '',
          lastName: data.fullName.trim().split(' ').slice(1).join(' ') || '',
          dateOfBirth: data.dateOfBirth ? format(data.dateOfBirth, 'yyyy-MM-dd') : undefined,
          gender: typeof data.gender === 'number' ? data.gender : undefined,
          identityNumber: data.idNumber || undefined,
          address: data.address || undefined,
          personalPhoneNumber: data.phoneNumber || undefined,
          personalEmail: data.email || undefined,
          maritalStatus: data.maritalStatus !== 'none' ? data.maritalStatus : undefined,
          departmentId: data.departmentId !== 'none' ? data.departmentId : undefined,
          positionId: data.positionId !== 'none' ? data.positionId : undefined,
          directManagerId: data.managerId !== 'none' ? data.managerId : undefined,
          status: data.status,
        };
        success = await updateEmployee(employee.id, updatePayload as UpdateEmployeeRequest);
      } else {
        // Create new employee
        success = await createEmployee(payload);
      }

      if (success) {
        onSuccess();
      }
    } catch (error) {
      console.error('Error saving employee:', error);
      // TODO: Show error toast to user
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
          <DialogDescription>
            {employee ? 'Cập nhật thông tin của nhân viên' : 'Nhập thông tin để tạo nhân viên mới'}
          </DialogDescription>
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
                    value={watch('maritalStatus')?.toString() || 'none'}
                    onValueChange={(value) => setValue('maritalStatus', value === 'none' ? 'none' : parseInt(value) as MaritalStatus)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn tình trạng hôn nhân" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Chọn tình trạng hôn nhân</SelectItem>
                      {maritalStatusOptions.map((option) => (
                        <SelectItem key={option.value} value={option.value.toString()}>
                          {option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="gender">Giới tính *</Label>
                  <Select
                    value={watch('gender')?.toString() || 'none'}
                    onValueChange={(value) => setValue('gender', value === 'none' ? 'none' : parseInt(value) as Gender)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn giới tính" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Chọn giới tính</SelectItem>
                      {genderOptions.map((option) => (
                        <SelectItem key={option.value} value={option.value.toString()}>
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
                    value={watch('departmentId')?.toString() || 'none'}
                    onValueChange={(value) => {
                      setValue('departmentId', value === 'none' ? 'none' : parseInt(value));
                      setValue('positionId', 'none'); // Reset position when department changes
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn phòng ban" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Chọn phòng ban</SelectItem>
                      {departments?.map((dept) => (
                        <SelectItem key={dept.id} value={dept.id.toString()}>
                          {dept.name}
                        </SelectItem>
                      )) || []}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="positionId">Vị trí</Label>
                  <Select
                    value={watch('positionId')?.toString() || 'none'}
                    onValueChange={(value) => setValue('positionId', value === 'none' ? 'none' : parseInt(value))}
                    disabled={!watchedDepartmentId || watchedDepartmentId === 'none'}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn vị trí" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Chọn vị trí</SelectItem>
                      {filteredPositions?.map((pos) => (
                        <SelectItem key={pos.id} value={pos.id.toString()}>
                          {pos.title}
                        </SelectItem>
                      )) || []}
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
                      value={watch('status')?.toString() || '1'}
                      onValueChange={(value) => setValue('status', parseInt(value) as EmployeeStatus)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Chọn trạng thái" />
                      </SelectTrigger>
                      <SelectContent>
                        {statusOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value.toString()}>
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
