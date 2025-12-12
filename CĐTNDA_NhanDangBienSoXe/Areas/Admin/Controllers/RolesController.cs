using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;
using CĐTNDA_NhanDangBienSoXe.Services;
using System.Security.Claims;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public RolesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : null;
        }

        // GET: Admin/Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            return View(roles);
        }

        // GET: Admin/Roles/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Permissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View();
        }

        // POST: Admin/Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if role name exists
                if (await _context.Roles.AnyAsync(r => r.Name == model.Name))
                {
                    ModelState.AddModelError("Name", "Tên vai trò đã tồn tại");
                    ViewBag.Permissions = await _context.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                var role = new Role
                {
                    Name = model.Name,
                    Description = model.Description
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                // Assign permissions
                if (model.PermissionIds != null && model.PermissionIds.Any())
                {
                    foreach (var permissionId in model.PermissionIds)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.RoleId,
                            PermissionId = permissionId,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Tạo vai trò",
                    $"Tạo vai trò mới: {role.Name}",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Vai trò '{role.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Permissions = await _context.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
            return View(model);
        }

        // GET: Admin/Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null) return NotFound();

            var model = new RoleEditViewModel
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description,
                PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
            };

            ViewBag.Permissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(model);
        }

        // POST: Admin/Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleEditViewModel model)
        {
            if (id != model.RoleId) return NotFound();

            if (ModelState.IsValid)
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.RoleId == id);

                if (role == null) return NotFound();

                // Check if role name is changed and already exists
                if (model.Name != role.Name && await _context.Roles.AnyAsync(r => r.Name == model.Name && r.RoleId != id))
                {
                    ModelState.AddModelError("Name", "Tên vai trò đã tồn tại");
                    ViewBag.Permissions = await _context.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                role.Name = model.Name;
                role.Description = model.Description;

                // Update permissions
                _context.RolePermissions.RemoveRange(role.RolePermissions);
                if (model.PermissionIds != null && model.PermissionIds.Any())
                {
                    foreach (var permissionId in model.PermissionIds)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.RoleId,
                            PermissionId = permissionId,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Cập nhật vai trò",
                    $"Cập nhật vai trò: {role.Name}",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Vai trò '{role.Name}' đã được cập nhật!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Permissions = await _context.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
            return View(model);
        }

        // POST: Admin/Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role != null)
            {
                // Check if role is assigned to users
                if (role.UserRoles.Any())
                {
                    TempData["Error"] = $"Không thể xóa vai trò '{role.Name}' vì còn {role.UserRoles.Count} người dùng đang sử dụng!";
                    return RedirectToAction(nameof(Index));
                }

                // Prevent deleting Admin role
                if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Không thể xóa vai trò Admin!";
                    return RedirectToAction(nameof(Index));
                }

                var deletedRoleName = role.Name;

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Xóa vai trò",
                    $"Xóa vai trò: {deletedRoleName}",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Vai trò '{deletedRoleName}' đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null) return NotFound();

            var model = new RoleWithPermissionsViewModel
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description,
                UserCount = role.UserRoles.Count,
                Permissions = role.RolePermissions.Select(rp => new PermissionInfo
                {
                    PermissionId = rp.Permission.PermissionId,
                    Name = rp.Permission.Name,
                    Code = rp.Permission.Code,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category,
                    IsGranted = true
                }).ToList()
            };

            return View(model);
        }
    }
}
