using System.ComponentModel.DataAnnotations;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class RoleEditViewModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class RoleWithPermissionsViewModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public List<PermissionInfo> Permissions { get; set; } = new List<PermissionInfo>();
    }

    public class PermissionInfo
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsGranted { get; set; }
    }
}
