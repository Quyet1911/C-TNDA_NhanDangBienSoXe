using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(AppDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetUserPermissionCodesAsync(string userName)
        {
            // Query rõ ràng với explicit joins
            var permissionCodes = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.UserId equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.RoleId
                join rp in _context.RolePermissions on r.RoleId equals rp.RoleId
                join p in _context.Permissions on rp.PermissionId equals p.PermissionId
                where u.UserName == userName
                select p.Code
            ).Distinct().ToListAsync();

            _logger.LogInformation("User '{UserName}' has permissions: [{Permissions}]",
                userName, string.Join(", ", permissionCodes));

            return permissionCodes;
        }

        public async Task<bool> HasPermissionAsync(string userName, string permissionCode)
        {
            // Query rõ ràng với explicit joins và so sánh KHÔNG phân biệt hoa thường
            var hasPermission = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.UserId equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.RoleId
                join rp in _context.RolePermissions on r.RoleId equals rp.RoleId
                join p in _context.Permissions on rp.PermissionId equals p.PermissionId
                where u.UserName == userName
                      && p.Code.ToUpper() == permissionCode.ToUpper()
                select p
            ).AnyAsync();

            _logger.LogInformation("Checking permission '{PermissionCode}' for user '{UserName}': {Result}",
                permissionCode, userName, hasPermission);

            return hasPermission;
        }
    }
}
