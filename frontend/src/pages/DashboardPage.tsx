import React from 'react';
import { useAuth } from '@/hooks/useAuth';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { UserRole } from '@/types/auth';

const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  const getRoleColor = (role: string) => {
    switch (role) {
      case UserRole.Admin:
        return 'bg-red-500';
      case UserRole.HRManager:
        return 'bg-blue-500';
      case UserRole.Manager:
        return 'bg-green-500';
      case UserRole.Employee:
        return 'bg-gray-500';
      default:
        return 'bg-gray-500';
    }
  };

  const getRoleDescription = (role: string) => {
    switch (role) {
      case UserRole.Admin:
        return 'Quản trị viên - Toàn quyền hệ thống';
      case UserRole.HRManager:
        return 'Quản lý HR - Phê duyệt tài khoản, quản lý nhân sự';
      case UserRole.Manager:
        return 'Quản lý - Quản lý nhân viên';
      case UserRole.Employee:
        return 'Nhân viên - Quyền cơ bản';
      default:
        return 'Không xác định';
    }
  };

  if (!user) {
    return <div>Loading...</div>;
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600 mt-2">Chào mừng bạn đến với hệ thống HRM</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* User Info Card */}
        <Card>
          <CardHeader>
            <CardTitle>Thông tin cá nhân</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div>
              <span className="font-semibold">Họ tên:</span> {user.fullName}
            </div>
            <div>
              <span className="font-semibold">Email:</span> {user.email}
            </div>
            <div>
              <span className="font-semibold">Username:</span> {user.username}
            </div>
            {user.phoneNumber && (
              <div>
                <span className="font-semibold">SĐT:</span> {user.phoneNumber}
              </div>
            )}
            <div className="flex items-center gap-2">
              <span className="font-semibold">Vai trò:</span>
              <Badge className={getRoleColor(user.role)}>
                {user.role}
              </Badge>
            </div>
            <div className="text-sm text-gray-600">
              {getRoleDescription(user.role)}
            </div>
          </CardContent>
        </Card>

        {/* Status Card */}
        <Card>
          <CardHeader>
            <CardTitle>Trạng thái tài khoản</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex items-center gap-2">
              <span className="font-semibold">Trạng thái:</span>
              <Badge variant={user.isActive ? 'default' : 'destructive'}>
                {user.isActive ? 'Hoạt động' : 'Bị khóa'}
              </Badge>
            </div>
            <div className="flex items-center gap-2">
              <span className="font-semibold">Phê duyệt:</span>
              <Badge variant={user.isApproved ? 'default' : 'secondary'}>
                {user.isApproved ? 'Đã phê duyệt' : 'Chờ phê duyệt'}
              </Badge>
            </div>
            <div className="text-sm text-gray-600">
              Tạo lúc: {new Date(user.createdAt).toLocaleDateString('vi-VN')}
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions Card */}
        <Card>
          <CardHeader>
            <CardTitle>Thao tác nhanh</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {(user.role === UserRole.Admin || user.role === UserRole.HRManager) && (
                <div className="text-sm">
                  <strong>Quyền của bạn:</strong>
                  <ul className="list-disc list-inside mt-1 text-gray-600">
                    <li>Phê duyệt tài khoản mới</li>
                    <li>Quản lý nhân viên</li>
                    {user.role === UserRole.Admin && <li>Quản trị toàn hệ thống</li>}
                  </ul>
                </div>
              )}
              {user.role === UserRole.Manager && (
                <div className="text-sm">
                  <strong>Quyền của bạn:</strong>
                  <ul className="list-disc list-inside mt-1 text-gray-600">
                    <li>Quản lý nhân viên</li>
                    <li>Xem báo cáo</li>
                  </ul>
                </div>
              )}
              {user.role === UserRole.Employee && (
                <div className="text-sm">
                  <strong>Chức năng khả dụng:</strong>
                  <ul className="list-disc list-inside mt-1 text-gray-600">
                    <li>Xem thông tin cá nhân</li>
                    <li>Chấm công</li>
                    <li>Xin nghỉ phép</li>
                  </ul>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default DashboardPage;
