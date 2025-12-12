using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CĐTNDA_NhanDangBienSoXe.Models;
using CĐTNDA_NhanDangBienSoXe.Services;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuditLogService _auditLogService;

        public AccountController(AppDbContext context, ILogger<AccountController> logger, IAuditLogService auditLogService)
        {
            _context = context;
            _logger = logger;
            _auditLogService = auditLogService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập, chuyển về trang chủ
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // DEBUG: Log username
                _logger.LogInformation("=== LOGIN ATTEMPT ===");
                _logger.LogInformation("Username entered: {UserName}", model.UserName);
                _logger.LogInformation("Password entered: {Password}", model.Password);

                // Tìm user theo username
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserName}", model.UserName);
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                _logger.LogInformation("User found: UserId={UserId}, UserName={UserName}", user.UserId, user.UserName);
                _logger.LogInformation("PasswordHash from DB: {Hash}", user.PasswordHash);
                _logger.LogInformation("IsActive: {IsActive}", user.IsActive);

                // Kiểm tra active
                if (!user.IsActive)
                {
                    _logger.LogWarning("User account is inactive: {UserName}", user.UserName);
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa");
                    return View(model);
                }

                // Verify password (sử dụng BCrypt)
                _logger.LogInformation("Verifying password with BCrypt...");
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                _logger.LogInformation("BCrypt.Verify result: {IsValid}", isPasswordValid);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Password verification failed for user: {UserName}", user.UserName);
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Tạo claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("FullName", user.FullName ?? user.UserName),
                    new Claim(ClaimTypes.Email, user.Email ?? "")
                };

                // Thêm roles vào claims
                foreach (var userRole in user.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                // Đăng nhập
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Cập nhật LastLoginAt
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserName} logged in at {Time}", user.UserName, DateTime.UtcNow);

                // Ghi audit log
                await _auditLogService.LogAsync(
                    "Đăng nhập",
                    $"Người dùng '{user.FullName}' ({user.UserName}) đăng nhập thành công",
                    user.UserId,
                    user.UserName
                );

                // Redirect
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                // Redirect theo role
                if (user.UserRoles.Any(ur => ur.Role.Name == "Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {UserName}", model.UserName);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out at {Time}", DateTime.UtcNow);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

