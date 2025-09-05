# HRM Frontend

Frontend application cho hệ thống quản lý nhân sự (HRM) được xây dựng với React, TypeScript, Vite và Tailwind CSS.

## Tính năng chính

- **Authentication**: Đăng nhập/đăng ký với JWT
- **Role-based Access**: Phân quyền theo vai trò (Employee, Manager, HRManager, Admin)
- **User Management**: Quản lý và phê duyệt tài khoản người dùng (dành cho HR/Admin)
- **Responsive Design**: Giao diện thích ứng với mobile và desktop
- **Toast Notifications**: Thông báo thông minh với Sonner
- **Modern UI**: Sử dụng shadcn/ui components

## Tech Stack

- **React 18** với TypeScript
- **Vite** - Build tool
- **React Router** - Client-side routing
- **Tailwind CSS** - Styling
- **shadcn/ui** - UI Components
- **Sonner** - Toast notifications
- **Axios** - HTTP client
- **Lucide React** - Icons

## Cài đặt và chạy

### 1. Cài đặt dependencies

```bash
npm install
```

### 2. Cấu hình Environment

Tạo file `.env.local`:

```env
# API Configuration
VITE_API_BASE_URL=https://localhost:7093/api

# App Configuration
VITE_APP_NAME="HRM System"
VITE_APP_VERSION="1.0.0"
```

### 3. Chạy development server

```bash
npm run dev
```

Application sẽ chạy tại: http://localhost:5173

### 4. Build cho production

```bash
npm run build
```

## Authentication Flow

### 1. Đăng ký
- Người dùng đăng ký tài khoản mới
- Người đầu tiên tự động trở thành **Admin**
- Các người dùng tiếp theo cần chờ HR/Admin phê duyệt

### 2. Đăng nhập
- Xác thực với email/password
- Nhận JWT token
- Token được lưu trong localStorage
- Auto-redirect dựa trên role

### 3. Phân quyền
- **Employee**: Quyền cơ bản
- **Manager**: Quản lý nhân viên  
- **HR Manager**: Phê duyệt tài khoản + quản lý HR
- **Admin**: Toàn quyền hệ thống

## API Integration

### Authentication APIs
- `POST /auth/login` - Đăng nhập
- `POST /auth/register` - Đăng ký
- `GET /auth/me` - Lấy thông tin user hiện tại
- `GET /auth/pending-users` - Lấy danh sách user chờ duyệt
- `POST /auth/approve-user/{id}` - Phê duyệt user
- `POST /auth/reject-user/{id}` - Từ chối user

## Scripts

```bash
npm run dev          # Start development server
npm run build        # Build for production  
npm run preview      # Preview production build
npm run lint         # Run ESLint
```

Để biết thêm thông tin về backend API, xem [Backend README](../backend/README.md)
