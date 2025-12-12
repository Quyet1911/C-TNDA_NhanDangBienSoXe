using System.Diagnostics;
using IronOcr;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    /// <summary>
    /// OCR Service sử dụng IronOCR - Thư viện C# mạnh nhất cho OCR
    /// Pure C#, Offline, Độ chính xác cao
    /// </summary>
    public class IronOcrService : IOcrService
    {
        private readonly ILogger<IronOcrService> _logger;
        private readonly IronTesseract _ocrEngine;

        public IronOcrService(ILogger<IronOcrService> logger)
        {
            _logger = logger;

            // Khởi tạo IronOCR engine
            _ocrEngine = new IronTesseract();

            // Cấu hình cho nhận dạng biển số
            _ocrEngine.Language = OcrLanguage.English;
            _ocrEngine.Configuration.ReadBarCodes = false; // Không cần đọc barcode
            _ocrEngine.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.Auto;

            _logger.LogInformation("IronOCR Service initialized");
        }

        public async Task<OcrResult> RecognizePlateAsync(string imagePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!File.Exists(imagePath))
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = "File ảnh không tồn tại"
                    };
                }

                _logger.LogInformation($"Processing image with IronOCR: {imagePath}");

                // Xử lý ảnh và OCR
                return await Task.Run(() =>
                {
                    try
                    {
                        // Load ảnh
                        using var input = new OcrInput();
                        input.LoadImage(imagePath);

                        // Cải thiện chất lượng ảnh
                        input.Deskew(); // Xoay thẳng
                        input.DeNoise(); // Giảm noise
                        input.Contrast(); // Tăng contrast

                        // Thực hiện OCR
                        var result = _ocrEngine.Read(input);

                        stopwatch.Stop();

                        if (result == null || string.IsNullOrWhiteSpace(result.Text))
                        {
                            _logger.LogWarning("IronOCR: No text detected");
                            return new OcrResult
                            {
                                Success = false,
                                ErrorMessage = "Không nhận dạng được biển số",
                                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                                Engine = "IronOCR"
                            };
                        }

                        // Làm sạch text
                        var cleanedText = CleanPlateText(result.Text);

                        if (string.IsNullOrWhiteSpace(cleanedText))
                        {
                            _logger.LogWarning($"IronOCR: Text detected but invalid after cleaning: '{result.Text}'");
                            return new OcrResult
                            {
                                Success = false,
                                ErrorMessage = "Text phát hiện không phải biển số hợp lệ",
                                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                                Engine = "IronOCR"
                            };
                        }

                        // Lấy confidence trung bình
                        var confidence = result.Confidence;

                        _logger.LogInformation($"IronOCR success: '{cleanedText}' (confidence: {confidence}%)");

                        return new OcrResult
                        {
                            Success = true,
                            PlateText = cleanedText,
                            Confidence = (decimal)confidence,
                            ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                            Engine = "IronOCR",
                            Version = "2025.10"
                        };
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        _logger.LogError(ex, "Error during IronOCR processing");

                        return new OcrResult
                        {
                            Success = false,
                            ErrorMessage = $"Lỗi IronOCR: {ex.Message}",
                            ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                            Engine = "IronOCR"
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Lỗi tổng quát khi nhận dạng với IronOCR");

                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Engine = "IronOCR"
                };
            }
        }

        private string CleanPlateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Loại bỏ ký tự xuống dòng, khoảng trống
            text = text.Replace("\n", "").Replace("\r", "").Trim();

            // Loại bỏ khoảng trắng
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "");

            // Chỉ giữ lại chữ cái in hoa và số
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^A-Z0-9]", "");

            return text.ToUpper();
        }
    }
}
