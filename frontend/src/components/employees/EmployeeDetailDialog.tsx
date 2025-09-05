import React from 'react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { Mail, Phone, MapPin, Calendar, Users, Briefcase, Clock } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import type { Employee, EmployeeStatus } from '@/types/employee';

interface EmployeeDetailDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  employee: Employee | null;
}

const getStatusBadge = (status: EmployeeStatus) => {
  const statusConfig = {
    Active: { label: 'Đang làm việc', variant: 'default' as const },
    Inactive: { label: 'Tạm nghỉ', variant: 'secondary' as const },
    Terminated: { label: 'Đã nghỉ việc', variant: 'destructive' as const },
    OnLeave: { label: 'Đang nghỉ phép', variant: 'outline' as const },
  };

  const config = statusConfig[status] || statusConfig.Active;
  return (
    <Badge variant={config.variant}>
      {config.label}
    </Badge>
  );
};

const getMaritalStatusLabel = (status?: string) => {
  const statusMap = {
    Single: 'Độc thân',
    Married: 'Đã kết hôn',
    Divorced: 'Đã ly hôn',
    Widowed: 'Góa phụ',
  };
  return status ? statusMap[status as keyof typeof statusMap] || status : '-';
};

const getInitials = (name: string) => {
  return name
    .split(' ')
    .map(word => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2);
};

export const EmployeeDetailDialog: React.FC<EmployeeDetailDialogProps> = ({
  open,
  onOpenChange,
  employee,
}) => {
  if (!employee) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Chi tiết nhân viên</DialogTitle>
          <DialogDescription>
            Xem thông tin chi tiết của nhân viên
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Header Info */}
          <div className="flex items-start space-x-4">
            <Avatar className="h-20 w-20">
              <AvatarFallback className="text-lg">
                {getInitials(employee.fullName)}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <div className="flex items-center space-x-3 mb-2">
                <h2 className="text-2xl font-bold">{employee.fullName}</h2>
                {getStatusBadge(employee.status)}
              </div>
              <p className="text-lg text-muted-foreground mb-1">
                {employee.position?.name || 'Chưa có vị trí'} • {employee.department?.name || 'Chưa có phòng ban'}
              </p>
              <p className="text-sm text-muted-foreground">
                Mã nhân viên: <span className="font-medium">{employee.employeeCode}</span>
              </p>
            </div>
          </div>

          <Separator />

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Personal Information */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Users className="h-5 w-5" />
                  <span>Thông tin cá nhân</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {employee.email && (
                  <div className="flex items-center space-x-3">
                    <Mail className="h-4 w-4 text-muted-foreground" />
                    <div>
                      <p className="text-sm text-muted-foreground">Email</p>
                      <p className="font-medium">{employee.email}</p>
                    </div>
                  </div>
                )}

                {employee.phoneNumber && (
                  <div className="flex items-center space-x-3">
                    <Phone className="h-4 w-4 text-muted-foreground" />
                    <div>
                      <p className="text-sm text-muted-foreground">Số điện thoại</p>
                      <p className="font-medium">{employee.phoneNumber}</p>
                    </div>
                  </div>
                )}

                {employee.dateOfBirth && (
                  <div className="flex items-center space-x-3">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    <div>
                      <p className="text-sm text-muted-foreground">Ngày sinh</p>
                      <p className="font-medium">
                        {format(new Date(employee.dateOfBirth), 'dd/MM/yyyy', { locale: vi })}
                      </p>
                    </div>
                  </div>
                )}

                {employee.address && (
                  <div className="flex items-start space-x-3">
                    <MapPin className="h-4 w-4 text-muted-foreground mt-1" />
                    <div>
                      <p className="text-sm text-muted-foreground">Địa chỉ</p>
                      <p className="font-medium">{employee.address}</p>
                    </div>
                  </div>
                )}

                {employee.idNumber && (
                  <div>
                    <p className="text-sm text-muted-foreground">CMND/CCCD</p>
                    <p className="font-medium">{employee.idNumber}</p>
                  </div>
                )}

                <div>
                  <p className="text-sm text-muted-foreground">Tình trạng hôn nhân</p>
                  <p className="font-medium">{getMaritalStatusLabel(employee.maritalStatus)}</p>
                </div>
              </CardContent>
            </Card>

            {/* Work Information */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Briefcase className="h-5 w-5" />
                  <span>Thông tin công việc</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-muted-foreground">Phòng ban</p>
                  <p className="font-medium">{employee.department?.name || '-'}</p>
                  {employee.department?.description && (
                    <p className="text-sm text-muted-foreground mt-1">
                      {employee.department.description}
                    </p>
                  )}
                </div>

                <div>
                  <p className="text-sm text-muted-foreground">Vị trí</p>
                  <p className="font-medium">{employee.position?.name || '-'}</p>
                  {employee.position?.description && (
                    <p className="text-sm text-muted-foreground mt-1">
                      {employee.position.description}
                    </p>
                  )}
                  {employee.position?.level && (
                    <p className="text-sm text-muted-foreground">
                      Cấp độ: {employee.position.level}
                    </p>
                  )}
                </div>

                {employee.manager && (
                  <div>
                    <p className="text-sm text-muted-foreground">Quản lý trực tiếp</p>
                    <p className="font-medium">{employee.manager.fullName}</p>
                    <p className="text-sm text-muted-foreground">
                      {employee.manager.employeeCode}
                    </p>
                  </div>
                )}

                <div className="flex items-center space-x-3">
                  <Clock className="h-4 w-4 text-muted-foreground" />
                  <div>
                    <p className="text-sm text-muted-foreground">Ngày vào làm</p>
                    <p className="font-medium">
                      {format(new Date(employee.hireDate), 'dd/MM/yyyy', { locale: vi })}
                    </p>
                  </div>
                </div>

                <div>
                  <p className="text-sm text-muted-foreground">Trạng thái</p>
                  <div className="mt-1">
                    {getStatusBadge(employee.status)}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* System Information */}
          <Card>
            <CardHeader>
              <CardTitle>Thông tin hệ thống</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground">Ngày tạo</p>
                  <p className="font-medium">
                    {format(new Date(employee.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                  </p>
                </div>
                <div>
                  <p className="text-muted-foreground">Cập nhật lần cuối</p>
                  <p className="font-medium">
                    {format(new Date(employee.updatedAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default EmployeeDetailDialog;
