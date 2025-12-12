using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    [Authorize(Policy = "CanViewStats")]
    public class StatsController : Controller
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeDashboardViewModel();

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Thống kê hôm nay
            viewModel.TodayTotalRecognitions = await _context.Recognitions
                .Where(r => r.DetectedAt.Date == today)
                .CountAsync();

            viewModel.TodayUniqueVehicles = await _context.Recognitions
                .Where(r => r.DetectedAt.Date == today && r.PlateNorm != null)
                .Select(r => r.PlateNorm)
                .Distinct()
                .CountAsync();

            viewModel.TodayInCount = await _context.Recognitions
                .Where(r => r.DetectedAt.Date == today && r.Direction == "In")
                .CountAsync();

            viewModel.TodayOutCount = await _context.Recognitions
                .Where(r => r.DetectedAt.Date == today && r.Direction == "Out")
                .CountAsync();

            // Thống kê tuần này
            viewModel.WeekTotalRecognitions = await _context.Recognitions
                .Where(r => r.DetectedAt.Date >= weekStart)
                .CountAsync();

            viewModel.WeekUniqueVehicles = await _context.Recognitions
                .Where(r => r.DetectedAt.Date >= weekStart && r.PlateNorm != null)
                .Select(r => r.PlateNorm)
                .Distinct()
                .CountAsync();

            // Thống kê tháng này
            viewModel.MonthTotalRecognitions = await _context.Recognitions
                .Where(r => r.DetectedAt.Date >= monthStart)
                .CountAsync();

            viewModel.MonthUniqueVehicles = await _context.Recognitions
                .Where(r => r.DetectedAt.Date >= monthStart && r.PlateNorm != null)
                .Select(r => r.PlateNorm)
                .Distinct()
                .CountAsync();

            // Thống kê camera
            var cameras = await _context.Cameras.ToListAsync();
            viewModel.TotalCameras = cameras.Count;
            viewModel.ActiveCameras = cameras.Count(c => c.IsActive);
            viewModel.InactiveCameras = cameras.Count(c => !c.IsActive);

            viewModel.CameraStatuses = cameras.Select(c => new CameraStatusInfo
            {
                CameraId = c.CameraId,
                Name = c.Name,
                LocationNote = c.LocationNote,
                IsActive = c.IsActive
            }).ToList();

            // Nhận dạng gần đây (10 cái mới nhất)
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
                    Confidence = r.Confidence,
                    Direction = r.Direction,
                    ImagePath = r.PlateCropPath ?? r.ImagePath
                })
                .ToListAsync();

            return View(viewModel);
        }

        // API endpoint cho biểu đồ theo ngày (7 ngày gần nhất)
        [HttpGet]
        public async Task<IActionResult> GetDailyStats()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-6);

            var dailyStats = await _context.Recognitions
                .Where(r => r.DetectedAt.Date >= startDate && r.DetectedAt.Date <= endDate)
                .GroupBy(r => r.DetectedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Count(),
                    InCount = g.Count(r => r.Direction == "In"),
                    OutCount = g.Count(r => r.Direction == "Out")
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Json(dailyStats);
        }

        // API endpoint cho biểu đồ theo camera
        [HttpGet]
        public async Task<IActionResult> GetCameraStats()
        {
            var cameraStats = await _context.Recognitions
                .Include(r => r.Camera)
                .GroupBy(r => new { r.CameraId, CameraName = r.Camera != null ? r.Camera.Name : "Unknown" })
                .Select(g => new
                {
                    CameraName = g.Key.CameraName,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToListAsync();

            return Json(cameraStats);
        }

        // API endpoint cho biểu đồ theo giờ (hôm nay)
        [HttpGet]
        public async Task<IActionResult> GetHourlyStats()
        {
            var today = DateTime.UtcNow.Date;

            var hourlyStats = await _context.Recognitions
                .Where(r => r.DetectedAt.Date == today)
                .GroupBy(r => r.DetectedAt.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Total = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            return Json(hourlyStats);
        }
    }
}
