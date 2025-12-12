using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;
using CĐTNDA_NhanDangBienSoXe.Services;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    [Authorize(Policy = "CanRecognize")]
    public class RecognitionController : Controller
    {
        private readonly ILogger<RecognitionController> _logger;
        private readonly AppDbContext _context;
        private readonly PlateRecognitionService _recognitionService;
        private readonly IWebHostEnvironment _env;

        public RecognitionController(
            ILogger<RecognitionController> logger,
            AppDbContext context,
            PlateRecognitionService recognitionService,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _recognitionService = recognitionService;
            _env = env;
        }

        // GET: /Recognition/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = new RecognitionIndexViewModel();

            try
            {
                // Lấy danh sách camera
                var cameras = await _context.Cameras
                    .OrderBy(c => c.Name)
                    .Select(c => new CameraOption
                    {
                        CameraId = c.CameraId,
                        Name = c.Name,
                        LocationNote = c.LocationNote,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                viewModel.AvailableCameras = cameras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cameras for recognition page");
            }

            return View(viewModel);
        }

        // POST: /Recognition/UploadImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(UploadImageViewModel model)
        {
            if (!ModelState.IsValid || model.ImageFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng chọn ảnh để tải lên"
                });
            }

            // Kiểm tra file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new
                {
                    success = false,
                    message = "Chỉ chấp nhận file ảnh định dạng JPG, PNG, hoặc BMP"
                });
            }

            // Kiểm tra kích thước file (max 10MB)
            if (model.ImageFile.Length > 10 * 1024 * 1024)
            {
                return Json(new
                {
                    success = false,
                    message = "Kích thước file không được vượt quá 10MB"
                });
            }

            try
            {
                var result = await _recognitionService.RecognizePlateAsync(
                    model.ImageFile,
                    model.CameraId,
                    model.Direction);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    plateText = result.PlateText,
                    plateNorm = result.PlateNorm,
                    confidence = result.Confidence,
                    imagePath = result.ImagePath,
                    plateCropPath = result.PlateCropPath,
                    detectedAt = result.DetectedAt?.ToString("dd/MM/yyyy HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image upload");
                return Json(new
                {
                    success = false,
                    message = $"Lỗi khi xử lý ảnh: {ex.Message}"
                });
            }
        }

        // POST: /Recognition/Recognize (API cho Camera LiveView - không cần AntiForgeryToken)
        [HttpPost]
        public async Task<IActionResult> Recognize(IFormFile ImageFile, int? CameraId, string? Direction)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Không nhận được ảnh"
                });
            }

            // Kiểm tra file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new
                {
                    success = false,
                    message = "Chỉ chấp nhận file ảnh định dạng JPG, PNG, hoặc BMP"
                });
            }

            try
            {
                var result = await _recognitionService.RecognizePlateAsync(
                    ImageFile,
                    CameraId,
                    Direction ?? "In");

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    plateTextRaw = result.PlateText,
                    plateNorm = result.PlateNorm,
                    confidence = result.Confidence,
                    imagePath = result.ImagePath,
                    plateCropPath = result.PlateCropPath,
                    detectedAt = result.DetectedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Recognize API");
                return Json(new
                {
                    success = false,
                    message = $"Lỗi khi xử lý ảnh: {ex.Message}"
                });
            }
        }

        // GET: /Recognition/History
        public async Task<IActionResult> History(
            int page = 1,
            string? searchPlate = null,
            int? cameraId = null,
            string? direction = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var viewModel = new RecognitionHistoryViewModel
            {
                PageNumber = page,
                PageSize = 20,
                SearchPlate = searchPlate,
                FilterCameraId = cameraId,
                FilterDirection = direction,
                FromDate = fromDate,
                ToDate = toDate
            };

            // Initialize ViewBag.Cameras as empty list (will be populated if cameras exist)
            ViewBag.Cameras = new List<object>();

            try
            {
                var query = _context.Recognitions
                    .Include(r => r.Camera)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchPlate))
                {
                    query = query.Where(r => r.PlateNorm != null && r.PlateNorm.Contains(searchPlate.ToUpper()));
                }

                if (cameraId.HasValue)
                {
                    query = query.Where(r => r.CameraId == cameraId.Value);
                }

                if (!string.IsNullOrWhiteSpace(direction))
                {
                    query = query.Where(r => r.Direction == direction);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(r => r.DetectedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.DetectedAt <= endOfDay);
                }

                // Get total count
                viewModel.TotalCount = await query.CountAsync();

                // Get paged data
                var recognitions = await query
                    .OrderByDescending(r => r.DetectedAt)
                    .Skip((page - 1) * viewModel.PageSize)
                    .Take(viewModel.PageSize)
                    .Select(r => new RecognitionHistoryItem
                    {
                        RecognitionId = r.RecognitionId,
                        PlateText = r.PlateTextRaw,
                        PlateNorm = r.PlateNorm,
                        Confidence = r.Confidence,
                        Direction = r.Direction,
                        CameraName = r.Camera != null ? r.Camera.Name : null,
                        DetectedAt = r.DetectedAt,
                        ImagePath = r.ImagePath,
                        PlateCropPath = r.PlateCropPath
                    })
                    .ToListAsync();

                viewModel.Recognitions = recognitions;

                // Load cameras for filter dropdown
                ViewBag.Cameras = await _context.Cameras
                    .OrderBy(c => c.Name)
                    .Select(c => new { c.CameraId, c.Name })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recognition history");
            }

            return View(viewModel);
        }

        // POST: /Recognition/Delete/{id} - Chỉ Admin mới được xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            // Kiểm tra quyền Admin
            if (!User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa" });
            }

            try
            {
                var recognition = await _context.Recognitions.FindAsync(id);
                if (recognition == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi" });
                }

                // Xóa file ảnh nếu có
                if (!string.IsNullOrEmpty(recognition.ImagePath))
                {
                    var imagePath = Path.Combine(_env.WebRootPath, recognition.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                if (!string.IsNullOrEmpty(recognition.PlateCropPath))
                {
                    var cropPath = Path.Combine(_env.WebRootPath, recognition.PlateCropPath.TrimStart('/'));
                    if (System.IO.File.Exists(cropPath))
                    {
                        System.IO.File.Delete(cropPath);
                    }
                }

                _context.Recognitions.Remove(recognition);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin {User.Identity?.Name} deleted recognition ID: {id}");

                return Json(new { success = true, message = "Đã xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting recognition ID: {id}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa" });
            }
        }

        // GET: /Recognition/Details/{id}
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                var recognition = await _context.Recognitions
                    .Include(r => r.Camera)
                    .FirstOrDefaultAsync(r => r.RecognitionId == id);

                if (recognition == null)
                {
                    return NotFound();
                }

                return View(recognition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading recognition details for ID: {id}");
                return NotFound();
            }
        }
    }
}
