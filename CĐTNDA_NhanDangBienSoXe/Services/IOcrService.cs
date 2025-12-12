namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public interface IOcrService
    {
        /// <summary>
        /// Nhận dạng biển số xe
        /// </summary>
        Task<OcrResult> RecognizePlateAsync(string imagePath);
    }

    public class OcrResult
    {
        public bool Success { get; set; }
        public string? PlateText { get; set; }
        public decimal? Confidence { get; set; }
        public string? ErrorMessage { get; set; }
        public int ProcessingTimeMs { get; set; }
        public string? Engine { get; set; }
        public string? Version { get; set; }

        // Bounding box coordinates (nếu có)
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
