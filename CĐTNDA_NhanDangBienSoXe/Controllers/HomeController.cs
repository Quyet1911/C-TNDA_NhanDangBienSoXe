using System.Diagnostics;
using System.Security.Claims;
using CĐTNDA_NhanDangBienSoXe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeDashboardViewModel();

            try
            {
                // Lấy thông tin user hiện tại
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                viewModel.UserName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
                viewModel.FullName = User.FindFirstValue("FullName") ?? viewModel.UserName;
                viewModel.RoleName = User.FindFirstValue(ClaimTypes.Role) ?? "User";

                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Thống kê hôm nay (giả sử có bảng Recognitions)
                // Vì chưa có dữ liệu thật, tôi sẽ để mặc định 0
                // Bạn có thể uncomment khi đã có bảng Recognitions
                /*
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

                // Nhận dạng gần đây (10 cái mới nhất)
                viewModel.RecentRecognitions = await _context.Recognitions
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
                */

                // Camera status (giả sử có bảng Cameras - uncomment khi đã có)
                /*
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
                */

                // Demo data cho dev (xóa khi đã có data thật)
                viewModel.TodayTotalRecognitions = 0;
                viewModel.TodayUniqueVehicles = 0;
                viewModel.TodayInCount = 0;
                viewModel.TodayOutCount = 0;
                viewModel.WeekTotalRecognitions = 0;
                viewModel.MonthTotalRecognitions = 0;
                viewModel.TotalCameras = 0;
                viewModel.ActiveCameras = 0;
                viewModel.InactiveCameras = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Vẫn return view với data mặc định
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
