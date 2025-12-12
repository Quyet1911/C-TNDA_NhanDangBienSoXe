using System.ComponentModel.DataAnnotations;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    // Báo cáo tổng hợp
    public class ReportOverviewViewModel
    {
        // Thời gian báo cáo
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Period { get; set; } = "today"; // today, week, month, custom

        // Thống kê tổng quan
        public int TotalRecognitions { get; set; }
        public int UniqueVehicles { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public decimal AverageConfidence { get; set; }
        public int SuccessRate { get; set; } // Tỷ lệ nhận dạng thành công (confidence > 70%)

        // Thống kê theo camera
        public List<CameraReportItem> CameraStats { get; set; } = new List<CameraReportItem>();

        // Thống kê theo giờ
        public List<HourlyReportItem> HourlyStats { get; set; } = new List<HourlyReportItem>();

        // Thống kê theo ngày
        public List<DailyReportItem> DailyStats { get; set; } = new List<DailyReportItem>();

        // Thống kê theo vùng (nếu có)
        public List<RegionReportItem> RegionStats { get; set; } = new List<RegionReportItem>();

        // Top xe ra vào nhiều nhất
        public List<TopVehicleItem> TopVehicles { get; set; } = new List<TopVehicleItem>();

        // Thời gian xử lý trung bình
        public double AverageProcessingTime { get; set; }
    }

    public class CameraReportItem
    {
        public int CameraId { get; set; }
        public string CameraName { get; set; } = string.Empty;
        public string? LocationNote { get; set; }
        public int TotalRecognitions { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public decimal AverageConfidence { get; set; }
        public bool IsActive { get; set; }
    }

    public class HourlyReportItem
    {
        public int Hour { get; set; }
        public int Total { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
    }

    public class DailyReportItem
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public int UniqueVehicles { get; set; }
    }

    public class RegionReportItem
    {
        public string? Region { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopVehicleItem
    {
        public string PlateNorm { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }

    // Báo cáo theo ngày với filter
    public class DateRangeReportViewModel
    {
        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public int? CameraId { get; set; }
        public string? Direction { get; set; } // In, Out

        // Kết quả
        public List<Recognition> Recognitions { get; set; } = new List<Recognition>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
    }

    // Báo cáo theo loại xe (nếu có phân loại)
    public class VehicleTypeReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<VehicleTypeItem> VehicleTypes { get; set; } = new List<VehicleTypeItem>();
    }

    public class VehicleTypeItem
    {
        public string VehicleType { get; set; } = string.Empty; // Car, Truck, Motorcycle, etc.
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
