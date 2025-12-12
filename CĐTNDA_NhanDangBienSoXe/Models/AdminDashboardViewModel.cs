namespace CĐTNDA_NhanDangBienSoXe.Models
{
    public class AdminDashboardViewModel
    {
        // Thống kê người dùng
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }

        // Thống kê camera
        public int TotalCameras { get; set; }
        public int ActiveCameras { get; set; }

        // Thống kê nhận dạng
        public int TotalRecognitions { get; set; }
        public int TodayRecognitions { get; set; }
        public int WeekRecognitions { get; set; }
        public int MonthRecognitions { get; set; }

        // Dữ liệu chi tiết
        public List<RecentUserInfo> RecentUsers { get; set; } = new List<RecentUserInfo>();
        public List<RecentRecognitionInfo> RecentRecognitions { get; set; } = new List<RecentRecognitionInfo>();
    }

    public class RecentUserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                if (timeSpan.TotalMinutes < 1) return "Vừa xong";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
                if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} ngày trước";
                return CreatedAt.ToString("dd/MM/yyyy");
            }
        }
    }
}
