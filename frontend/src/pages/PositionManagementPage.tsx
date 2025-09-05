import { useState, useEffect, useCallback } from 'react';
import { Plus, Search, MoreHorizontal, Edit, Trash2, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { PositionDialog } from '@/components/positions';
import { positionApi } from '@/api/position';
import { departmentApi } from '@/api/department';
import type { Position, Department } from '@/types/employee';
import { toast } from 'sonner';

export default function PositionManagementPage() {
  const [positions, setPositions] = useState<Position[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPosition, setEditingPosition] = useState<Position | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [departmentFilter, setDepartmentFilter] = useState<string>('all');
  const [levelFilter, setLevelFilter] = useState<string>('all');

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [positionsData, departmentsData] = await Promise.all([
        positionApi.getPositions(),
        departmentApi.getDepartments()
      ]);
      setPositions(positionsData);
      setDepartments(departmentsData);
    } catch {
      toast.error('Không thể tải dữ liệu');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleCreate = () => {
    setEditingPosition(null);
    setDialogOpen(true);
  };

  const handleEdit = (position: Position) => {
    setEditingPosition(position);
    setDialogOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa vị trí này?')) return;

    try {
      await positionApi.deletePosition(id);
      setPositions(positions.filter(p => p.id !== id));
      toast.success('Đã xóa vị trí');
    } catch {
      toast.error('Không thể xóa vị trí');
    }
  };

  const handleSave = async () => {
    await fetchData();
    setDialogOpen(false);
  };

  const filteredPositions = positions.filter(position => {
    const matchesSearch = position.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         position.code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         position.departmentName?.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesDepartment = departmentFilter === 'all' || 
                             position.departmentId?.toString() === departmentFilter;
    
    const matchesLevel = levelFilter === 'all' || 
                        position.level.toString() === levelFilter;

    return matchesSearch && matchesDepartment && matchesLevel;
  });

  const formatSalaryRange = (min: number | undefined, max: number | undefined) => {
    if (!min && !max) return 'Chưa định';
    if (min && max) return `${min.toLocaleString()} - ${max.toLocaleString()} VNĐ`;
    if (min) return `Từ ${min.toLocaleString()} VNĐ`;
    if (max) return `Tối đa ${max.toLocaleString()} VNĐ`;
    return 'Chưa định';
  };

  const getLevelBadgeVariant = (level: number) => {
    if (level <= 3) return 'default';
    if (level <= 6) return 'secondary';
    return 'destructive';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Quản lý vị trí</h1>
          <p className="text-muted-foreground">
            Quản lý các vị trí công việc trong công ty
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 h-4 w-4" />
          Thêm vị trí
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Bộ lọc</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4 items-center">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Tìm kiếm theo tên, mã vị trí hoặc phòng ban..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <Select value={departmentFilter} onValueChange={setDepartmentFilter}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Lọc theo phòng ban" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả phòng ban</SelectItem>
                {departments.map((dept) => (
                  <SelectItem key={dept.id} value={dept.id.toString()}>
                    {dept.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={levelFilter} onValueChange={setLevelFilter}>
              <SelectTrigger className="w-32">
                <SelectValue placeholder="Cấp bậc" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả</SelectItem>
                {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((level) => (
                  <SelectItem key={level} value={level.toString()}>
                    Cấp {level}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {filteredPositions.map((position) => (
          <Card key={position.id} className="hover:shadow-md transition-shadow">
            <CardHeader className="pb-3">
              <div className="flex items-start justify-between">
                <div className="space-y-1">
                  <CardTitle className="text-lg">{position.title}</CardTitle>
                  {position.code && (
                    <p className="text-sm text-muted-foreground">Mã: {position.code}</p>
                  )}
                </div>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="sm">
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onClick={() => handleEdit(position)}>
                      <Edit className="mr-2 h-4 w-4" />
                      Chỉnh sửa
                    </DropdownMenuItem>
                    <DropdownMenuItem 
                      onClick={() => handleDelete(position.id)}
                      className="text-red-600"
                    >
                      <Trash2 className="mr-2 h-4 w-4" />
                      Xóa
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Phòng ban:</span>
                <span className="text-sm font-medium">
                  {position.departmentName || 'Chưa phân công'}
                </span>
              </div>
              
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Cấp bậc:</span>
                <Badge variant={getLevelBadgeVariant(position.level)}>
                  Cấp {position.level}
                </Badge>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Nhân viên:</span>
                <div className="flex items-center gap-1">
                  <Users className="h-4 w-4 text-muted-foreground" />
                  <span className="text-sm font-medium">{position.employeeCount}</span>
                </div>
              </div>

              <div className="space-y-1">
                <span className="text-sm text-muted-foreground">Mức lương:</span>
                <p className="text-sm font-medium">
                  {formatSalaryRange(position.minSalary, position.maxSalary)}
                </p>
              </div>

              {position.description && (
                <div className="space-y-1">
                  <span className="text-sm text-muted-foreground">Mô tả:</span>
                  <p className="text-sm line-clamp-2">{position.description}</p>
                </div>
              )}

              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Trạng thái:</span>
                <Badge variant={position.isActive ? 'default' : 'secondary'}>
                  {position.isActive ? 'Hoạt động' : 'Tạm dừng'}
                </Badge>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredPositions.length === 0 && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Users className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Không tìm thấy vị trí nào</h3>
            <p className="text-muted-foreground text-center">
              {searchTerm || departmentFilter !== 'all' || levelFilter !== 'all'
                ? 'Thử thay đổi bộ lọc để xem thêm kết quả'
                : 'Chưa có vị trí nào được tạo'
              }
            </p>
          </CardContent>
        </Card>
      )}

      <PositionDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        position={editingPosition}
        departments={departments}
        onSave={handleSave}
      />
    </div>
  );
}
