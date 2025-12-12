using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    [Table("Recognitions", Schema = "pr")]
    public class Recognition
    {
        [Key]
        public long RecognitionId { get; set; }

        public int? CameraId { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        [StringLength(64)]
        public string? PlateTextRaw { get; set; }

        [StringLength(32)]
        public string? PlateNorm { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Confidence { get; set; }

        [StringLength(10)]
        public string? Direction { get; set; }

        [StringLength(10)]
        public string? Region { get; set; }

        [StringLength(50)]
        public string? OcrEngine { get; set; }

        [StringLength(50)]
        public string? OcrVersion { get; set; }

        public int? ProcessingMs { get; set; }

        public long? VehicleId { get; set; }

        public long? BestTagId { get; set; }

        [StringLength(500)]
        public string? ImagePath { get; set; }

        [StringLength(500)]
        public string? PlateCropPath { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? BBoxesJson { get; set; }

        [Column(TypeName = "varbinary(32)")]
        public byte[]? HashDedup { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CameraId")]
        public virtual Camera? Camera { get; set; }
    }
}
