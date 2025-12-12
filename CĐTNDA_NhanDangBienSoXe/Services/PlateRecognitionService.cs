using System.Diagnostics;
using CĐTNDA_NhanDangBienSoXe.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public class PlateRecognitionService
    {
        private readonly ILogger<PlateRecognitionService> _logger;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IOcrService _ocrService;

        public PlateRecognitionService(
            ILogger<PlateRecognitionService> logger,
            AppDbContext context,
            IWebHostEnvironment env,
            IOcrService ocrService)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _ocrService = ocrService;
        }

        /// <summary>
        /// Nhận dạng biển số xe từ ảnh
        /// </summary>
        public async Task<RecognitionResultViewModel> RecognizePlateAsync(
            IFormFile imageFile,
            int? cameraId = null,
            string? direction = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Lưu ảnh gốc
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "plates");
                Directory.CreateDirectory(uploadsFolder);

                // Luôn lưu file dưới dạng .jpg
                var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Convert bất kỳ format nào (WebP, PNG, etc) sang JPEG
                using (var imageStream = imageFile.OpenReadStream())
                using (var image = await Image.LoadAsync(imageStream))
                {
                    // Lưu dưới dạng JPEG với quality 95
                    await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 95 });
                }

                var webPath = $"/uploads/plates/{fileName}";

                // Gọi OCR service để nhận dạng
                _logger.LogInformation($"Bắt đầu nhận dạng ảnh: {filePath}");
                var ocrResult = await _ocrService.RecognizePlateAsync(filePath);

                stopwatch.Stop();

                if (!ocrResult.Success || string.IsNullOrWhiteSpace(ocrResult.PlateText))
                {
                    _logger.LogWarning($"OCR không thành công: {ocrResult.ErrorMessage}");

                    return new RecognitionResultViewModel
                    {
                        Success = false,
                        Message = ocrResult.ErrorMessage ?? "Không nhận dạng được biển số",
                        ImagePath = webPath,
                        DetectedAt = DateTime.UtcNow
                    };
                }

                // Chuẩn hóa biển số
                var plateNorm = NormalizePlate(ocrResult.PlateText);

                var result = new RecognitionResultViewModel
                {
                    Success = true,
                    Message = $"Nhận dạng thành công với {ocrResult.Engine}",
                    PlateText = ocrResult.PlateText,
                    PlateNorm = plateNorm,
                    Confidence = ocrResult.Confidence ?? 0,
                    ImagePath = webPath,
                    PlateCropPath = webPath,
                    DetectedAt = DateTime.UtcNow
                };

                // Lưu vào database
                var recognition = new Recognition
                {
                    CameraId = cameraId,
                    ImagePath = webPath,
                    PlateCropPath = webPath,
                    PlateTextRaw = ocrResult.PlateText,
                    PlateNorm = plateNorm,
                    Confidence = ocrResult.Confidence,
                    Direction = direction,
                    DetectedAt = DateTime.UtcNow,
                    Region = "VN-std",
                    OcrEngine = ocrResult.Engine,
                    OcrVersion = ocrResult.Version,
                    ProcessingMs = ocrResult.ProcessingTimeMs,
                    CreatedAt = DateTime.UtcNow
                };

                // Lưu bounding box nếu có
                if (ocrResult.X.HasValue && ocrResult.Y.HasValue && ocrResult.Width.HasValue && ocrResult.Height.HasValue)
                {
                    recognition.BBoxesJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        x = ocrResult.X.Value,
                        y = ocrResult.Y.Value,
                        width = ocrResult.Width.Value,
                        height = ocrResult.Height.Value
                    });
                }

                _context.Recognitions.Add(recognition);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Saved recognition: {recognition.RecognitionId} - Plate: {plateNorm} - Confidence: {ocrResult.Confidence}% - Time: {stopwatch.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during plate recognition");
                return new RecognitionResultViewModel
                {
                    Success = false,
                    Message = $"Lỗi khi nhận dạng: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Chuẩn hóa biển số (loại bỏ dấu gạch ngang, khoảng trắng, chuyển về uppercase)
        /// </summary>
        public string NormalizePlate(string plateText)
        {
            if (string.IsNullOrWhiteSpace(plateText))
                return string.Empty;

            return plateText
                .Replace("-", "")
                .Replace(" ", "")
                .Replace(".", "")
                .ToUpper()
                .Trim();
        }
    }
}
