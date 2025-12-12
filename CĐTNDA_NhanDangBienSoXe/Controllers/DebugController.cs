using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    public class DebugController : Controller
    {
        private readonly AppDbContext _context;

        public DebugController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Debug/TestHash
        public async Task<IActionResult> TestHash()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("=== DEBUG BCRYPT HASH ===\n");

            try
            {
                // 1. Lấy user admin từ DB
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");

                if (user == null)
                {
                    result.AppendLine("❌ KHÔNG TÌM THẤY USER 'admin' TRONG DATABASE!");
                    return Content(result.ToString(), "text/plain");
                }

                result.AppendLine($"✅ Tìm thấy user:");
                result.AppendLine($"   UserId: {user.UserId}");
                result.AppendLine($"   UserName: {user.UserName}");
                result.AppendLine($"   FullName: {user.FullName}");
                result.AppendLine($"   IsActive: {user.IsActive}");
                result.AppendLine($"   PasswordHash: {user.PasswordHash}");
                result.AppendLine();

                // 2. Test với password "Admin@123"
                string testPassword = "Admin@123";
                result.AppendLine($"🔑 Test password: '{testPassword}'");
                result.AppendLine();

                // 3. Verify với BCrypt
                try
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(testPassword, user.PasswordHash);
                    result.AppendLine($"BCrypt.Verify Result: {isValid}");

                    if (isValid)
                    {
                        result.AppendLine("\n✅ ✅ ✅ PASSWORD ĐÚNG! ✅ ✅ ✅");
                        result.AppendLine("Bạn có thể đăng nhập với:");
                        result.AppendLine($"  Username: {user.UserName}");
                        result.AppendLine($"  Password: {testPassword}");
                    }
                    else
                    {
                        result.AppendLine("\n❌ ❌ ❌ PASSWORD SAI! ❌ ❌ ❌");
                        result.AppendLine("\nCần tạo hash mới!");

                        // 4. Tạo hash mới
                        string newHash = BCrypt.Net.BCrypt.HashPassword(testPassword, 11);
                        result.AppendLine($"\nHash mới cho password '{testPassword}':");
                        result.AppendLine(newHash);
                        result.AppendLine("\nChạy SQL sau trong SQL Management Studio:");
                        result.AppendLine($"UPDATE pr.Users SET PasswordHash = N'{newHash}' WHERE UserName = N'admin';");
                    }
                }
                catch (Exception ex)
                {
                    result.AppendLine($"\n❌ LỖI KHI VERIFY: {ex.Message}");
                    result.AppendLine($"Exception Type: {ex.GetType().Name}");
                    result.AppendLine($"Stack: {ex.StackTrace}");
                }

                // 5. Test tạo hash mới
                result.AppendLine("\n=== TẠO HASH MỚI ===");
                for (int i = 0; i < 3; i++)
                {
                    string hash = BCrypt.Net.BCrypt.HashPassword(testPassword, 11);
                    bool verify = BCrypt.Net.BCrypt.Verify(testPassword, hash);
                    result.AppendLine($"\nHash #{i + 1}:");
                    result.AppendLine($"  {hash}");
                    result.AppendLine($"  Verify: {verify}");
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"\n❌ EXCEPTION: {ex.Message}");
                result.AppendLine($"Type: {ex.GetType().Name}");
                result.AppendLine($"Stack: {ex.StackTrace}");
            }

            return Content(result.ToString(), "text/plain; charset=utf-8");
        }

        // GET: /Debug/CheckDb
        public async Task<IActionResult> CheckDb()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("=== KIỂM TRA DATABASE ===\n");

            try
            {
                // Kiểm tra Users
                var users = await _context.Users.ToListAsync();
                result.AppendLine($"Số lượng Users: {users.Count}");
                foreach (var u in users)
                {
                    result.AppendLine($"  - UserId={u.UserId}, UserName={u.UserName}, IsActive={u.IsActive}");
                }
                result.AppendLine();

                // Kiểm tra Roles
                var roles = await _context.Roles.ToListAsync();
                result.AppendLine($"Số lượng Roles: {roles.Count}");
                foreach (var r in roles)
                {
                    result.AppendLine($"  - RoleId={r.RoleId}, Name={r.Name}");
                }
                result.AppendLine();

                // Kiểm tra UserRoles
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .ToListAsync();
                result.AppendLine($"Số lượng UserRoles: {userRoles.Count}");
                foreach (var ur in userRoles)
                {
                    result.AppendLine($"  - User={ur.User.UserName}, Role={ur.Role.Name}");
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"\n❌ LỖI: {ex.Message}");
            }

            return Content(result.ToString(), "text/plain; charset=utf-8");
        }
    }
}
