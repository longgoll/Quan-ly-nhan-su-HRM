# Hệ thống Quản lý Ca làm việc, Chấm công và Nghỉ phép

## Tổng quan

Hệ thống HRM đã được mở rộng với 3 module chính:

### 1. 🕒 Quản lý Ca làm việc (Work Schedule Management)
- **Quản lý ca làm việc đa dạng**: Ca cố định, xoay ca, theo dự án, ca linh hoạt
- **Phân ca tự động**: Gán ca cho nhân viên theo phòng ban/vị trí
- **Lịch làm việc linh hoạt**: Tạo lịch theo tuần/tháng với bulk operations
- **Hỗ trợ ca đêm**: Xử lý ca qua ngày với tính toán thời gian chính xác

### 2. ⏰ Chấm công thông minh (Attendance Management)
- **Check-in/Check-out GPS**: Theo dõi vị trí chấm công với độ chính xác cao
- **Tính toán tự động**: Giờ làm việc, làm thêm, đi trễ, về sớm
- **Chấm công linh hoạt**: Hỗ trợ break time, multiple device types
- **Báo cáo chi tiết**: Daily, monthly, department summaries

### 3. 🏖️ Quản lý Nghỉ phép (Leave Management)
- **Chính sách nghỉ phép đa dạng**: 12+ loại nghỉ phép khác nhau
- **Workflow phê duyệt**: Multi-level approval với tracking
- **Tính toán tự động**: Số ngày phép còn lại, carry forward
- **Lịch nghỉ tập thể**: Public holidays theo phòng ban

## Tính năng nổi bật

### ✨ Cải tiến so với yêu cầu ban đầu:

1. **GPS Tracking**: Chấm công với vị trí địa lý
2. **Multi-device Support**: Hỗ trợ chấm công từ nhiều thiết bị
3. **Photo Verification**: Chụp ảnh khi check-in/out
4. **Real-time Dashboard**: Theo dõi real-time attendance
5. **Advanced Reporting**: Báo cáo đa chiều với analytics
6. **Flexible Workflow**: Workflow phê duyệt có thể tuỳ chỉnh
7. **Holiday Management**: Quản lý ngày lễ theo location
8. **Leave Calendar**: Visualize leave requests trên calendar

## Cấu trúc Database

### Work Schedule Tables
```
WorkShifts - Định nghĩa ca làm việc
├── EmployeeShiftAssignments - Phân ca cho nhân viên  
└── WorkSchedules - Lịch làm việc chi tiết
```

### Attendance Tables
```
Attendances - Bản ghi chấm công chính
├── AttendanceDetails - Chi tiết từng lần check-in/out
└── AttendanceSummaries - Báo cáo tháng
```

### Leave Management Tables
```
LeavePolicies - Chính sách nghỉ phép
├── EmployeeLeaveBalances - Số dư nghỉ phép của NV
├── LeaveRequests - Đơn xin nghỉ phép
├── LeaveApprovalWorkflows - Workflow phê duyệt
└── PublicHolidays - Ngày nghỉ lễ
```

## API Endpoints

### 🔗 Work Schedule APIs

#### Work Shifts
- `GET /api/workschedule/shifts` - Danh sách ca làm việc
- `POST /api/workschedule/shifts` - Tạo ca mới
- `PUT /api/workschedule/shifts/{id}` - Cập nhật ca
- `DELETE /api/workschedule/shifts/{id}` - Xoá ca

#### Shift Assignments  
- `GET /api/workschedule/shift-assignments` - Danh sách phân ca
- `POST /api/workschedule/shift-assignments` - Phân ca cho NV
- `PUT /api/workschedule/shift-assignments/{id}` - Cập nhật phân ca

#### Work Schedules
- `GET /api/workschedule/schedules` - Lịch làm việc
- `POST /api/workschedule/schedules` - Tạo lịch
- `POST /api/workschedule/schedules/bulk` - Tạo lịch hàng loạt

### ⏰ Attendance APIs

#### Check-in/Check-out
- `POST /api/attendance/check-in` - Chấm công vào
- `POST /api/attendance/check-out` - Chấm công ra  
- `POST /api/attendance/break-time` - Ghi nhận giờ nghỉ

#### Attendance Management
- `GET /api/attendance` - Danh sách chấm công
- `GET /api/attendance/today` - Chấm công hôm nay
- `PUT /api/attendance/{id}/approve` - Phê duyệt chấm công

#### Reports
- `GET /api/attendance/summary/monthly` - Báo cáo tháng
- `GET /api/attendance/reports/daily` - Báo cáo ngày
- `GET /api/attendance/reports/employee-history/{id}` - Lịch sử NV

### 🏖️ Leave Management APIs

#### Leave Policies
- `GET /api/leave/policies` - Chính sách nghỉ phép
- `POST /api/leave/policies` - Tạo chính sách mới
- `PUT /api/leave/policies/{id}` - Cập nhật chính sách

#### Leave Balances
- `GET /api/leave/balances` - Số dư nghỉ phép
- `POST /api/leave/balances/initialize/{year}` - Khởi tạo số dư năm
- `POST /api/leave/balances/adjust` - Điều chỉnh số dư

#### Leave Requests
- `GET /api/leave/requests` - Danh sách đơn nghỉ phép
- `POST /api/leave/requests` - Tạo đơn mới
- `POST /api/leave/requests/{id}/cancel` - Huỷ đơn

#### Approvals
- `POST /api/leave/approvals` - Phê duyệt đơn
- `GET /api/leave/approvals/pending` - Đơn chờ phê duyệt

#### Public Holidays
- `GET /api/leave/holidays` - Ngày nghỉ lễ
- `POST /api/leave/holidays` - Thêm ngày nghỉ lễ

#### Reports & Calendar
- `GET /api/leave/calendar` - Lịch nghỉ phép
- `GET /api/leave/history/{employeeId}/{year}` - Lịch sử nghỉ phép

## Quy trình sử dụng

### 🚀 Khởi tạo hệ thống

1. **Thiết lập ca làm việc**:
```bash
POST /api/workschedule/shifts
{
  "name": "Ca hành chính",
  "type": "Fixed",
  "startTime": "08:00:00",
  "endTime": "17:00:00",
  "workingHours": 8,
  "flexibleMinutes": 15
}
```

2. **Tạo chính sách nghỉ phép**:
```bash
POST /api/leave/policies
{
  "name": "Phép năm",
  "leaveType": "Annual", 
  "annualAllowanceDays": 12,
  "maxCarryForwardDays": 3,
  "minAdvanceNoticeDays": 1
}
```

3. **Khởi tạo số dư nghỉ phép**:
```bash
POST /api/leave/balances/initialize/2025
```

### 📅 Quản lý lịch làm việc

1. **Phân ca cho nhân viên**:
```bash
POST /api/workschedule/shift-assignments
{
  "employeeId": 1,
  "workShiftId": 1,
  "effectiveFrom": "2025-01-01",
  "isDefaultShift": true
}
```

2. **Tạo lịch hàng loạt**:
```bash
POST /api/workschedule/schedules/bulk
{
  "employeeIds": [1, 2, 3],
  "workShiftId": 1,
  "startDate": "2025-01-01",
  "endDate": "2025-01-31",
  "selectedDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
  "skipHolidays": true
}
```

### ⏰ Quy trình chấm công

1. **Nhân viên check-in**:
```bash
POST /api/attendance/check-in
{
  "checkInTime": "2025-01-15T08:00:00Z",
  "latitude": 21.028511,
  "longitude": 105.804817,
  "location": "Văn phòng Hà Nội",
  "deviceType": "Mobile"
}
```

2. **Check-out**:
```bash
POST /api/attendance/check-out
{
  "checkOutTime": "2025-01-15T17:00:00Z",
  "latitude": 21.028511,
  "longitude": 105.804817,
  "location": "Văn phòng Hà Nội"
}
```

3. **Quản lý phê duyệt**:
```bash
PUT /api/attendance/{id}/approve
{
  "status": "Approved",
  "managerNotes": "Đã xác nhận làm thêm giờ"
}
```

### 🏖️ Quy trình nghỉ phép

1. **Nhân viên tạo đơn**:
```bash
POST /api/leave/requests
{
  "leavePolicyId": 1,
  "startDate": "2025-02-01",
  "endDate": "2025-02-03", 
  "reason": "Nghỉ phép cá nhân",
  "coverEmployeeId": 2
}
```

2. **Quản lý phê duyệt**:
```bash
POST /api/leave/approvals
{
  "leaveRequestId": 1,
  "status": "Approved",
  "comments": "Đồng ý nghỉ phép"
}
```

## Business Rules & Validations

### 💼 Ca làm việc
- Ca đêm tự động tính qua ngày mới
- Flexible minutes cho phép check-in/out linh hoạt
- Rotation cycle tự động chuyển ca theo chu kỳ
- Không được conflict giữa các ca

### ⏱️ Chấm công  
- Phải có schedule hoặc default shift mới check-in được
- GPS tracking với tolerance distance
- Tự động tính late/early/overtime dựa trên shift
- Break time không tính vào working hours
- Approval workflow cho các case đặc biệt

### 🗓️ Nghỉ phép
- Kiểm tra số dư trước khi tạo đơn
- Minimum advance notice theo policy
- Không conflict với đơn khác đã approved
- Automatic deduction từ balance khi approved
- Carry forward rules theo policy
- Multi-level approval workflow

## Dashboard & Analytics

### 📊 Metrics chính:
- **Attendance Rate**: Tỷ lệ chấm công đúng giờ
- **Overtime Trends**: Xu hướng làm thêm giờ  
- **Leave Utilization**: Tỷ lệ sử dụng phép
- **Department Comparison**: So sánh giữa các phòng ban
- **Real-time Status**: Ai đang online/offline

### 📈 Reports:
- Daily attendance summary
- Monthly department reports  
- Employee leave history
- Overtime analysis
- Late/Early trends
- Leave calendar view

## Security & Permissions

### 🔐 Role-based Access:
- **Employee**: Chỉ xem/chỉnh sửa dữ liệu của mình
- **Manager**: Quản lý team + approval quyền
- **HR**: Full access + policy management
- **Admin**: System configuration

### 🛡️ Data Protection:
- GPS data encryption
- Photo verification với privacy
- Audit trail cho tất cả changes
- GDPR compliance ready

## Performance Optimization

### ⚡ Database:
- Proper indexing cho date ranges
- Partitioning cho attendance data
- Archiving old records
- Connection pooling

### 🚀 API:
- Pagination cho large datasets
- Caching frequently accessed data
- Background jobs cho heavy operations
- Rate limiting protection

## Deployment & Monitoring

### 🐳 Docker Setup:
```yaml
services:
  hrm-backend:
    build: ./backend
    environment:
      - ConnectionStrings__DefaultConnection=...
      - JwtSettings__SecretKey=...
    depends_on:
      - postgres
      - redis
```

### 📱 Mobile App Integration:
- REST APIs ready cho mobile
- Offline capability với sync
- Push notifications
- Biometric authentication support

---

## 🎯 Kết luận

Hệ thống đã được nâng cấp toàn diện với các tính năng:

✅ **Quản lý ca làm việc linh hoạt** - Hỗ trợ mọi loại ca
✅ **Chấm công thông minh** - GPS + Photo verification  
✅ **Nghỉ phép tự động** - Workflow + Policy engine
✅ **Analytics mạnh mẽ** - Real-time dashboard
✅ **Mobile-ready** - APIs optimized cho mobile
✅ **Enterprise-grade** - Security + Performance

Hệ thống sẵn sàng triển khai production với khả năng scale cao và đáp ứng mọi yêu cầu doanh nghiệp hiện đại! 🚀
