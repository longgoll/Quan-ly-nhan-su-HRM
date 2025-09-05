import React, { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/hooks/useAuth';
import { authAPI } from '@/api/auth';
import type { PendingUser } from '@/types/auth';
import { toast } from 'sonner';
import { UserCheck, UserX, Clock, Users } from 'lucide-react';

const UserManagementPage: React.FC = () => {
  const { user } = useAuth();
  const [pendingUsers, setPendingUsers] = useState<PendingUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  useEffect(() => {
    loadPendingUsers();
  }, []);

  const loadPendingUsers = async () => {
    try {
      setLoading(true);
      const response = await authAPI.getPendingUsers();
      if (response.success) {
        setPendingUsers(response.data);
      }
    } catch {
      toast.error('Không thể tải danh sách người dùng chờ duyệt');
    } finally {
      setLoading(false);
    }
  };

  const handleApproveUser = async (userId: number) => {
    try {
      setActionLoading(userId);
      const response = await authAPI.approveUser(userId);
      if (response.success) {
        toast.success('Phê duyệt người dùng thành công');
        setPendingUsers(prev => prev.filter(u => u.id !== userId));
      }
    } catch {
      toast.error('Không thể phê duyệt người dùng');
    } finally {
      setActionLoading(null);
    }
  };

  const handleRejectUser = async (userId: number) => {
    try {
      setActionLoading(userId);
      const response = await authAPI.rejectUser(userId);
      if (response.success) {
        toast.success('Từ chối người dùng thành công');
        setPendingUsers(prev => prev.filter(u => u.id !== userId));
      }
    } catch {
      toast.error('Không thể từ chối người dùng');
    } finally {
      setActionLoading(null);
    }
  };

  if (!user || (user.role !== 'Admin' && user.role !== 'HRManager')) {
    return (
      <div className="container mx-auto py-8 px-4">
        <Card>
          <CardContent className="text-center py-8">
            <h2 className="text-xl font-semibold text-red-600">Không có quyền truy cập</h2>
            <p className="text-gray-600 mt-2">Bạn không có quyền truy cập trang này.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-2">
          <Users className="h-8 w-8" />
          Quản lý người dùng
        </h1>
        <p className="text-gray-600 mt-2">Phê duyệt và quản lý tài khoản người dùng mới</p>
      </div>

      {loading ? (
        <Card>
          <CardContent className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
            <p className="mt-2 text-gray-600">Đang tải...</p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-6">
          {/* Statistics */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card>
              <CardContent className="p-6">
                <div className="flex items-center gap-2">
                  <Clock className="h-5 w-5 text-orange-500" />
                  <div>
                    <p className="text-sm font-medium text-gray-600">Chờ phê duyệt</p>
                    <p className="text-2xl font-bold">{pendingUsers.length}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Pending Users List */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Người dùng chờ phê duyệt ({pendingUsers.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {pendingUsers.length === 0 ? (
                <div className="text-center py-8">
                  <Clock className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-gray-600">Không có người dùng chờ duyệt</h3>
                  <p className="text-gray-500">Tất cả người dùng đã được phê duyệt.</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {pendingUsers.map((pendingUser) => (
                    <div
                      key={pendingUser.id}
                      className="border rounded-lg p-4 hover:shadow-md transition-shadow"
                    >
                      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <h3 className="font-semibold">{pendingUser.fullName}</h3>
                            <Badge variant="secondary">Chờ duyệt</Badge>
                          </div>
                          <p className="text-sm text-gray-600">
                            <strong>Email:</strong> {pendingUser.email}
                          </p>
                          <p className="text-sm text-gray-600">
                            <strong>Username:</strong> {pendingUser.username}
                          </p>
                          {pendingUser.phoneNumber && (
                            <p className="text-sm text-gray-600">
                              <strong>SĐT:</strong> {pendingUser.phoneNumber}
                            </p>
                          )}
                          <p className="text-xs text-gray-500">
                            Đăng ký: {new Date(pendingUser.createdAt).toLocaleDateString('vi-VN')}
                          </p>
                        </div>
                        <div className="flex gap-2">
                          <Button
                            onClick={() => handleApproveUser(pendingUser.id)}
                            disabled={actionLoading === pendingUser.id}
                            size="sm"
                            variant="default"
                          >
                            {actionLoading === pendingUser.id ? (
                              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                            ) : (
                              <>
                                <UserCheck className="h-4 w-4 mr-1" />
                                Phê duyệt
                              </>
                            )}
                          </Button>
                          <Button
                            onClick={() => handleRejectUser(pendingUser.id)}
                            disabled={actionLoading === pendingUser.id}
                            size="sm"
                            variant="destructive"
                          >
                            {actionLoading === pendingUser.id ? (
                              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                            ) : (
                              <>
                                <UserX className="h-4 w-4 mr-1" />
                                Từ chối
                              </>
                            )}
                          </Button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
};

export default UserManagementPage;
