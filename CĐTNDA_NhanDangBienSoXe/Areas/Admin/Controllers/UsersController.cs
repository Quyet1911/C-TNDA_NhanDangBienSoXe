using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;
using CĐTNDA_NhanDangBienSoXe.Services;
using BCrypt.Net;
using System.Security.Claims;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public UsersController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : null;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string search, bool? isActive)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Filter by search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(search) ||
                    (u.FullName != null && u.FullName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            // Filter by status
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;

            return View(users);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            // Get all unique permissions from all roles
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
                .Distinct()
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToList();

            // Get statistics - simplified for now
            // TODO: Add CreatedBy field to Recognition table and implement proper statistics
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var totalRecognitions = 0;
            var recognitionsThisMonth = 0;
            var recognitionsToday = 0;

            // TODO: Implement AuditLog properly with all required fields
            var recentActivities = new List<AuditLog>();

            var model = new UserDetailsViewModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role).ToList(),
                Permissions = permissions,
                TotalRecognitions = totalRecognitions,
                RecognitionsThisMonth = recognitionsThisMonth,
                RecognitionsToday = recognitionsToday,
                RecentActivities = recentActivities
            };

            return View(model);
        }

        // GET: Admin/Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username exists
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    ViewBag.Roles = await _context.Roles.ToListAsync();
                    return View(model);
                }

                // Check if email exists
                if (!string.IsNullOrEmpty(model.Email) && await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    ViewBag.Roles = await _context.Roles.ToListAsync();
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 11),
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign roles
                if (model.RoleIds != null && model.RoleIds.Any())
                {
                    foreach (var roleId in model.RoleIds)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = roleId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Tạo người dùng",
                    $"Tạo người dùng mới: {user.UserName} ({user.FullName})",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Người dùng '{user.UserName}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            var model = new UserEditViewModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                RoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList()
            };

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null) return NotFound();

                // Check if email is changed and already exists
                if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
                {
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng");
                        ViewBag.Roles = await _context.Roles.ToListAsync();
                        return View(model);
                    }
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.IsActive = model.IsActive;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, workFactor: 11);
                }

                // Update roles
                _context.UserRoles.RemoveRange(user.UserRoles);
                if (model.RoleIds != null && model.RoleIds.Any())
                {
                    foreach (var roleId in model.RoleIds)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = roleId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Cập nhật người dùng",
                    $"Cập nhật thông tin người dùng: {user.UserName} ({user.FullName})",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Người dùng '{user.UserName}' đã được cập nhật!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(model);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Prevent deleting yourself
                var currentUserName = User.Identity?.Name;
                if (user.UserName == currentUserName)
                {
                    TempData["Error"] = "Không thể xóa tài khoản của chính mình!";
                    return RedirectToAction(nameof(Index));
                }

                var deletedUserName = user.UserName;
                var deletedFullName = user.FullName;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Xóa người dùng",
                    $"Xóa người dùng: {deletedUserName} ({deletedFullName})",
                    GetCurrentUserId(),
                    User.Identity?.Name
                );

                TempData["Success"] = $"Người dùng '{deletedUserName}' đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại" });
            }

            // Prevent disabling yourself
            var currentUserName = User.Identity?.Name;
            if (user.UserName == currentUserName)
            {
                return Json(new { success = false, message = "Không thể vô hiệu hóa tài khoản của chính mình!" });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            // Ghi audit log
            var statusText = user.IsActive ? "Kích hoạt" : "Vô hiệu hóa";
            await _auditLogService.LogAsync(
                $"{statusText} người dùng",
                $"{statusText} người dùng: {user.UserName} ({user.FullName})",
                GetCurrentUserId(),
                User.Identity?.Name
            );

            return Json(new { success = true, isActive = user.IsActive });
        }
    }
}
