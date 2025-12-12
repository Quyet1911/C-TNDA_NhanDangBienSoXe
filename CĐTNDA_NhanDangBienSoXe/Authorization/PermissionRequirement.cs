using Microsoft.AspNetCore.Authorization;

namespace CĐTNDA_NhanDangBienSoXe.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionCode { get; }

        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }
}
