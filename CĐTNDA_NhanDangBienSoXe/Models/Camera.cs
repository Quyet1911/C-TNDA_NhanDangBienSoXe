using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    [Table("Cameras", Schema = "pr")]
    public class Camera
    {
        [Key]
        public int CameraId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int? AreaId { get; set; }

        [StringLength(255)]
        public string? LocationNote { get; set; }

        [StringLength(64)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? StreamUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime2(0)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Recognition> Recognitions { get; set; } = new List<Recognition>();
    }
}
