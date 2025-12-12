using Microsoft.AspNetCore.Authorization;
using CĐTNDA_NhanDangBienSoXe.Services;

namespace CĐTNDA_NhanDangBienSoXe.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Nếu user chưa authenticated thì fail
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userName = context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }

            // Kiểm tra nếu là Admin thì cho phép tất cả
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Kiểm tra permission
            var hasPermission = await _permissionService.HasPermissionAsync(userName, requirement.PermissionCode);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
