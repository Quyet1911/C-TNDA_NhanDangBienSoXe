using System.Diagnostics;
using System.Text.Json;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public class PlateRecognizerApiService : IOcrService
    {
        private readonly ILogger<PlateRecognizerApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly string _apiUrl;

        public PlateRecognizerApiService(
            ILogger<PlateRecognizerApiService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();

            _apiKey = _configuration["Ocr:PlateRecognizerApiKey"];
            _apiUrl = _configuration["Ocr:PlateRecognizerApiUrl"] ?? "https://api.platerecognizer.com/v1/plate-reader/";
        }

        public async Task<OcrResult> RecognizePlateAsync(string imagePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = "Chưa cấu hình API Key cho Plate Recognizer. Vui lòng thêm 'Ocr:PlateRecognizerApiKey' vào appsettings.json"
                    };
                }

                if (!File.Exists(imagePath))
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = "File ảnh không tồn tại"
                    };
                }

                // Đọc file ảnh
                var imageBytes = await File.ReadAllBytesAsync(imagePath);

                // Tạo multipart form data
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageBytes), "upload", Path.GetFileName(imagePath));
                content.Add(new StringContent("vn"), "regions"); // Vietnam region

                // Thêm API key vào header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_apiKey}");

                // Gửi yêu cầu API
                var response = await _httpClient.PostAsync(_apiUrl, content);

                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Plate Recognizer API error: {response.StatusCode} - {errorContent}");

                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode}",
                        ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        Engine = "PlateRecognizer"
                    };
                }

                // Parse response JSON
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<PlateRecognizerResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Results == null || apiResponse.Results.Count == 0)
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = "Không phát hiện biển số",
                        ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        Engine = "PlateRecognizer"
                    };
                }

                // Lấy kết quả đầu tiên (có confidence cao nhất)
                var bestResult = apiResponse.Results[0];

                return new OcrResult
                {
                    Success = true,
                    PlateText = bestResult.Plate,
                    Confidence = (decimal)(bestResult.Score * 100),
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Engine = "PlateRecognizer",
                    Version = "API v1",
                    X = bestResult.Box?.Xmin,
                    Y = bestResult.Box?.Ymin,
                    Width = bestResult.Box?.Xmax - bestResult.Box?.Xmin,
                    Height = bestResult.Box?.Ymax - bestResult.Box?.Ymin
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Lỗi khi gọi Plate Recognizer API");

                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}",
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Engine = "PlateRecognizer"
                };
            }
        }
    }

    // Response models cho Plate Recognizer API
    public class PlateRecognizerResponse
    {
        public List<PlateResult>? Results { get; set; }
        public double ProcessingTime { get; set; }
    }

    public class PlateResult
    {
        public string? Plate { get; set; }
        public double Score { get; set; }
        public BoxCoordinates? Box { get; set; }
    }

    public class BoxCoordinates
    {
        public int Xmin { get; set; }
        public int Ymin { get; set; }
        public int Xmax { get; set; }
        public int Ymax { get; set; }
    }
}
