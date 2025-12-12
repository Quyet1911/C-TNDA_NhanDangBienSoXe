namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<string>> GetUserPermissionCodesAsync(string userName);
        Task<bool> HasPermissionAsync(string userName, string permissionCode);
    }
}
