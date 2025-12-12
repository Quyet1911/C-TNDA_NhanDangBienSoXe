# HƯỚNG DẪN HỆ THỐNG PHÂN QUYỀN

## Tổng quan

Hệ thống đã được cập nhật với logic phân quyền dựa trên **Permission-Based Access Control (PBAC)**. Người dùng sẽ chỉ có thể truy cập các trang mà họ có quyền tương ứng.

## Các quyền trong hệ thống

Có 3 quyền chính:

| Mã quyền | Tên quyền | Mô tả | Trang được phép truy cập |
|----------|-----------|-------|--------------------------|
| `RECOGNIZE` | Nhận dạng biển số | Quyền thực hiện nhận dạng biển số xe | `/Recognition/*` |
| `VIEW_STATS` | Xem thống kê | Quyền xem các báo cáo và thống kê | `/Stats/*` |
| `MANAGE_CAMERA` | Quản lý camera | Quyền quản lý camera trong hệ thống | `/Camera/*` |

## Các Role mẫu

| Role | Quyền | Mô tả |
|------|-------|-------|
| **Admin** | Tất cả | Có quyền truy cập tất cả mọi trang (tự động bypass) |
| **Manager** | RECOGNIZE + VIEW_STATS + MANAGE_CAMERA | Quản lý - có tất cả các quyền |
| **Operator** | RECOGNIZE | Người vận hành - chỉ có quyền quét biển số |
| **Viewer** | VIEW_STATS | Người xem - chỉ có quyền xem thống kê |

## Cách thiết lập

### Bước 1: Chạy SQL Script

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối đến database `NhanDienBienSoXe`
3. Mở file `Setup_Permissions.sql`
4. Chạy script để tạo permissions và roles

### Bước 2: Gán Role cho User

Có 2 cách để gán role cho user:

#### Cách 1: Qua giao diện Admin

1. Đăng nhập với tài khoản Admin
2. Vào **Admin > Users**
3. Chọn **Edit** user cần gán role
4. Chọn role phù hợp từ dropdown
5. Nhấn **Save**

#### Cách 2: Qua SQL

```sql
-- Gán role cho user
DECLARE @UserId INT, @RoleId INT;

-- Lấy UserId
SELECT @UserId = UserId FROM pr.Users WHERE UserName = 'ten_user';

-- Lấy RoleId
SELECT @RoleId = RoleId FROM pr.Roles WHERE Name = 'Operator'; -- hoặc Manager, Viewer

-- Gán role
INSERT INTO pr.UserRoles (UserId, RoleId)
VALUES (@UserId, @RoleId);
```

## Cách hoạt động

### 1. Phân quyền tại Controller

Các controller đã được bảo vệ bằng `[Authorize(Policy = "...")]`:

```csharp
// RecognitionController - yêu cầu quyền RECOGNIZE
[Authorize(Policy = "CanRecognize")]
public class RecognitionController : Controller { ... }

// StatsController - yêu cầu quyền VIEW_STATS
[Authorize(Policy = "CanViewStats")]
public class StatsController : Controller { ... }

// CameraController - yêu cầu quyền MANAGE_CAMERA
[Authorize(Policy = "CanManageCamera")]
public class CameraController : Controller { ... }
```

### 2. Ẩn/hiện menu trong giao diện

Menu trong `_Layout.cshtml` sẽ tự động ẩn/hiện dựa trên quyền của user:

- **Người dùng chỉ có quyền RECOGNIZE**: Chỉ thấy menu "Trang chủ" và "Quét biển số"
- **Người dùng chỉ có quyền VIEW_STATS**: Chỉ thấy menu "Trang chủ" và "Thống kê"
- **Người dùng chỉ có quyền MANAGE_CAMERA**: Chỉ thấy menu "Trang chủ" và "Camera"
- **Admin/Manager**: Thấy tất cả các menu

## Ví dụ sử dụng

### Ví dụ 1: Nhân viên vận hành

**Kịch bản**: Bạn muốn tạo tài khoản cho nhân viên bảo vệ chỉ quét biển số, không xem thống kê và không quản lý camera.

**Giải pháp**:
1. Tạo user mới (hoặc chỉnh sửa user hiện có)
2. Gán role **Operator** cho user đó
3. User chỉ có thể:
   - Vào trang Trang chủ
   - Vào trang Quét biển số
   - Upload ảnh và nhận dạng biển số

### Ví dụ 2: Giám đốc

**Kịch bản**: Bạn muốn tạo tài khoản cho giám đốc chỉ xem thống kê, không quét biển số và không quản lý camera.

**Giải pháp**:
1. Tạo user mới (hoặc chỉnh sửa user hiện có)
2. Gán role **Viewer** cho user đó
3. User chỉ có thể:
   - Vào trang Trang chủ
   - Vào trang Thống kê
   - Xem các báo cáo và biểu đồ

### Ví dụ 3: Quản lý IT

**Kịch bản**: Bạn muốn tạo tài khoản cho quản lý IT có tất cả các quyền.

**Giải pháp**:
1. Tạo user mới (hoặc chỉnh sửa user hiện có)
2. Gán role **Manager** hoặc **Admin** cho user đó
3. User có thể:
   - Truy cập tất cả các trang
   - Quét biển số
   - Xem thống kê
   - Quản lý camera

## Tạo Role và Permission tùy chỉnh

Nếu bạn muốn tạo role hoặc permission mới:

### Tạo Permission mới

```sql
INSERT INTO pr.Permissions (Name, Code, Description, Category)
VALUES (N'Tên quyền', 'PERMISSION_CODE', N'Mô tả quyền', 'Category');
```

### Tạo Role mới

```sql
INSERT INTO pr.Roles (Name, Description)
VALUES ('RoleName', N'Mô tả role');
```

### Gán Permission cho Role

```sql
DECLARE @RoleId INT, @PermissionId INT;

SELECT @RoleId = RoleId FROM pr.Roles WHERE Name = 'RoleName';
SELECT @PermissionId = PermissionId FROM pr.Permissions WHERE Code = 'PERMISSION_CODE';

INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
VALUES (@RoleId, @PermissionId, GETUTCDATE());
```

## Lưu ý quan trọng

1. **Admin luôn có tất cả quyền**: Role "Admin" tự động có quyền truy cập tất cả các trang mà không cần gán permissions.

2. **Một user có thể có nhiều roles**: Nếu user có nhiều roles, họ sẽ có tất cả permissions từ tất cả các roles đó.

3. **Phân quyền cascade**: Nếu xóa role, tất cả permissions của role đó cũng bị xóa (ON DELETE CASCADE).

4. **Session cache**: Sau khi thay đổi permissions, user cần đăng xuất và đăng nhập lại để permissions mới có hiệu lực.

## Kiểm tra quyền của user

```sql
-- Xem tất cả permissions của một user
SELECT DISTINCT
    u.UserName,
    p.Code AS PermissionCode,
    p.Name AS PermissionName
FROM pr.Users u
JOIN pr.UserRoles ur ON u.UserId = ur.UserId
JOIN pr.Roles r ON ur.RoleId = r.RoleId
JOIN pr.RolePermissions rp ON r.RoleId = rp.RoleId
JOIN pr.Permissions p ON rp.PermissionId = p.PermissionId
WHERE u.UserName = 'ten_user';
```

## Troubleshooting

### Vấn đề: User không thấy menu nào cả

**Nguyên nhân**: User chưa được gán role hoặc role không có permissions.

**Giải pháp**:
1. Kiểm tra user có role chưa: `SELECT * FROM pr.UserRoles WHERE UserId = ?`
2. Kiểm tra role có permissions chưa: `SELECT * FROM pr.RolePermissions WHERE RoleId = ?`
3. Gán role hoặc gán permissions cho role

### Vấn đề: Sau khi gán role, user vẫn không vào được trang

**Nguyên nhân**: Session cache chưa được refresh.

**Giải pháp**:
1. Đăng xuất
2. Đăng nhập lại
3. Kiểm tra lại

### Vấn đề: Admin không vào được trang

**Nguyên nhân**: Có thể có lỗi trong code hoặc user không có role "Admin".

**Giải pháp**:
1. Kiểm tra user có role "Admin" chưa: `SELECT * FROM pr.UserRoles ur JOIN pr.Roles r ON ur.RoleId = r.RoleId WHERE ur.UserId = ? AND r.Name = 'Admin'`
2. Nếu chưa có, gán role "Admin"

## Liên hệ

Nếu có vấn đề gì, vui lòng liên hệ với đội ngũ phát triển.
