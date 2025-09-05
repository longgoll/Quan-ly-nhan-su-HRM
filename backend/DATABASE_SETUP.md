# Database Setup Instructions

## 🗄️ Khởi động PostgreSQL Database

Để sử dụng hệ thống HRM mới, bạn cần khởi động database PostgreSQL:

### Option 1: Sử dụng Docker (Recommended)

1. **Khởi động PostgreSQL container**:
```bash
cd d:\HoangLong\Dev\HRM\docker-dev
docker-compose up -d
```

2. **Kiểm tra container đang chạy**:
```bash
docker ps
```

3. **Apply database migrations**:
```bash
cd d:\HoangLong\Dev\HRM\backend
dotnet ef database update
```

### Option 2: Cài đặt PostgreSQL local

1. **Download và cài đặt PostgreSQL**:
   - Tải từ: https://www.postgresql.org/download/windows/
   - Chọn password cho user `postgres`
   - Port mặc định: 5432

2. **Tạo database**:
```sql
CREATE DATABASE hrm_db;
```

3. **Cập nhật connection string** trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hrm_db;Username=postgres;Password=your_password"
  }
}
```

4. **Apply migrations**:
```bash
dotnet ef database update
```

## 🚀 Khởi động ứng dụng

```bash
cd d:\HoangLong\Dev\HRM\backend
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:7067`

## 📊 Swagger API Documentation

Truy cập Swagger UI để test APIs:
- URL: `https://localhost:7067/swagger`

## 🔧 Troubleshooting

### Lỗi kết nối PostgreSQL:
```
Failed to connect to 127.0.0.1:5432 - No connection could be made because the target machine actively refused it
```

**Giải pháp**:
1. Kiểm tra PostgreSQL service đang chạy
2. Kiểm tra port 5432 có available không
3. Verify connection string trong appsettings.json

### Kiểm tra PostgreSQL service (Windows):
```powershell
Get-Service postgresql*
```

### Khởi động service nếu stopped:
```powershell
Start-Service postgresql-x64-XX  # XX là version number
```
