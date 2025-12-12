using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Nhật ký thao tác
namespace CĐTNDA_NhanDangBienSoXe.Models
{
    [Table("AuditLogs", Schema = "dbo")]
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }

        [StringLength(50)]
        public string? UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // VD: "Đăng nhập", "Nhận dạng biển số", "Tạo người dùng"

        [StringLength(500)]
        public string? Detail { get; set; } // Chi tiết thao tác

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
