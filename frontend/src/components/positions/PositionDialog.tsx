import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { positionApi } from '@/api/position';
import type { Position, Department, CreatePositionRequest, UpdatePositionRequest } from '@/types/employee';
import { toast } from 'sonner';

interface PositionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  position?: Position | null;
  departments: Department[];
  onSave: () => void;
}

export function PositionDialog({ 
  open, 
  onOpenChange, 
  position, 
  departments, 
  onSave 
}: PositionDialogProps) {
  const [formData, setFormData] = useState({
    title: '',
    code: '',
    description: '',
    requirements: '',
    departmentId: '',
    level: 1,
    minSalary: '',
    maxSalary: '',
    isActive: true
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (position) {
      setFormData({
        title: position.title,
        code: position.code || '',
        description: position.description || '',
        requirements: position.requirements || '',
        departmentId: position.departmentId?.toString() || '',
        level: position.level,
        minSalary: position.minSalary?.toString() || '',
        maxSalary: position.maxSalary?.toString() || '',
        isActive: position.isActive
      });
    } else {
      setFormData({
        title: '',
        code: '',
        description: '',
        requirements: '',
        departmentId: '',
        level: 1,
        minSalary: '',
        maxSalary: '',
        isActive: true
      });
    }
  }, [position, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.title.trim()) {
      toast.error('Vui lòng nhập tên vị trí');
      return;
    }

    setLoading(true);
    try {
      const requestData = {
        title: formData.title.trim(),
        code: formData.code.trim() || undefined,
        description: formData.description.trim() || undefined,
        requirements: formData.requirements.trim() || undefined,
        departmentId: formData.departmentId ? parseInt(formData.departmentId) : undefined,
        level: formData.level,
        minSalary: formData.minSalary ? parseFloat(formData.minSalary) : undefined,
        maxSalary: formData.maxSalary ? parseFloat(formData.maxSalary) : undefined,
        ...(position && { isActive: formData.isActive })
      };

      if (position) {
        await positionApi.updatePosition(position.id, requestData as UpdatePositionRequest);
        toast.success('Đã cập nhật vị trí');
      } else {
        await positionApi.createPosition(requestData as CreatePositionRequest);
        toast.success('Đã tạo vị trí mới');
      }
      
      onSave();
    } catch {
      toast.error(position ? 'Không thể cập nhật vị trí' : 'Không thể tạo vị trí');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: string, value: string | number | boolean) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {position ? 'Chỉnh sửa vị trí' : 'Thêm vị trí mới'}
          </DialogTitle>
        </DialogHeader>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="title">Tên vị trí *</Label>
              <Input
                id="title"
                value={formData.title}
                onChange={(e) => handleChange('title', e.target.value)}
                placeholder="Nhập tên vị trí"
                required
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="code">Mã vị trí</Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) => handleChange('code', e.target.value)}
                placeholder="Nhập mã vị trí"
                maxLength={10}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="department">Phòng ban</Label>
              <Select 
                value={formData.departmentId} 
                onValueChange={(value) => handleChange('departmentId', value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn phòng ban" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Không phân công</SelectItem>
                  {departments.map((dept) => (
                    <SelectItem key={dept.id} value={dept.id.toString()}>
                      {dept.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="level">Cấp bậc</Label>
              <Select 
                value={formData.level.toString()} 
                onValueChange={(value) => handleChange('level', parseInt(value))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((level) => (
                    <SelectItem key={level} value={level.toString()}>
                      Cấp {level}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="minSalary">Lương tối thiểu (VNĐ)</Label>
              <Input
                id="minSalary"
                type="number"
                value={formData.minSalary}
                onChange={(e) => handleChange('minSalary', e.target.value)}
                placeholder="0"
                min="0"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="maxSalary">Lương tối đa (VNĐ)</Label>
              <Input
                id="maxSalary"
                type="number"
                value={formData.maxSalary}
                onChange={(e) => handleChange('maxSalary', e.target.value)}
                placeholder="0"
                min="0"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Mô tả công việc</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleChange('description', e.target.value)}
              placeholder="Mô tả chi tiết về vị trí công việc"
              rows={3}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="requirements">Yêu cầu</Label>
            <Textarea
              id="requirements"
              value={formData.requirements}
              onChange={(e) => handleChange('requirements', e.target.value)}
              placeholder="Các yêu cầu về kiến thức, kỹ năng, kinh nghiệm"
              rows={3}
            />
          </div>

          {position && (
            <div className="flex items-center space-x-2">
              <Switch
                id="isActive"
                checked={formData.isActive}
                onCheckedChange={(checked) => handleChange('isActive', checked)}
              />
              <Label htmlFor="isActive">Vị trí đang hoạt động</Label>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-4">
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => onOpenChange(false)}
              disabled={loading}
            >
              Hủy
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? 'Đang xử lý...' : (position ? 'Cập nhật' : 'Tạo mới')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
