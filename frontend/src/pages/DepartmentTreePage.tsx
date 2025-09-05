import { useState, useEffect, useCallback } from 'react';
import { Plus, ChevronRight, ChevronDown, Building2, Users, Edit, Trash2, MoreHorizontal } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { DepartmentDialog } from '@/components/departments/DepartmentDialog';
import { departmentApi } from '@/api/department';
import { employeeApi } from '@/api/employee';
import type { Department, Employee } from '@/types/employee';
import { toast } from 'sonner';

interface DepartmentTreeNode extends Department {
  children: DepartmentTreeNode[];
  employees: Employee[];
  isExpanded: boolean;
}

export default function DepartmentTreePage() {
  const [departments, setDepartments] = useState<DepartmentTreeNode[]>([]);
  const [allEmployees, setAllEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingDepartment, setEditingDepartment] = useState<Department | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [departmentsData, employeesResponse] = await Promise.all([
        departmentApi.getDepartments(),
        employeeApi.getEmployees({ pageSize: 1000 }) // Get all employees
      ]);
      
      const departmentTree = buildDepartmentTree(departmentsData, employeesResponse.data);
      setDepartments(departmentTree);
      setAllEmployees(employeesResponse.data);
    } catch {
      toast.error('Không thể tải dữ liệu');
    } finally {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const buildDepartmentTree = useCallback((departments: Department[], employees: Employee[]): DepartmentTreeNode[] => {
    const departmentMap = new Map<number, DepartmentTreeNode>();
    
    // Initialize all departments
    departments.forEach(dept => {
      departmentMap.set(dept.id, {
        ...dept,
        children: [],
        employees: employees.filter(emp => emp.departmentId === dept.id),
        isExpanded: true
      });
    });

    const rootDepartments: DepartmentTreeNode[] = [];

    // Build tree structure
    departments.forEach(dept => {
      const node = departmentMap.get(dept.id)!;
      
      if (dept.parentId && departmentMap.has(dept.parentId)) {
        const parent = departmentMap.get(dept.parentId)!;
        parent.children.push(node);
      } else {
        rootDepartments.push(node);
      }
    });

    return rootDepartments;
  }, []);

  const toggleExpanded = (departmentId: number) => {
    const updateExpanded = (nodes: DepartmentTreeNode[]): DepartmentTreeNode[] => {
      return nodes.map(node => {
        if (node.id === departmentId) {
          return { ...node, isExpanded: !node.isExpanded };
        }
        return { ...node, children: updateExpanded(node.children) };
      });
    };
    
    setDepartments(updateExpanded(departments));
  };

  const handleCreate = useCallback((parentId?: number) => {
    setEditingDepartment(parentId ? { parentId } as Department : null);
    setDialogOpen(true);
  }, []);

  const handleEdit = useCallback((department: Department) => {
    setEditingDepartment(department);
    setDialogOpen(true);
  }, []);

  const handleDelete = useCallback(async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa phòng ban này?')) return;

    try {
      await departmentApi.deleteDepartment(id);
      await fetchData();
      toast.success('Đã xóa phòng ban');
    } catch {
      toast.error('Không thể xóa phòng ban');
    }
  }, [fetchData]);

  const handleSave = useCallback(async () => {
    await fetchData();
    setDialogOpen(false);
  }, [fetchData]);

  const getManagerName = useCallback((managerId?: number) => {
    if (!managerId) return 'Chưa có';
    const manager = allEmployees.find(emp => emp.id === managerId);
    return manager?.fullName || 'Không xác định';
  }, [allEmployees]);

  const DepartmentTreeItem = ({ department, level = 0 }: { department: DepartmentTreeNode; level?: number }) => {
    const hasChildren = department.children.length > 0;
    const paddingLeft = level * 24;

    return (
      <div>
        {/* Department Row */}
        <div 
          className="flex items-center p-3 hover:bg-gray-50 border-b"
          style={{ paddingLeft: paddingLeft + 12 }}
        >
          {/* Expand/Collapse Button */}
          <div className="w-6 h-6 flex items-center justify-center mr-2">
            {hasChildren && (
              <Button
                variant="ghost"
                size="sm"
                className="w-6 h-6 p-0"
                onClick={() => toggleExpanded(department.id)}
              >
                {department.isExpanded ? (
                  <ChevronDown className="h-4 w-4" />
                ) : (
                  <ChevronRight className="h-4 w-4" />
                )}
              </Button>
            )}
          </div>

          {/* Department Icon */}
          <Building2 className="h-5 w-5 text-blue-600 mr-3" />

          {/* Department Info */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-3">
              <h3 className="font-medium text-gray-900 truncate">
                {department.name}
              </h3>
              <Badge variant={department.isActive ? 'default' : 'secondary'}>
                {department.isActive ? 'Hoạt động' : 'Tạm dừng'}
              </Badge>
            </div>
            
            <div className="flex items-center gap-6 mt-1 text-sm text-gray-600">
              <div className="flex items-center gap-1">
                <Users className="h-4 w-4" />
                <span>{department.employees.length} nhân viên</span>
              </div>
              <span>Quản lý: {getManagerName(department.managerId)}</span>
            </div>
            
            {department.description && (
              <p className="text-sm text-gray-500 mt-1 line-clamp-1">
                {department.description}
              </p>
            )}
          </div>

          {/* Actions */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="sm">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => handleCreate(department.id)}>
                <Plus className="mr-2 h-4 w-4" />
                Thêm phòng ban con
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => handleEdit(department)}>
                <Edit className="mr-2 h-4 w-4" />
                Chỉnh sửa
              </DropdownMenuItem>
              <DropdownMenuItem 
                onClick={() => handleDelete(department.id)}
                className="text-red-600"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Xóa
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        {/* Children */}
        {hasChildren && department.isExpanded && (
          <div>
            {department.children.map((child) => (
              <DepartmentTreeItem 
                key={child.id} 
                department={child} 
                level={level + 1} 
              />
            ))}
          </div>
        )}
      </div>
    );
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
          <h1 className="text-3xl font-bold">Cơ cấu tổ chức</h1>
          <p className="text-muted-foreground">
            Quản lý cấu trúc phòng ban theo hình cây phân cấp
          </p>
        </div>
        <Button onClick={() => handleCreate()}>
          <Plus className="mr-2 h-4 w-4" />
          Thêm phòng ban
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building2 className="h-5 w-5" />
            Cơ cấu tổ chức
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {departments.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-semibold mb-2">Chưa có phòng ban nào</h3>
              <p className="text-muted-foreground text-center">
                Tạo phòng ban đầu tiên để bắt đầu xây dựng cơ cấu tổ chức
              </p>
            </div>
          ) : (
            <div className="border rounded-lg">
              {departments.map((department) => (
                <DepartmentTreeItem key={department.id} department={department} />
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <DepartmentDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        department={editingDepartment}
        onSuccess={handleSave}
      />
    </div>
  );
}
