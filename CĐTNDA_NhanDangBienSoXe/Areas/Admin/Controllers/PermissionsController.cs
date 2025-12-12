using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly AppDbContext _context;

        public PermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Permissions
        public async Task<IActionResult> Index(string? category)
        {
            var query = _context.Permissions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var permissions = await query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Get distinct categories for filter
            ViewBag.Categories = await _context.Permissions
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.SelectedCategory = category;

            return View(permissions);
        }

        // GET: Admin/Permissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Permissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermissionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if code exists
                if (await _context.Permissions.AnyAsync(p => p.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã quyền đã tồn tại");
                    return View(model);
                }

                var permission = new Permission
                {
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                    Category = model.Category
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Quyền '{permission.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Permissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            var model = new PermissionEditViewModel
            {
                PermissionId = permission.PermissionId,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Category = permission.Category
            };

            return View(model);
        }

        // POST: Admin/Permissions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PermissionEditViewModel model)
        {
            if (id != model.PermissionId) return NotFound();

            if (ModelState.IsValid)
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null) return NotFound();

                // Check if code is changed and already exists
                if (model.Code != permission.Code && await _context.Permissions.AnyAsync(p => p.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã quyền đã tồn tại");
                    return View(model);
                }

                permission.Name = model.Name;
                permission.Code = model.Code;
                permission.Description = model.Description;
                permission.Category = model.Category;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Quyền '{permission.Name}' đã được cập nhật!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Permissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null) return NotFound();

            return View(permission);
        }

        // POST: Admin/Permissions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission != null)
            {
                // Check if permission is assigned to roles
                if (permission.RolePermissions.Any())
                {
                    TempData["Error"] = $"Không thể xóa quyền '{permission.Name}' vì đang được gán cho {permission.RolePermissions.Count} vai trò!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Quyền '{permission.Name}' đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
