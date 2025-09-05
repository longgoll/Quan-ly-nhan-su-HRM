# Frontend Employee Management - HRM System

## 📁 Cấu trúc dự án

```
frontend/src/
├── api/                    # API clients
│   ├── employee.ts        # Employee API calls
│   ├── department.ts      # Department API calls
│   └── position.ts        # Position API calls
├── components/            # React components
│   ├── employees/         # Employee-related components
│   │   ├── EmployeeList.tsx
│   │   ├── EmployeeTable.tsx
│   │   ├── EmployeeDialog.tsx
│   │   └── EmployeeDetailDialog.tsx
│   ├── departments/       # Department-related components
│   │   └── DepartmentDialog.tsx
│   └── ui/               # Shared UI components
│       └── delete-confirm-dialog.tsx
├── hooks/                # Custom React hooks
│   ├── useEmployees.ts   # Employee management hook
│   ├── useDepartments.ts # Department management hook
│   └── usePositions.ts   # Position management hook
├── pages/                # Page components
│   ├── EmployeeManagementPage.tsx
│   └── DepartmentManagementPage.tsx
├── types/                # TypeScript type definitions
│   └── employee.ts       # Employee, Department, Position types
└── config/               # Configuration
    └── api.ts            # Axios configuration
```

## 🚀 Tính năng đã triển khai

### 1. **Quản lý Nhân viên** (`/employees`)
- ✅ Danh sách nhân viên với pagination
- ✅ Tìm kiếm nhân viên theo tên
- ✅ Lọc theo phòng ban, vị trí, trạng thái
- ✅ Thêm nhân viên mới với form đầy đủ
- ✅ Chỉnh sửa thông tin nhân viên
- ✅ Xem chi tiết nhân viên
- ✅ Xóa nhân viên với xác nhận
- ✅ Hiển thị thống kê tổng quan

**Thông tin nhân viên bao gồm:**
- Thông tin cá nhân: Họ tên, email, SĐT, ngày sinh, địa chỉ, CMND/CCCD
- Thông tin công việc: Phòng ban, vị trí, ngày vào làm, trạng thái
- Mã nhân viên tự động tạo theo backend

### 2. **Quản lý Phòng ban** (`/departments`)
- ✅ Danh sách phòng ban
- ✅ Thêm phòng ban mới
- ✅ Chỉnh sửa phòng ban
- ✅ Xóa phòng ban với xác nhận
- ✅ Hỗ trợ cấu trúc phòng ban cha-con
- ✅ Quản lý trạng thái hoạt động

### 3. **Quản lý Vị trí** (Hooks & API đã sẵn sàng)
- ✅ API và hooks đã được tạo
- ✅ Lọc vị trí theo phòng ban
- 🔄 Trang quản lý (chưa tạo UI)

## 🛠️ Công nghệ sử dụng

### Frontend Stack:
- **React 19** + **TypeScript**
- **React Router** cho routing
- **React Hook Form** cho form management
- **Axios** cho API calls
- **date-fns** cho xử lý ngày tháng
- **Sonner** cho toast notifications

### UI Components:
- **Radix UI** primitives
- **Tailwind CSS** cho styling
- **Lucide React** cho icons
- **shadcn/ui** components

## 📋 API Integration

### Employee API:
```typescript
// Get employees with pagination and filters
employeeApi.getEmployees(params?: EmployeeQueryParams)

// CRUD operations
employeeApi.createEmployee(data: CreateEmployeeRequest)
employeeApi.updateEmployee(id: number, data: UpdateEmployeeRequest)
employeeApi.deleteEmployee(id: number)
employeeApi.getEmployee(id: number)
```

### Department API:
```typescript
// Basic CRUD
departmentApi.getDepartments()
departmentApi.createDepartment(data: CreateDepartmentRequest)
departmentApi.updateDepartment(id: number, data: UpdateDepartmentRequest)
departmentApi.deleteDepartment(id: number)

// Special
departmentApi.getDepartmentHierarchy() // Cây phòng ban
```

### Position API:
```typescript
// Basic CRUD
positionApi.getPositions()
positionApi.createPosition(data: CreatePositionRequest)
positionApi.updatePosition(id: number, data: UpdatePositionRequest)
positionApi.deletePosition(id: number)

// Filter by department
positionApi.getPositionsByDepartment(departmentId: number)
```

## 🎯 Hooks Usage

### useEmployees Hook:
```typescript
const {
  employees,        // PaginatedResponse<Employee>
  loading,          // boolean
  error,            // string | null
  fetchEmployees,   // (params?) => Promise<void>
  createEmployee,   // (data) => Promise<boolean>
  updateEmployee,   // (id, data) => Promise<boolean>
  deleteEmployee,   // (id) => Promise<boolean>
  refetch          // () => void
} = useEmployees();
```

### useDepartments Hook:
```typescript
const {
  departments,      // Department[]
  loading,          // boolean
  error,            // string | null
  createDepartment, // (data) => Promise<boolean>
  updateDepartment, // (id, data) => Promise<boolean>
  deleteDepartment, // (id) => Promise<boolean>
  refetch          // () => void
} = useDepartments();
```

## 🔐 Phân quyền

- **Admin**: Toàn quyền
- **HRManager**: Quản lý nhân sự, phòng ban, vị trí
- **Manager**: Xem và quản lý nhân viên trong phòng ban
- **Employee**: Chỉ xem thông tin cá nhân

## 🚧 Chức năng cần bổ sung

### 1. Position Management Page
- Tạo trang quản lý vị trí tương tự như phòng ban
- UI cho CRUD operations

### 2. Employee Features
- Upload avatar nhân viên
- Import/Export Excel
- Lịch sử thay đổi thông tin

### 3. Advanced Features
- Org chart visualization
- Advanced search & filters
- Bulk operations
- Activity logs

## 🎨 UI/UX Features

### Responsive Design:
- Mobile-friendly tables
- Responsive dialogs
- Adaptive layouts

### User Experience:
- Loading states với skeleton
- Toast notifications
- Confirm dialogs cho actions nguy hiểm
- Real-time search với debounce
- Pagination với số trang linh hoạt

### Data Handling:
- Optimistic updates
- Error handling
- Retry mechanisms
- Cache management

## 🔄 Data Flow

```
Component → Hook → API → Backend
    ↓        ↓      ↓       ↓
   UI     Custom  Axios   .NET API
 Updates   Hook   HTTP    + EF Core
           State  Calls   + PostgreSQL
```

## 📝 Form Validation

### Employee Form:
- Required: Họ tên, ngày vào làm
- Optional: Email, SĐT, địa chỉ, CMND/CCCD
- Validation: Email format, ngày hợp lệ
- Dynamic: Vị trí thay đổi theo phòng ban

### Department Form:
- Required: Tên phòng ban
- Optional: Mô tả, phòng ban cha
- Validation: Tránh circular references

## 🌐 Multi-language Support

- Tất cả labels và messages đều bằng tiếng Việt
- Date formatting theo locale Việt Nam
- Số điện thoại format theo chuẩn VN

## 🔧 Configuration

### API Configuration:
```typescript
// config/api.ts
export const api = axios.create({
  baseURL: config.API_BASE_URL, // https://localhost:7093/api
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
});
```

### Environment Variables:
```env
VITE_API_BASE_URL=https://localhost:7093/api
```

## 🚀 Getting Started

1. **Install dependencies:**
   ```bash
   cd frontend
   npm install
   ```

2. **Start development server:**
   ```bash
   npm run dev
   ```

3. **Access application:**
   - Frontend: http://localhost:5173
   - Login với tài khoản đã tạo từ backend

## 📱 Screenshots & Demo

### Employee Management:
- Danh sách nhân viên với search và filter
- Form thêm/sửa nhân viên với tabs
- Chi tiết nhân viên với thông tin đầy đủ

### Department Management:
- Quản lý cấu trúc phòng ban
- Form tạo phòng ban với parent selection
- Trạng thái hoạt động

## 🔮 Roadmap

### Phase 1 (Completed): ✅
- Employee CRUD operations
- Department management
- Basic UI/UX

### Phase 2 (Next):
- Position management page
- File upload integration
- Advanced search

### Phase 3 (Future):
- Reports and analytics
- Org chart visualization
- Mobile app

---

**Lưu ý:** Frontend hiện tại đã hoàn thiện các chức năng cơ bản cho quản lý nhân viên và phòng ban. Tất cả APIs đã được tích hợp và sẵn sàng hoạt động với backend .NET.
