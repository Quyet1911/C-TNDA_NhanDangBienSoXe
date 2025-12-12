namespace CĐTNDA_NhanDangBienSoXe.Models
{
    public class HomeDashboardViewModel
    {
        // Thống kê hôm nay
        public int TodayTotalRecognitions { get; set; }
        public int TodayUniqueVehicles { get; set; }
        public int TodayInCount { get; set; }
        public int TodayOutCount { get; set; }

        // Thống kê tuần này
        public int WeekTotalRecognitions { get; set; }
        public int WeekUniqueVehicles { get; set; }

        // Thống kê tháng này
        public int MonthTotalRecognitions { get; set; }
        public int MonthUniqueVehicles { get; set; }

        // Camera status
        public int TotalCameras { get; set; }
        public int ActiveCameras { get; set; }
        public int InactiveCameras { get; set; }
        public List<CameraStatusInfo> CameraStatuses { get; set; } = new List<CameraStatusInfo>();

        // Nhận dạng gần đây
        public List<RecentRecognitionInfo> RecentRecognitions { get; set; } = new List<RecentRecognitionInfo>();

        // Thông tin user
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class CameraStatusInfo
    {
        public int CameraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LocationNote { get; set; }
        public bool IsActive { get; set; }
        public string StatusText => IsActive ? "Hoạt động" : "Không hoạt động";
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
    }

    public class RecentRecognitionInfo
    {
        public long RecognitionId { get; set; }
        public string? PlateNorm { get; set; }
        public DateTime DetectedAt { get; set; }
        public string? CameraName { get; set; }
        public decimal? Confidence { get; set; }
        public string? Direction { get; set; }
        public string? ImagePath { get; set; }

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
    }
}
