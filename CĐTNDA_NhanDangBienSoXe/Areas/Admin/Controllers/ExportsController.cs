using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;
using System.Text;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportsController : Controller
    {
        private readonly AppDbContext _context;

        public ExportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Exports
        public IActionResult Index()
        {
            return View();
        }

        // POST: Admin/Exports/ExportRecognitions
        [HttpPost]
        public async Task<IActionResult> ExportRecognitions(DateTime? startDate, DateTime? endDate,
            int? cameraId = null, string? direction = null, string format = "csv")
        {
            // Xác định khoảng thời gian
            startDate = startDate ?? DateTime.Today.AddDays(-7);
            endDate = endDate ?? DateTime.Today;
            var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Lấy dữ liệu
            var query = _context.Recognitions
                .Include(r => r.Camera)
                .Where(r => r.DetectedAt >= startDate && r.DetectedAt <= endOfDay);

            if (cameraId.HasValue)
            {
                query = query.Where(r => r.CameraId == cameraId);
            }

            if (!string.IsNullOrEmpty(direction))
            {
                query = query.Where(r => r.Direction == direction);
            }

            var recognitions = await query
                .OrderByDescending(r => r.DetectedAt)
                .ToListAsync();

            // Xuất theo định dạng
            if (format.ToLower() == "excel")
            {
                return ExportToExcel(recognitions, startDate.Value, endDate.Value);
            }
            else
            {
                return ExportToCsv(recognitions, startDate.Value, endDate.Value);
            }
        }

        // POST: Admin/Exports/ExportCameras
        [HttpPost]
        public async Task<IActionResult> ExportCameras(string format = "csv")
        {
            var cameras = await _context.Cameras
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (format.ToLower() == "excel")
            {
                return ExportCamerasToExcel(cameras);
            }
            else
            {
                return ExportCamerasToCsv(cameras);
            }
        }

        // POST: Admin/Exports/ExportUsers
        [HttpPost]
        public async Task<IActionResult> ExportUsers(string format = "csv")
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            if (format.ToLower() == "excel")
            {
                return ExportUsersToExcel(users);
            }
            else
            {
                return ExportUsersToCsv(users);
            }
        }

        // Helper: Export Recognitions to CSV
        private FileResult ExportToCsv(List<Recognition> recognitions, DateTime startDate, DateTime endDate)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Thời gian,Biển số,Camera,Vị trí,Hướng,Độ tin cậy (%),Vùng,Thời gian xử lý (ms)");

            foreach (var r in recognitions)
            {
                csv.AppendLine($"{r.DetectedAt:dd/MM/yyyy HH:mm:ss}," +
                              $"{r.PlateNorm ?? "N/A"}," +
                              $"{r.Camera?.Name ?? "N/A"}," +
                              $"{r.Camera?.LocationNote ?? ""}," +
                              $"{r.Direction ?? ""}," +
                              $"{r.Confidence?.ToString("F2") ?? ""}," +
                              $"{r.Region ?? ""}," +
                              $"{r.ProcessingMs?.ToString() ?? ""}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            var fileName = $"NhanDang_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // Helper: Export Recognitions to Excel (HTML table format)
        private FileResult ExportToExcel(List<Recognition> recognitions, DateTime startDate, DateTime endDate)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            html.AppendLine("<table border='1'>");
            html.AppendLine("<tr><th>Thời gian</th><th>Biển số</th><th>Camera</th><th>Vị trí</th><th>Hướng</th><th>Độ tin cậy (%)</th><th>Vùng</th><th>Thời gian xử lý (ms)</th></tr>");

            foreach (var r in recognitions)
            {
                html.AppendLine($"<tr>" +
                    $"<td>{r.DetectedAt:dd/MM/yyyy HH:mm:ss}</td>" +
                    $"<td>{r.PlateNorm ?? "N/A"}</td>" +
                    $"<td>{r.Camera?.Name ?? "N/A"}</td>" +
                    $"<td>{r.Camera?.LocationNote ?? ""}</td>" +
                    $"<td>{r.Direction ?? ""}</td>" +
                    $"<td>{r.Confidence?.ToString("F2") ?? ""}</td>" +
                    $"<td>{r.Region ?? ""}</td>" +
                    $"<td>{r.ProcessingMs?.ToString() ?? ""}</td>" +
                    $"</tr>");
            }

            html.AppendLine("</table></body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = $"NhanDang_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xls";
            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        // Helper: Export Cameras to CSV
        private FileResult ExportCamerasToCsv(List<Camera> cameras)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Tên camera,Vị trí,URL,Trạng thái,Ngày tạo");

            foreach (var c in cameras)
            {
                csv.AppendLine($"{c.CameraId}," +
                              $"{c.Name}," +
                              $"{c.LocationNote ?? ""}," +
                              $"{c.StreamUrl ?? ""}," +
                              $"{(c.IsActive ? "Hoạt động" : "Tắt")}," +
                              $"{c.CreatedAt:dd/MM/yyyy HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            var fileName = $"DanhSachCamera_{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // Helper: Export Cameras to Excel
        private FileResult ExportCamerasToExcel(List<Camera> cameras)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            html.AppendLine("<table border='1'>");
            html.AppendLine("<tr><th>ID</th><th>Tên camera</th><th>Vị trí</th><th>URL</th><th>Trạng thái</th><th>Ngày tạo</th></tr>");

            foreach (var c in cameras)
            {
                html.AppendLine($"<tr>" +
                    $"<td>{c.CameraId}</td>" +
                    $"<td>{c.Name}</td>" +
                    $"<td>{c.LocationNote ?? ""}</td>" +
                    $"<td>{c.StreamUrl ?? ""}</td>" +
                    $"<td>{(c.IsActive ? "Hoạt động" : "Tắt")}</td>" +
                    $"<td>{c.CreatedAt:dd/MM/yyyy HH:mm:ss}</td>" +
                    $"</tr>");
            }

            html.AppendLine("</table></body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = $"DanhSachCamera_{DateTime.Now:yyyyMMdd}.xls";
            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        // Helper: Export Users to CSV
        private FileResult ExportUsersToCsv(List<User> users)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Tên đăng nhập,Họ tên,Email,Vai trò,Trạng thái,Đăng nhập cuối,Ngày tạo");

            foreach (var u in users)
            {
                var roles = string.Join(", ", u.UserRoles.Select(ur => ur.Role.Name));
                csv.AppendLine($"{u.UserId}," +
                              $"{u.UserName}," +
                              $"{u.FullName ?? ""}," +
                              $"{u.Email ?? ""}," +
                              $"{roles}," +
                              $"{(u.IsActive ? "Hoạt động" : "Khóa")}," +
                              $"{(u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "")}," +
                              $"{u.CreatedAt:dd/MM/yyyy HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            var fileName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // Helper: Export Users to Excel
        private FileResult ExportUsersToExcel(List<User> users)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            html.AppendLine("<table border='1'>");
            html.AppendLine("<tr><th>ID</th><th>Tên đăng nhập</th><th>Họ tên</th><th>Email</th><th>Vai trò</th><th>Trạng thái</th><th>Đăng nhập cuối</th><th>Ngày tạo</th></tr>");

            foreach (var u in users)
            {
                var roles = string.Join(", ", u.UserRoles.Select(ur => ur.Role.Name));
                html.AppendLine($"<tr>" +
                    $"<td>{u.UserId}</td>" +
                    $"<td>{u.UserName}</td>" +
                    $"<td>{u.FullName ?? ""}</td>" +
                    $"<td>{u.Email ?? ""}</td>" +
                    $"<td>{roles}</td>" +
                    $"<td>{(u.IsActive ? "Hoạt động" : "Khóa")}</td>" +
                    $"<td>{(u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "")}</td>" +
                    $"<td>{u.CreatedAt:dd/MM/yyyy HH:mm:ss}</td>" +
                    $"</tr>");
            }

            html.AppendLine("</table></body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd}.xls";
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
    }
}
