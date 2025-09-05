import React, { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Search, Plus, Filter, Download, Upload } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useEmployees } from '@/hooks/useEmployees';
import { useDepartments } from '@/hooks/useDepartments';
import { usePositions } from '@/hooks/usePositions';
import EmployeeTable from '@/components/employees/EmployeeTable';
import EmployeeDialog from '@/components/employees/EmployeeDialog';
import type { EmployeeQueryParams } from '@/types/employee';

export const EmployeeList: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [filters, setFilters] = useState<EmployeeQueryParams>({
    page: 1,
    pageSize: 10
  });
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  // Use ref to store current filters to avoid stale closure
  const currentFiltersRef = useRef(filters);
  const currentSearchRef = useRef(searchQuery);
  
  // Update refs when state changes
  currentFiltersRef.current = filters;
  currentSearchRef.current = searchQuery;

  const { employees, loading, fetchEmployees } = useEmployees();
  const { departments, fetchDepartments } = useDepartments();
  const { positions, fetchPositions } = usePositions();

  // Memoize query params to prevent unnecessary re-renders
  const queryParams = useMemo(() => ({
    ...filters,
    search: searchQuery || undefined
  }), [filters, searchQuery]);

  // Load employees when component mounts or query params change
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      fetchEmployees(queryParams);
    }, 300); // Debounce search

    return () => clearTimeout(timeoutId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [queryParams]); // Only depend on memoized queryParams

  // Fetch departments and positions when component mounts
  useEffect(() => {
    fetchDepartments();
    fetchPositions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Intentionally excluding functions to prevent infinite loop

  const handleSearch = useCallback((value: string) => {
    setSearchQuery(value);
    setFilters(prev => ({ ...prev, page: 1 })); // Reset to first page
  }, []);

  const handleFilterChange = useCallback((key: keyof EmployeeQueryParams, value: string | number | undefined) => {
    setFilters(prev => ({
      ...prev,
      [key]: value,
      page: 1 // Reset to first page when filter changes
    }));
  }, []);

  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, []);

  // Memoize refresh function to prevent unnecessary re-renders
  const refreshEmployees = useCallback(() => {
    const currentParams = {
      ...currentFiltersRef.current,
      search: currentSearchRef.current || undefined
    };
    fetchEmployees(currentParams);
  }, [fetchEmployees]);

  // Memoize pagination object to prevent unnecessary re-renders
  const paginationProps = useMemo(() => ({
    page: employees?.page || 1,
    pageSize: employees?.pageSize || 10,
    totalItems: employees?.totalItems || 0,
    totalPages: employees?.totalPages || 0
  }), [employees?.page, employees?.pageSize, employees?.totalItems, employees?.totalPages]);

  const clearFilters = useCallback(() => {
    setSearchQuery('');
    setFilters({ page: 1, pageSize: 10 });
  }, []);

  const activeFiltersCount = Object.keys(filters).filter(key => 
    key !== 'page' && key !== 'pageSize' && filters[key as keyof EmployeeQueryParams]
  ).length + (searchQuery ? 1 : 0);

  return !employees ? (
    <div className="flex items-center justify-center min-h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
    </div>
  ) : (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Quản lý nhân viên</h1>
          <p className="text-muted-foreground">
            Quản lý thông tin nhân viên, phòng ban và vị trí công việc
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            Thêm nhân viên
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tổng nhân viên</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{employees?.totalItems || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Đang làm việc</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {employees?.data?.filter(emp => emp.status === 'Active').length || 0}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Phòng ban</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{departments?.length || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Vị trí</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{positions?.length || 0}</div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            {/* Search */}
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Tìm kiếm nhân viên..."
                  value={searchQuery}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="pl-9"
                />
              </div>
            </div>

            {/* Department Filter */}
            <Select
              value={filters.departmentId?.toString() || "all"}
              onValueChange={(value) => 
                handleFilterChange('departmentId', value === "all" ? undefined : parseInt(value))
              }
            >
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Phòng ban" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả phòng ban</SelectItem>
                {departments?.map((dept) => (
                  <SelectItem key={dept.id} value={dept.id.toString()}>
                    {dept.name}
                  </SelectItem>
                )) || []}
              </SelectContent>
            </Select>

            {/* Position Filter */}
            <Select
              value={filters.positionId?.toString() || "all"}
              onValueChange={(value) => 
                handleFilterChange('positionId', value === "all" ? undefined : parseInt(value))
              }
            >
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Vị trí" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả vị trí</SelectItem>
                {positions?.map((pos) => (
                  <SelectItem key={pos.id} value={pos.id.toString()}>
                    {pos.name}
                  </SelectItem>
                )) || []}
              </SelectContent>
            </Select>

            {/* Status Filter */}
            <Select
              value={filters.status || "all"}
              onValueChange={(value) => 
                handleFilterChange('status', value === "all" ? undefined : value)
              }
            >
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="Active">Đang làm việc</SelectItem>
                <SelectItem value="Inactive">Tạm nghỉ</SelectItem>
                <SelectItem value="Terminated">Đã nghỉ việc</SelectItem>
                <SelectItem value="OnLeave">Đang nghỉ phép</SelectItem>
              </SelectContent>
            </Select>

            {/* Clear Filters */}
            {activeFiltersCount > 0 && (
              <Button variant="outline" onClick={clearFilters}>
                <Filter className="h-4 w-4 mr-2" />
                Xóa bộ lọc
                <Badge variant="secondary" className="ml-2">
                  {activeFiltersCount}
                </Badge>
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Employee Table */}
      <EmployeeTable
        data={employees?.data || []}
        loading={loading}
        pagination={paginationProps}
        onPageChange={handlePageChange}
        onRefresh={refreshEmployees}
      />

      {/* Create Employee Dialog */}
      <EmployeeDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
        onSuccess={() => {
          setIsCreateDialogOpen(false);
          refreshEmployees();
        }}
      />
    </div>
  );
};

export default EmployeeList;
