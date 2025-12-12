using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // Redirect Index to Overview
        public IActionResult Index()
        {
            return RedirectToAction("Overview");
        }

        // Báo cáo tổng hợp - Overview
        public async Task<IActionResult> Overview(string period = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModel = new ReportOverviewViewModel
            {
                Period = period,
                StartDate = startDate,
                EndDate = endDate
            };

            // Xác định khoảng thời gian
            DateTime start, end;
            switch (period.ToLower())
            {
                case "today":
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "week":
                    start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "month":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "custom":
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        start = startDate.Value.Date;
                        end = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    }
                    else
                    {
                        start = DateTime.Today.AddDays(-30);
                        end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    }
                    break;
                default:
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }

            viewModel.StartDate = start;
            viewModel.EndDate = end;

            // Lấy dữ liệu nhận dạng trong khoảng thời gian
            var recognitions = await _context.Recognitions
                .Include(r => r.Camera)
                .Where(r => r.DetectedAt >= start && r.DetectedAt <= end)
                .ToListAsync();

            // Thống kê tổng quan
            viewModel.TotalRecognitions = recognitions.Count;
            viewModel.UniqueVehicles = recognitions
                .Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                .Select(r => r.PlateNorm)
                .Distinct()
                .Count();
            viewModel.InCount = recognitions.Count(r => r.Direction == "In");
            viewModel.OutCount = recognitions.Count(r => r.Direction == "Out");

            var withConfidence = recognitions.Where(r => r.Confidence.HasValue).ToList();
            viewModel.AverageConfidence = withConfidence.Any()
                ? withConfidence.Average(r => r.Confidence!.Value)
                : 0;
            viewModel.SuccessRate = recognitions.Any()
                ? (int)((double)recognitions.Count(r => r.Confidence >= 70) / recognitions.Count * 100)
                : 0;

            // Thống kê theo camera
            viewModel.CameraStats = recognitions
                .GroupBy(r => new {
                    r.CameraId,
                    CameraName = r.Camera != null ? r.Camera.Name : "Unknown",
                    LocationNote = r.Camera != null ? r.Camera.LocationNote : "",
                    IsActive = r.Camera != null ? r.Camera.IsActive : false
                })
                .Select(g => new CameraReportItem
                {
                    CameraId = g.Key.CameraId ?? 0,
                    CameraName = g.Key.CameraName,
                    LocationNote = g.Key.LocationNote,
                    TotalRecognitions = g.Count(),
                    InCount = g.Count(r => r.Direction == "In"),
                    OutCount = g.Count(r => r.Direction == "Out"),
                    AverageConfidence = g.Where(r => r.Confidence.HasValue).Any()
                        ? g.Where(r => r.Confidence.HasValue).Average(r => r.Confidence!.Value)
                        : 0,
                    IsActive = g.Key.IsActive
                })
                .OrderByDescending(c => c.TotalRecognitions)
                .ToList();

            // Thống kê theo giờ
            viewModel.HourlyStats = recognitions
                .GroupBy(r => r.DetectedAt.Hour)
                .Select(g => new HourlyReportItem
                {
                    Hour = g.Key,
                    Total = g.Count(),
                    InCount = g.Count(r => r.Direction == "In"),
                    OutCount = g.Count(r => r.Direction == "Out")
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // Thống kê theo ngày (chỉ cho period > 1 ngày)
            if ((end - start).TotalDays > 1)
            {
                viewModel.DailyStats = recognitions
                    .GroupBy(r => r.DetectedAt.Date)
                    .Select(g => new DailyReportItem
                    {
                        Date = g.Key,
                        Total = g.Count(),
                        InCount = g.Count(r => r.Direction == "In"),
                        OutCount = g.Count(r => r.Direction == "Out"),
                        UniqueVehicles = g.Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                            .Select(r => r.PlateNorm)
                            .Distinct()
                            .Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
            }

            // Thống kê theo vùng
            var regionStats = recognitions
                .Where(r => !string.IsNullOrEmpty(r.Region))
                .GroupBy(r => r.Region)
                .Select(g => new RegionReportItem
                {
                    Region = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var totalWithRegion = regionStats.Sum(r => r.Count);
            foreach (var item in regionStats)
            {
                item.Percentage = totalWithRegion > 0 ? (decimal)item.Count / totalWithRegion * 100 : 0;
            }
            viewModel.RegionStats = regionStats.OrderByDescending(r => r.Count).ToList();

            // Top xe ra vào nhiều nhất
            viewModel.TopVehicles = recognitions
                .Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                .GroupBy(r => r.PlateNorm)
                .Select(g => new TopVehicleItem
                {
                    PlateNorm = g.Key!,
                    Count = g.Count(),
                    FirstSeen = g.Min(r => r.DetectedAt),
                    LastSeen = g.Max(r => r.DetectedAt)
                })
                .OrderByDescending(v => v.Count)
                .Take(10)
                .ToList();

            // Thời gian xử lý trung bình
            var withProcessingTime = recognitions.Where(r => r.ProcessingMs.HasValue).ToList();
            viewModel.AverageProcessingTime = withProcessingTime.Any()
                ? withProcessingTime.Average(r => r.ProcessingMs!.Value)
                : 0;

            return View(viewModel);
        }

        // Báo cáo theo ngày với filter và phân trang
        public async Task<IActionResult> ByDate(DateTime? startDate = null, DateTime? endDate = null,
            int? cameraId = null, string? direction = null, int page = 1, int pageSize = 50)
        {
            var viewModel = new DateRangeReportViewModel
            {
                StartDate = startDate ?? DateTime.Today.AddDays(-7),
                EndDate = endDate ?? DateTime.Today,
                CameraId = cameraId,
                Direction = direction,
                PageNumber = page,
                PageSize = pageSize
            };

            var query = _context.Recognitions
                .Include(r => r.Camera)
                .Where(r => r.DetectedAt >= viewModel.StartDate && r.DetectedAt <= viewModel.EndDate.AddDays(1).AddSeconds(-1));

            // Apply filters
            if (cameraId.HasValue)
            {
                query = query.Where(r => r.CameraId == cameraId);
            }

            if (!string.IsNullOrEmpty(direction))
            {
                query = query.Where(r => r.Direction == direction);
            }

            // Get total count
            viewModel.TotalCount = await query.CountAsync();
            viewModel.TotalPages = (int)Math.Ceiling(viewModel.TotalCount / (double)pageSize);

            // Get paged data
            viewModel.Recognitions = await query
                .OrderByDescending(r => r.DetectedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get cameras for filter dropdown
            ViewBag.Cameras = await _context.Cameras.ToListAsync();

            return View(viewModel);
        }

        // API: Lấy dữ liệu báo cáo dưới dạng JSON (cho AJAX)
        [HttpGet]
        public async Task<IActionResult> GetOverviewData(string period = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModel = new ReportOverviewViewModel
            {
                Period = period,
                StartDate = startDate,
                EndDate = endDate
            };

            // Xác định khoảng thời gian
            DateTime start, end;
            switch (period.ToLower())
            {
                case "today":
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "week":
                    start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "month":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "custom":
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        start = startDate.Value.Date;
                        end = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    }
                    else
                    {
                        start = DateTime.Today.AddDays(-30);
                        end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    }
                    break;
                default:
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }

            viewModel.StartDate = start;
            viewModel.EndDate = end;

            // Lấy dữ liệu nhận dạng trong khoảng thời gian
            var recognitions = await _context.Recognitions
                .Include(r => r.Camera)
                .Where(r => r.DetectedAt >= start && r.DetectedAt <= end)
                .ToListAsync();

            // Thống kê tổng quan
            viewModel.TotalRecognitions = recognitions.Count;
            viewModel.UniqueVehicles = recognitions
                .Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                .Select(r => r.PlateNorm)
                .Distinct()
                .Count();
            viewModel.InCount = recognitions.Count(r => r.Direction == "In");
            viewModel.OutCount = recognitions.Count(r => r.Direction == "Out");

            var withConfidence = recognitions.Where(r => r.Confidence.HasValue).ToList();
            viewModel.AverageConfidence = withConfidence.Any()
                ? withConfidence.Average(r => r.Confidence!.Value)
                : 0;
            viewModel.SuccessRate = recognitions.Any()
                ? (int)((double)recognitions.Count(r => r.Confidence >= 70) / recognitions.Count * 100)
                : 0;

            // Thống kê theo camera
            viewModel.CameraStats = recognitions
                .GroupBy(r => new {
                    r.CameraId,
                    CameraName = r.Camera != null ? r.Camera.Name : "Unknown",
                    LocationNote = r.Camera != null ? r.Camera.LocationNote : "",
                    IsActive = r.Camera != null ? r.Camera.IsActive : false
                })
                .Select(g => new CameraReportItem
                {
                    CameraId = g.Key.CameraId ?? 0,
                    CameraName = g.Key.CameraName,
                    LocationNote = g.Key.LocationNote,
                    TotalRecognitions = g.Count(),
                    InCount = g.Count(r => r.Direction == "In"),
                    OutCount = g.Count(r => r.Direction == "Out"),
                    AverageConfidence = g.Where(r => r.Confidence.HasValue).Any()
                        ? g.Where(r => r.Confidence.HasValue).Average(r => r.Confidence!.Value)
                        : 0,
                    IsActive = g.Key.IsActive
                })
                .OrderByDescending(c => c.TotalRecognitions)
                .ToList();

            // Thống kê theo giờ
            viewModel.HourlyStats = recognitions
                .GroupBy(r => r.DetectedAt.Hour)
                .Select(g => new HourlyReportItem
                {
                    Hour = g.Key,
                    Total = g.Count(),
                    InCount = g.Count(r => r.Direction == "In"),
                    OutCount = g.Count(r => r.Direction == "Out")
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // Thống kê theo ngày (chỉ cho period > 1 ngày)
            if ((end - start).TotalDays > 1)
            {
                viewModel.DailyStats = recognitions
                    .GroupBy(r => r.DetectedAt.Date)
                    .Select(g => new DailyReportItem
                    {
                        Date = g.Key,
                        Total = g.Count(),
                        InCount = g.Count(r => r.Direction == "In"),
                        OutCount = g.Count(r => r.Direction == "Out"),
                        UniqueVehicles = g.Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                            .Select(r => r.PlateNorm)
                            .Distinct()
                            .Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
            }

            // Thống kê theo vùng
            var regionStats = recognitions
                .Where(r => !string.IsNullOrEmpty(r.Region))
                .GroupBy(r => r.Region)
                .Select(g => new RegionReportItem
                {
                    Region = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var totalWithRegion = regionStats.Sum(r => r.Count);
            foreach (var item in regionStats)
            {
                item.Percentage = totalWithRegion > 0 ? (decimal)item.Count / totalWithRegion * 100 : 0;
            }
            viewModel.RegionStats = regionStats.OrderByDescending(r => r.Count).ToList();

            // Top xe ra vào nhiều nhất
            viewModel.TopVehicles = recognitions
                .Where(r => !string.IsNullOrEmpty(r.PlateNorm))
                .GroupBy(r => r.PlateNorm)
                .Select(g => new TopVehicleItem
                {
                    PlateNorm = g.Key!,
                    Count = g.Count(),
                    FirstSeen = g.Min(r => r.DetectedAt),
                    LastSeen = g.Max(r => r.DetectedAt)
                })
                .OrderByDescending(v => v.Count)
                .Take(10)
                .ToList();

            // Thời gian xử lý trung bình
            var withProcessingTime = recognitions.Where(r => r.ProcessingMs.HasValue).ToList();
            viewModel.AverageProcessingTime = withProcessingTime.Any()
                ? withProcessingTime.Average(r => r.ProcessingMs!.Value)
                : 0;

            return Json(viewModel);
        }

        // API: Export báo cáo ra CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string period = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            // Xác định khoảng thời gian
            DateTime start, end;
            switch (period.ToLower())
            {
                case "today":
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "week":
                    start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "month":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "custom":
                    start = startDate ?? DateTime.Today.AddDays(-30);
                    end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                default:
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }

            var recognitions = await _context.Recognitions
                .Include(r => r.Camera)
                .Where(r => r.DetectedAt >= start && r.DetectedAt <= end)
                .OrderByDescending(r => r.DetectedAt)
                .ToListAsync();

            // Generate CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Thời gian,Biển số,Camera,Hướng,Độ tin cậy,Vùng,Thời gian xử lý (ms)");

            foreach (var r in recognitions)
            {
                csv.AppendLine($"{r.DetectedAt:dd/MM/yyyy HH:mm:ss},{r.PlateNorm},{r.Camera?.Name},{r.Direction},{r.Confidence},{r.Region},{r.ProcessingMs}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"BaoCao_{period}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
