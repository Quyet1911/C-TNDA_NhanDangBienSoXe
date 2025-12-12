using System.ComponentModel.DataAnnotations;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    // ViewModel cho trang quét biển số xe
    public class RecognitionIndexViewModel
    {
        public List<CameraOption> AvailableCameras { get; set; } = new List<CameraOption>();
        public int? SelectedCameraId { get; set; }
    }

    public class CameraOption
    {
        public int CameraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LocationNote { get; set; }
        public bool IsActive { get; set; }
    }

    // ViewModel cho upload ảnh
    public class UploadImageViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ảnh")]
        public IFormFile? ImageFile { get; set; }

        public int? CameraId { get; set; }

        [StringLength(10)]
        public string? Direction { get; set; } // "In", "Out"
    }

    // ViewModel cho kết quả nhận dạng
    public class RecognitionResultViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? PlateText { get; set; }
        public string? PlateNorm { get; set; }
        public decimal? Confidence { get; set; }
        public string? ImagePath { get; set; }
        public string? PlateCropPath { get; set; }
        public DateTime? DetectedAt { get; set; }
    }

    // ViewModel cho lịch sử nhận dạng
    public class RecognitionHistoryViewModel
    {
        public List<RecognitionHistoryItem> Recognitions { get; set; } = new List<RecognitionHistoryItem>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Filters
        public string? SearchPlate { get; set; }
        public int? FilterCameraId { get; set; }
        public string? FilterDirection { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class RecognitionHistoryItem
    {
        public long RecognitionId { get; set; }
        public string? PlateText { get; set; }
        public string? PlateNorm { get; set; }
        public decimal? Confidence { get; set; }
        public string? Direction { get; set; }
        public string? CameraName { get; set; }
        public DateTime DetectedAt { get; set; }
        public string? ImagePath { get; set; }
        public string? PlateCropPath { get; set; }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - DetectedAt;
                if (timeSpan.TotalMinutes < 1) return "Vừa xong";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
                return $"{(int)timeSpan.TotalDays} ngày trước";
            }
        }

        public string DirectionIcon => Direction?.ToLower() switch
        {
            "in" => "fa-arrow-right",
            "out" => "fa-arrow-left",
            _ => "fa-minus"
        };

        public string DirectionText => Direction?.ToLower() switch
        {
            "in" => "Vào",
            "out" => "Ra",
            _ => ""
        };

        public string DirectionBadgeClass => Direction?.ToLower() switch
        {
            "in" => "badge bg-success",
            "out" => "badge bg-warning",
            _ => "badge bg-secondary"
        };
    }
}
