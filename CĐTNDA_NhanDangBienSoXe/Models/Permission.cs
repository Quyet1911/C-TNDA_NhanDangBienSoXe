using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    [Table("Permissions", Schema = "pr")]
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty; // VD: VIEW_CAMERA, VIEW_STATS, SCAN, RECOGNIZE

        [StringLength(255)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; } // VD: Camera, Statistics, Recognition

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
