using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using Tesseract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _tessdataPath;

        public TesseractOcrService(ILogger<TesseractOcrService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Lấy path từ config hoặc dùng mặc định
            _tessdataPath = _configuration["Ocr:TesseractDataPath"] ?? "./tessdata";
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

                // Kiểm tra thư mục tessdata
                if (!Directory.Exists(_tessdataPath))
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = $"Không tìm thấy thư mục tessdata tại: {_tessdataPath}. Vui lòng cài đặt dữ liệu ngôn ngữ (Tesseract language data)."
                    };
                }

                return await Task.Run(() =>
                {
                    string tempImagePath = Path.Combine(Path.GetTempPath(), $"ocr_temp_{Guid.NewGuid()}.jpg");

                    try
                    {
                        _logger.LogInformation($"Processing image: {imagePath}");

                        // Xử lý ảnh để cải thiện OCR
                        PreprocessImage(imagePath, tempImagePath);

                        // Thử nhiều cấu hình khác nhau
                        var bestResult = TryMultipleOcrConfigs(tempImagePath);

                        stopwatch.Stop();

                        if (bestResult == null || string.IsNullOrWhiteSpace(bestResult.Value.text))
                        {
                            _logger.LogWarning("Không nhận dạng được biển số sau tất cả các cấu hình");
                            return new OcrResult
                            {
                                Success = false,
                                ErrorMessage = "Không nhận dạng được biển số. Vui lòng thử ảnh rõ ràng hơn.",
                                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                                Engine = "Tesseract"
                            };
                        }

                        _logger.LogInformation($"OCR thành công: '{bestResult.Value.text}' với confidence {bestResult.Value.confidence:F2}");

                        return new OcrResult
                        {
                            Success = true,
                            PlateText = bestResult.Value.text,
                            Confidence = (decimal)(bestResult.Value.confidence * 100),
                            ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                            Engine = "Tesseract"
                        };
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        _logger.LogError(ex, "Lỗi khi nhận dạng với Tesseract");

                        return new OcrResult
                        {
                            Success = false,
                            ErrorMessage = $"Lỗi Tesseract: {ex.Message}",
                            ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                            Engine = "Tesseract"
                        };
                    }
                    finally
                    {
                        // Cleanup temp file
                        try
                        {
                            if (File.Exists(tempImagePath))
                            {
                                File.Delete(tempImagePath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Lỗi tổng quát khi nhận dạng");

                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        private void PreprocessImage(string inputPath, string outputPath)
        {
            try
            {
                using (var image = Image.Load<Rgb24>(inputPath))
                {
                    var width = image.Width;
                    var height = image.Height;

                    _logger.LogInformation($"Original image size: {width}x{height}");

                    image.Mutate(ctx =>
                    {
                        // 1. Resize nếu ảnh quá nhỏ
                        if (width < 400)
                        {
                            int newWidth = 800;
                            int newHeight = (int)(height * (800.0 / width));
                            ctx.Resize(newWidth, newHeight);
                            _logger.LogInformation($"Resized to: {newWidth}x{newHeight}");
                        }

                        // 2. Convert sang grayscale
                        ctx.Grayscale();

                        // 3. Tăng contrast
                        ctx.Contrast(1.5f);

                        // 4. Sharpen
                        ctx.GaussianSharpen(2.0f);

                        // 5. Tăng độ sáng một chút
                        ctx.Brightness(1.2f);
                    });

                    image.SaveAsJpeg(outputPath, new JpegEncoder { Quality = 100 });
                    _logger.LogInformation($"Preprocessed image saved to: {outputPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý ảnh");
                // Nếu lỗi, copy file gốc
                File.Copy(inputPath, outputPath, overwrite: true);
            }
        }

        private (string text, float confidence)? TryMultipleOcrConfigs(string imagePath)
        {
            var results = new List<(string text, float confidence)>();

            try
            {
                using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default))
                {
                    // Config 1: Default PSM với whitelist
                    _logger.LogInformation("Trying config 1: Default PSM");
                    var result1 = TrySingleOcrConfig(engine, imagePath, PageSegMode.Auto);
                    if (result1.HasValue) results.Add(result1.Value);

                    // Config 2: Single Line
                    _logger.LogInformation("Trying config 2: Single Line");
                    var result2 = TrySingleOcrConfig(engine, imagePath, PageSegMode.SingleLine);
                    if (result2.HasValue) results.Add(result2.Value);

                    // Config 3: Single Word
                    _logger.LogInformation("Trying config 3: Single Word");
                    var result3 = TrySingleOcrConfig(engine, imagePath, PageSegMode.SingleWord);
                    if (result3.HasValue) results.Add(result3.Value);

                    // Config 4: Sparse text
                    _logger.LogInformation("Trying config 4: Sparse Text");
                    var result4 = TrySingleOcrConfig(engine, imagePath, PageSegMode.SparseText);
                    if (result4.HasValue) results.Add(result4.Value);
                }

                if (results.Count == 0)
                {
                    _logger.LogWarning("Không có kết quả nào từ tất cả các config");
                    return null;
                }

                // Chọn kết quả tốt nhất
                var best = results
                    .OrderByDescending(r => r.confidence)
                    .ThenByDescending(r => r.text.Length)
                    .First();

                _logger.LogInformation($"Best result: '{best.text}' (confidence: {best.confidence:F2})");
                return best;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thử nhiều cấu hình OCR");
                return null;
            }
        }

        private (string text, float confidence)? TrySingleOcrConfig(
            TesseractEngine engine,
            string imagePath,
            PageSegMode pageSegMode)
        {
            try
            {
                // Set whitelist cho biển số
                engine.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHKLMNPRSTUVXYZ");

                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img, pageSegMode))
                    {
                        var text = page.GetText();
                        var confidence = page.GetMeanConfidence();

                        // Làm sạch text
                        text = CleanPlateText(text);

                        if (!string.IsNullOrWhiteSpace(text) && text.Length >= 4)
                        {
                            _logger.LogInformation($"  -> Result: '{text}' (confidence: {confidence:F2})");
                            return (text, confidence);
                        }

                        _logger.LogInformation($"  -> No valid result (text too short or empty)");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed with PSM {pageSegMode}");
                return null;
            }
        }

        private string CleanPlateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Loại bỏ ký tự xuống dòng, khoảng trống thừa
            text = text.Replace("\n", "").Replace("\r", "").Trim();

            // Loại bỏ khoảng trắng
            text = Regex.Replace(text, @"\s+", "");

            // Chỉ giữ lại chữ cái in hoa và số
            text = Regex.Replace(text, @"[^A-Z0-9]", "");

            return text.ToUpper();
        }
    }
}
