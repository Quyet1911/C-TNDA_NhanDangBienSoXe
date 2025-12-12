-- Script để thiết lập Permissions cho hệ thống phân quyền
-- Chạy script này trong SQL Server Management Studio

USE NhanDienBienSoXe;
GO

-- ===================================================================
-- 1. Thêm các Permissions cần thiết
-- ===================================================================

-- Kiểm tra và thêm quyền RECOGNIZE (Nhận dạng biển số)
IF NOT EXISTS (SELECT 1 FROM pr.Permissions WHERE Code = 'RECOGNIZE')
BEGIN
    INSERT INTO pr.Permissions (Name, Code, Description, Category)
    VALUES (N'Nhận dạng biển số', 'RECOGNIZE', N'Quyền thực hiện nhận dạng biển số xe', 'Recognition');
    PRINT 'Đã thêm quyền RECOGNIZE';
END
ELSE
BEGIN
    PRINT 'Quyền RECOGNIZE đã tồn tại';
END
GO

-- Kiểm tra và thêm quyền VIEW_STATS (Xem thống kê)
IF NOT EXISTS (SELECT 1 FROM pr.Permissions WHERE Code = 'VIEW_STATS')
BEGIN
    INSERT INTO pr.Permissions (Name, Code, Description, Category)
    VALUES (N'Xem thống kê', 'VIEW_STATS', N'Quyền xem các báo cáo và thống kê', 'Statistics');
    PRINT 'Đã thêm quyền VIEW_STATS';
END
ELSE
BEGIN
    PRINT 'Quyền VIEW_STATS đã tồn tại';
END
GO

-- Kiểm tra và thêm quyền MANAGE_CAMERA (Quản lý camera)
IF NOT EXISTS (SELECT 1 FROM pr.Permissions WHERE Code = 'MANAGE_CAMERA')
BEGIN
    INSERT INTO pr.Permissions (Name, Code, Description, Category)
    VALUES (N'Quản lý camera', 'MANAGE_CAMERA', N'Quyền quản lý camera trong hệ thống', 'Camera');
    PRINT 'Đã thêm quyền MANAGE_CAMERA';
END
ELSE
BEGIN
    PRINT 'Quyền MANAGE_CAMERA đã tồn tại';
END
GO

-- ===================================================================
-- 2. Tạo các Role mẫu nếu chưa có
-- ===================================================================

-- Tạo Role "Operator" (Người vận hành - chỉ quét biển số)
IF NOT EXISTS (SELECT 1 FROM pr.Roles WHERE Name = 'Operator')
BEGIN
    INSERT INTO pr.Roles (Name, Description)
    VALUES ('Operator', N'Người vận hành - chỉ có quyền nhận dạng biển số');
    PRINT 'Đã tạo Role Operator';
END
ELSE
BEGIN
    PRINT 'Role Operator đã tồn tại';
END
GO

-- Tạo Role "Manager" (Quản lý - tất cả quyền)
IF NOT EXISTS (SELECT 1 FROM pr.Roles WHERE Name = 'Manager')
BEGIN
    INSERT INTO pr.Roles (Name, Description)
    VALUES ('Manager', N'Quản lý - có tất cả các quyền');
    PRINT 'Đã tạo Role Manager';
END
ELSE
BEGIN
    PRINT 'Role Manager đã tồn tại';
END
GO

-- Tạo Role "Viewer" (Người xem - chỉ xem thống kê)
IF NOT EXISTS (SELECT 1 FROM pr.Roles WHERE Name = 'Viewer')
BEGIN
    INSERT INTO pr.Roles (Name, Description)
    VALUES ('Viewer', N'Người xem - chỉ có quyền xem thống kê');
    PRINT 'Đã tạo Role Viewer';
END
ELSE
BEGIN
    PRINT 'Role Viewer đã tồn tại';
END
GO

-- ===================================================================
-- 3. Gán Permissions cho các Role
-- ===================================================================

DECLARE @OperatorRoleId INT, @ManagerRoleId INT, @ViewerRoleId INT;
DECLARE @RecognizePermId INT, @ViewStatsPermId INT, @ManageCameraPermId INT;

-- Lấy Role IDs
SELECT @OperatorRoleId = RoleId FROM pr.Roles WHERE Name = 'Operator';
SELECT @ManagerRoleId = RoleId FROM pr.Roles WHERE Name = 'Manager';
SELECT @ViewerRoleId = RoleId FROM pr.Roles WHERE Name = 'Viewer';

-- Lấy Permission IDs
SELECT @RecognizePermId = PermissionId FROM pr.Permissions WHERE Code = 'RECOGNIZE';
SELECT @ViewStatsPermId = PermissionId FROM pr.Permissions WHERE Code = 'VIEW_STATS';
SELECT @ManageCameraPermId = PermissionId FROM pr.Permissions WHERE Code = 'MANAGE_CAMERA';

-- Gán quyền RECOGNIZE cho Role Operator
IF NOT EXISTS (SELECT 1 FROM pr.RolePermissions WHERE RoleId = @OperatorRoleId AND PermissionId = @RecognizePermId)
BEGIN
    INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
    VALUES (@OperatorRoleId, @RecognizePermId, GETUTCDATE());
    PRINT 'Đã gán quyền RECOGNIZE cho Role Operator';
END

-- Gán quyền VIEW_STATS cho Role Viewer
IF NOT EXISTS (SELECT 1 FROM pr.RolePermissions WHERE RoleId = @ViewerRoleId AND PermissionId = @ViewStatsPermId)
BEGIN
    INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
    VALUES (@ViewerRoleId, @ViewStatsPermId, GETUTCDATE());
    PRINT 'Đã gán quyền VIEW_STATS cho Role Viewer';
END

-- Gán TẤT CẢ các quyền cho Role Manager
IF NOT EXISTS (SELECT 1 FROM pr.RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @RecognizePermId)
BEGIN
    INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
    VALUES (@ManagerRoleId, @RecognizePermId, GETUTCDATE());
    PRINT 'Đã gán quyền RECOGNIZE cho Role Manager';
END

IF NOT EXISTS (SELECT 1 FROM pr.RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @ViewStatsPermId)
BEGIN
    INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
    VALUES (@ManagerRoleId, @ViewStatsPermId, GETUTCDATE());
    PRINT 'Đã gán quyền VIEW_STATS cho Role Manager';
END

IF NOT EXISTS (SELECT 1 FROM pr.RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @ManageCameraPermId)
BEGIN
    INSERT INTO pr.RolePermissions (RoleId, PermissionId, GrantedAt)
    VALUES (@ManagerRoleId, @ManageCameraPermId, GETUTCDATE());
    PRINT 'Đã gán quyền MANAGE_CAMERA cho Role Manager';
END
GO

-- ===================================================================
-- 4. Xem kết quả
-- ===================================================================

PRINT '';
PRINT '===================================================================';
PRINT 'DANH SÁCH PERMISSIONS:';
PRINT '===================================================================';
SELECT PermissionId, Name, Code, Category, Description
FROM pr.Permissions
ORDER BY Category, Name;

PRINT '';
PRINT '===================================================================';
PRINT 'DANH SÁCH ROLES VÀ PERMISSIONS:';
PRINT '===================================================================';
SELECT
    r.Name AS RoleName,
    p.Code AS PermissionCode,
    p.Name AS PermissionName,
    rp.GrantedAt
FROM pr.Roles r
LEFT JOIN pr.RolePermissions rp ON r.RoleId = rp.RoleId
LEFT JOIN pr.Permissions p ON rp.PermissionId = p.PermissionId
ORDER BY r.Name, p.Code;

PRINT '';
PRINT '===================================================================';
PRINT 'Setup hoàn tất!';
PRINT '===================================================================';
PRINT '';
PRINT 'CÁC ROLE ĐÃ TẠO:';
PRINT '- Admin: Có tất cả quyền (tự động bypass phân quyền)';
PRINT '- Manager: Có RECOGNIZE + VIEW_STATS + MANAGE_CAMERA';
PRINT '- Operator: Chỉ có RECOGNIZE (quét biển số)';
PRINT '- Viewer: Chỉ có VIEW_STATS (xem thống kê)';
PRINT '';
PRINT 'ĐỂ GÁN ROLE CHO USER:';
PRINT '1. Vào trang Admin > Users';
PRINT '2. Chọn Edit user';
PRINT '3. Chọn Role phù hợp';
PRINT '';
GO
