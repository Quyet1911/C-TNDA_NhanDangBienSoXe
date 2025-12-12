using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();

            // Thống kê người dùng
            viewModel.TotalUsers = await _context.Users.CountAsync();
            viewModel.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            viewModel.InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive);

            // Thống kê camera
            viewModel.TotalCameras = await _context.Cameras.CountAsync();
            viewModel.ActiveCameras = await _context.Cameras.CountAsync(c => c.IsActive);

            // Thống kê nhận dạng
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            viewModel.TotalRecognitions = await _context.Recognitions.CountAsync();
            viewModel.TodayRecognitions = await _context.Recognitions.CountAsync(r => r.DetectedAt.Date == today);
            viewModel.WeekRecognitions = await _context.Recognitions.CountAsync(r => r.DetectedAt.Date >= weekStart);
            viewModel.MonthRecognitions = await _context.Recognitions.CountAsync(r => r.DetectedAt.Date >= monthStart);

            // Người dùng mới nhất
            viewModel.RecentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentUserInfo
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // Nhận dạng mới nhất
            viewModel.RecentRecognitions = await _context.Recognitions
                .Include(r => r.Camera)
                .OrderByDescending(r => r.DetectedAt)
                .Take(10)
                .Select(r => new RecentRecognitionInfo
                {
                    RecognitionId = r.RecognitionId,
                    PlateNorm = r.PlateNorm,
                    DetectedAt = r.DetectedAt,
                    CameraName = r.Camera != null ? r.Camera.Name : null,
                    Confidence = r.Confidence != null ? (decimal)r.Confidence : null,
                    Direction = r.Direction,
                    ImagePath = r.PlateCropPath ?? r.ImagePath
                })
                .ToListAsync();

            return View(viewModel);
        }
    }
}
