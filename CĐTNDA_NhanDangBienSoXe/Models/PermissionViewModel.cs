using System.ComponentModel.DataAnnotations;

namespace CĐTNDA_NhanDangBienSoXe.Models
{
    public class PermissionCreateViewModel
    {
        [Required(ErrorMessage = "Tên quyền không được để trống")]
        [StringLength(50, ErrorMessage = "Tên quyền không được vượt quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã quyền không được để trống")]
        [StringLength(100, ErrorMessage = "Mã quyền không được vượt quá 100 ký tự")]
        [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Mã quyền chỉ chứa chữ in hoa và dấu gạch dưới (VD: VIEW_CAMERA)")]
        public string Code { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string? Category { get; set; }
    }

    public class PermissionEditViewModel
    {
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "Tên quyền không được để trống")]
        [StringLength(50, ErrorMessage = "Tên quyền không được vượt quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã quyền không được để trống")]
        [StringLength(100, ErrorMessage = "Mã quyền không được vượt quá 100 ký tự")]
        public string Code { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string? Category { get; set; }
    }
}
