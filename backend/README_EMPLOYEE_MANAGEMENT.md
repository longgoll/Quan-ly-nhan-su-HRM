# HRM - Human Resource Management System

## Tổng quan hệ thống quản lý nhân viên

Hệ thống HRM được phát triển để quản lý toàn diện thông tin nhân viên, bao gồm:

### Chức năng chính:

#### 1. Quản lý thông tin nhân viên
- **Thông tin cá nhân**: Họ tên, ngày sinh, địa chỉ, liên hệ, CMND/CCCD, tình trạng hôn nhân
- **Thông tin công việc**: Phòng ban, vị trí, cấp bậc, người quản lý trực tiếp
- **Mã nhân viên**: Tự động tạo mã nhân viên theo năm (VD: EMP2025001)

#### 2. Quản lý phòng ban và vị trí
- Tạo và quản lý cấu trúc phòng ban theo cây phân cấp
- Định nghĩa các vị trí công việc với mô tả chi tiết
- Phân quyền quản lý theo từng phòng ban

#### 3. Quản lý hợp đồng lao động
- Lưu trữ thông tin các lần ký, gia hạn, chấm dứt hợp đồng
- Upload và quản lý file hợp đồng
- Theo dõi trạng thái hợp đồng

#### 4. Quản lý lịch sử công tác
- Ghi nhận các thay đổi: thăng chức, chuyển phòng ban, tăng lương
- Theo dõi lịch sử phê duyệt
- Báo cáo quá trình phát triển nghề nghiệp

#### 5. Quản lý khen thưởng
- Ghi nhận các loại khen thưởng: thành tích, tiền thưởng, bằng khen
- Upload quyết định khen thưởng
- Theo dõi lịch sử khen thưởng của nhân viên

#### 6. Quản lý kỷ luật
- Ghi nhận các hình thức kỷ luật: cảnh cáo, khiển trách, phạt tiền, đình chỉ
- Upload quyết định kỷ luật
- Theo dõi thời hạn hiệu lực của kỷ luật

#### 7. Quản lý tài liệu
- Upload và lưu trữ tài liệu nhân viên: CV, bằng cấp, chứng chỉ, ảnh
- Phân loại tài liệu theo từng loại
- Download và xem tài liệu

### Công nghệ sử dụng:

#### Backend (.NET 9.0)
- **Framework**: ASP.NET Core Web API
- **Database**: PostgreSQL với Entity Framework Core
- **Authentication**: JWT Bearer Token
- **File Storage**: MinIO Object Storage
- **Mapping**: AutoMapper
- **Documentation**: OpenAPI/Swagger

#### Packages chính:
- `Microsoft.EntityFrameworkCore` - ORM
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `BCrypt.Net-Next` - Password hashing
- `Minio` - Object storage client
- `AutoMapper` - Object mapping

### Cấu trúc Database:

#### Bảng chính:
1. **Users** - Thông tin tài khoản người dùng
2. **Employees** - Thông tin chi tiết nhân viên
3. **Departments** - Phòng ban
4. **Positions** - Vị trí công việc
5. **EmployeeContracts** - Hợp đồng lao động
6. **WorkHistories** - Lịch sử công tác
7. **Rewards** - Khen thưởng
8. **Disciplines** - Kỷ luật
9. **EmployeeDocuments** - Tài liệu nhân viên

### API Endpoints:

#### Authentication
- `POST /api/auth/register` - Đăng ký tài khoản
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/approve/{userId}` - Phê duyệt tài khoản

#### Employee Management
- `GET /api/employees` - Danh sách nhân viên
- `GET /api/employees/{id}` - Chi tiết nhân viên
- `POST /api/employees` - Tạo nhân viên mới
- `PUT /api/employees/{id}` - Cập nhật thông tin nhân viên
- `DELETE /api/employees/{id}` - Xóa nhân viên

#### Department Management
- `GET /api/departments` - Danh sách phòng ban
- `POST /api/departments` - Tạo phòng ban mới
- `PUT /api/departments/{id}` - Cập nhật phòng ban
- `DELETE /api/departments/{id}` - Xóa phòng ban

#### Position Management
- `GET /api/positions` - Danh sách vị trí
- `POST /api/positions` - Tạo vị trí mới
- `PUT /api/positions/{id}` - Cập nhật vị trí
- `DELETE /api/positions/{id}` - Xóa vị trí

#### Contract Management
- `GET /api/contracts` - Danh sách hợp đồng
- `POST /api/contracts` - Tạo hợp đồng mới
- `PUT /api/contracts/{id}` - Cập nhật hợp đồng
- `POST /api/contracts/{id}/terminate` - Chấm dứt hợp đồng
- `POST /api/contracts/{id}/upload-document` - Upload file hợp đồng

#### Work History Management
- `GET /api/workhistory` - Lịch sử công tác
- `POST /api/workhistory` - Tạo bản ghi lịch sử
- `POST /api/workhistory/{id}/approve` - Phê duyệt thay đổi

#### Reward Management
- `GET /api/rewards` - Danh sách khen thưởng
- `POST /api/rewards` - Tạo khen thưởng mới
- `POST /api/rewards/{id}/approve` - Phê duyệt khen thưởng
- `POST /api/rewards/{id}/upload-document` - Upload quyết định

#### Discipline Management
- `GET /api/disciplines` - Danh sách kỷ luật
- `POST /api/disciplines` - Tạo kỷ luật mới
- `POST /api/disciplines/{id}/approve` - Phê duyệt kỷ luật
- `POST /api/disciplines/{id}/upload-document` - Upload quyết định

#### Document Management
- `GET /api/documents` - Danh sách tài liệu
- `POST /api/documents/upload` - Upload tài liệu
- `GET /api/documents/{id}/download` - Download tài liệu
- `DELETE /api/documents/{id}` - Xóa tài liệu

### Phân quyền người dùng:

1. **Employee** - Nhân viên: Xem thông tin cá nhân, upload tài liệu
2. **Manager** - Quản lý: Xem thông tin nhân viên trong phòng ban
3. **HRManager** - Quản lý HR: Toàn quyền quản lý nhân sự
4. **Admin** - Quản trị viên: Toàn quyền hệ thống

### Cài đặt và chạy:

#### 1. Yêu cầu hệ thống:
- .NET 9.0 SDK
- PostgreSQL
- MinIO Server
- Docker (tùy chọn)

#### 2. Cấu hình:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HRM_DB;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "HRM-Backend",
    "Audience": "HRM-Frontend",
    "ExpirationInMinutes": 1440
  },
  "MinIOSettings": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Secure": false,
    "BucketName": "hrm-documents",
    "Region": "us-east-1"
  }
}
```

#### 3. Chạy với Docker:
```bash
cd docker-dev
docker-compose up -d
```

#### 4. Chạy thủ công:
```bash
cd backend
dotnet ef database update
dotnet run
```

### Truy cập:
- **API Documentation**: http://localhost:5000/swagger
- **MinIO Console**: http://localhost:9001 (minioadmin/minioadmin)
- **Database**: localhost:5432

### Tính năng nổi bật:

1. **Tự động tạo mã nhân viên** theo năm
2. **Upload/Download file** với MinIO
3. **Quản lý phân cấp** phòng ban và quản lý
4. **Theo dõi lịch sử** thay đổi nhân sự
5. **Phê duyệt** các quyết định nhân sự
6. **Bảo mật** với JWT và phân quyền
7. **Tìm kiếm và lọc** dữ liệu linh hoạt

### Hướng phát triển:
- Báo cáo và thống kê
- Quản lý chấm công
- Tính lương
- Đào tạo và phát triển
- Đánh giá hiệu suất
- Mobile app
