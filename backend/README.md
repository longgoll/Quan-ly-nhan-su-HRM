# HRM Backend API

Đây là backend API cho hệ thống quản lý nhân sự (HRM) được xây dựng với .NET 9 và PostgreSQL.

## Tính năng chính

- **Đăng ký/Đăng nhập** với JWT Authentication
- **Quản lý quyền hạn** theo role (Employee, Manager, HRManager, Admin)
- **Hệ thống phê duyệt** tài khoản mới
- **Người đăng ký đầu tiên tự động trở thành Admin**

## Cấu trúc thư mục

```
backend/
├── Controllers/          # API Controllers
├── Models/              # Data Models
├── DTOs/                # Data Transfer Objects
├── Services/            # Business Logic Services
├── Data/                # Database Context
├── Configurations/      # Configuration Classes
├── Migrations/          # Entity Framework Migrations
└── Properties/          # Launch Settings
```

## Cài đặt và cấu hình

### 1. Cài đặt PostgreSQL

Đảm bảo bạn đã cài đặt PostgreSQL và tạo database.

### 2. Cấu hình Connection String

Cập nhật connection string trong `appsettings.json` và `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HRM_DB;Username=postgres;Password=your_password"
  }
}
```

### 3. Cấu hình JWT

Cập nhật JWT settings trong appsettings:

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "HRM-Backend",
    "Audience": "HRM-Frontend",
    "ExpirationInMinutes": 1440
  }
}
```

### 4. Chạy Migration

```bash
# Tạo database từ migration
dotnet ef database update
```

### 5. Chạy ứng dụng

```bash
dotnet run
```

API sẽ chạy tại: https://localhost:7093

## API Endpoints

### Authentication

| Method | Endpoint | Mô tả | Yêu cầu Auth |
|--------|----------|--------|--------------|
| POST | `/api/auth/register` | Đăng ký tài khoản mới | Không |
| POST | `/api/auth/login` | Đăng nhập | Không |
| GET | `/api/auth/me` | Lấy thông tin user hiện tại | Có |
| GET | `/api/auth/pending-users` | Lấy danh sách user chờ duyệt | HR/Admin |
| POST | `/api/auth/approve-user/{id}` | Phê duyệt user | HR/Admin |
| POST | `/api/auth/reject-user/{id}` | Từ chối user | HR/Admin |

### Mẫu Request/Response

#### Đăng ký
```json
POST /api/auth/register
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "fullName": "John Doe",
  "phoneNumber": "0123456789"
}
```

#### Đăng nhập
```json
POST /api/auth/login
{
  "email": "john@example.com",
  "password": "password123"
}
```

#### Response đăng nhập thành công
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "username": "johndoe",
      "email": "john@example.com",
      "fullName": "John Doe",
      "role": "Admin",
      "isActive": true,
      "isApproved": true
    }
  }
}
```

## Quy trình làm việc

### 1. Đăng ký người dùng đầu tiên
- Người đăng ký đầu tiên tự động trở thành **Admin** và được phê duyệt
- Có quyền quản lý toàn bộ hệ thống

### 2. Đăng ký người dùng tiếp theo
- Tài khoản mới sẽ có role **Employee**
- Cần chờ **HR Manager** hoặc **Admin** phê duyệt
- Chưa thể đăng nhập cho đến khi được phê duyệt

### 3. Phê duyệt tài khoản
- HR Manager/Admin có thể xem danh sách tài khoản chờ duyệt
- Có thể phê duyệt hoặc từ chối tài khoản

## Roles và Permissions

| Role | Mô tả | Quyền hạn |
|------|-------|-----------|
| **Employee** | Nhân viên | Cơ bản |
| **Manager** | Quản lý | Quản lý nhân viên |
| **HRManager** | Quản lý HR | Phê duyệt tài khoản, quản lý HR |
| **Admin** | Quản trị viên | Toàn quyền |

## Cấu trúc Database

### Bảng Users
- `Id` (Primary Key)
- `Username` (Unique)
- `Email` (Unique)
- `PasswordHash`
- `FullName`
- `PhoneNumber`
- `Role` (Enum)
- `IsActive`
- `IsApproved`
- `CreatedAt`
- `UpdatedAt`
- `ApprovedById` (Foreign Key)

## Bảo mật

- **JWT Token**: Xác thực và phân quyền
- **BCrypt**: Hash password
- **CORS**: Configured for cross-origin requests
- **Role-based Authorization**: Phân quyền theo vai trò

## Phát triển

### Thêm Migration mới
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Xóa Migration
```bash
dotnet ef migrations remove
```

### Reset Database
```bash
dotnet ef database drop
dotnet ef database update
```

## Lỗi thường gặp

1. **Connection String**: Đảm bảo PostgreSQL đang chạy và connection string đúng
2. **JWT Secret**: SecretKey phải có ít nhất 32 ký tự
3. **Migration**: Chạy `dotnet ef database update` sau khi thay đổi model
4. **CORS**: Đã cấu hình AllowAll cho development

## Để sử dụng:
Cài đặt PostgreSQL và cập nhật connection string trong appsettings.json
Chạy migration: dotnet ef database update
Chạy ứng dụng: dotnet run
API Documentation có sẵn tại swagger khi chạy development

## Liên hệ

Nếu có vấn đề gì, vui lòng tạo issue hoặc liên hệ team phát triển.

## Ghi chú Database
- Sử dụng PostgreSQL
Remove-Item -Path "Migrations\*" -Force
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update

##  Các bảng đã được tạo trong database:
Users - Quản lý người dùng
Employees - Thông tin nhân viên
Departments - Phòng ban
Positions - Chức vụ
EmployeeContracts - Hợp đồng nhân viên
WorkHistories - Lịch sử công việc
Rewards & Disciplines - Khen thưởng & kỷ luật
EmployeeDocuments - Tài liệu nhân viên
WorkShifts & EmployeeShiftAssignments - Ca làm việc
Attendances & AttendanceDetails - Chấm công
LeavePolicies & LeaveRequests - Quản lý nghỉ phép
PublicHolidays - Ngày lễ