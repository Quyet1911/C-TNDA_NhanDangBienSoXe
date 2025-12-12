using System.Diagnostics;
using System.Text.Json;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    /// <summary>
    /// OCR Service sử dụng Python EasyOCR Microservice
    /// </summary>
    public class EasyOcrService : IOcrService
    {
        private readonly ILogger<EasyOcrService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _pythonServiceUrl;

        public EasyOcrService(
            ILogger<EasyOcrService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // EasyOCR có thể mất vài giây

            _pythonServiceUrl = _configuration["Ocr:EasyOcrServiceUrl"] ?? "http://localhost:5001";
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

                // Kiểm tra Python service có chạy không
                try
                {
                    var healthCheck = await _httpClient.GetAsync($"{_pythonServiceUrl}/health");
                    if (!healthCheck.IsSuccessStatusCode)
                    {
                        return new OcrResult
                        {
                            Success = false,
                            ErrorMessage = "Python OCR Service không phản hồi. Vui lòng khởi động service bằng: python python_ocr_service/app.py"
                        };
                    }
                }
                catch (HttpRequestException)
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = $"Không kết nối được tới Python OCR Service tại {_pythonServiceUrl}. Vui lòng khởi động service."
                    };
                }

                // Đọc file ảnh
                var imageBytes = await File.ReadAllBytesAsync(imagePath);

                // Tạo multipart form data
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageBytes), "image", Path.GetFileName(imagePath));

                // Gửi request tới Python service
                _logger.LogInformation($"Sending image to Python EasyOCR service: {_pythonServiceUrl}/recognize");
                var response = await _httpClient.PostAsync($"{_pythonServiceUrl}/recognize", content);

                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Python OCR Service error: {response.StatusCode} - {errorContent}");

                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = $"Python Service Error: {response.StatusCode}",
                        ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        Engine = "EasyOCR"
                    };
                }

                // Parse response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<EasyOcrResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse == null || !apiResponse.Success)
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = apiResponse?.Error ?? "Không nhận dạng được biển số",
                        ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        Engine = "EasyOCR"
                    };
                }

                _logger.LogInformation($"EasyOCR result: '{apiResponse.PlateText}' (confidence: {apiResponse.Confidence}%)");

                return new OcrResult
                {
                    Success = true,
                    PlateText = apiResponse.PlateText,
                    Confidence = (decimal)apiResponse.Confidence,
                    ProcessingTimeMs = apiResponse.ProcessingTimeMs,
                    Engine = apiResponse.Engine ?? "EasyOCR",
                    X = apiResponse.Bbox?.X,
                    Y = apiResponse.Bbox?.Y,
                    Width = apiResponse.Bbox?.Width,
                    Height = apiResponse.Bbox?.Height
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Lỗi khi gọi Python EasyOCR service");

                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}",
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Engine = "EasyOCR"
                };
            }
        }
    }

    // Response models cho EasyOCR Python Service
    public class EasyOcrResponse
    {
        public bool Success { get; set; }
        public string? PlateText { get; set; }
        public string? RawText { get; set; }
        public double Confidence { get; set; }
        public int ProcessingTimeMs { get; set; }
        public string? Engine { get; set; }
        public string? Error { get; set; }
        public BboxCoordinates? Bbox { get; set; }
    }

    public class BboxCoordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
