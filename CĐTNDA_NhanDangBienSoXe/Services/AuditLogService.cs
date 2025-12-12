using CĐTNDA_NhanDangBienSoXe.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CĐTNDA_NhanDangBienSoXe.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string? detail = null, int? userId = null, string? userName = null);
        Task<List<AuditLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, int page = 1, int pageSize = 50);
        Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditLogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string action, string? detail = null, int? userId = null, string? userName = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Lấy IP address
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

                // Nếu không truyền userName, dùng "System"
                userName = userName ?? "System";

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    Detail = detail,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.Now
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit log saved: {Action} by {UserName}", action, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit: {Action}, {Detail}", action, detail);
            }
        }

        public async Task<List<AuditLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, int page = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(a => a.CreatedAt <= endOfDay);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action.Contains(action));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(a => a.CreatedAt <= endOfDay);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action.Contains(action));
            }

            return await query.CountAsync();
        }
    }
}
