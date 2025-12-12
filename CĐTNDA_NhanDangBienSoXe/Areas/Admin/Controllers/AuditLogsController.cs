using Microsoft.AspNetCore.Mvc;
using CĐTNDA_NhanDangBienSoXe.Services;

namespace CĐTNDA_NhanDangBienSoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: Admin/AuditLogs
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null,
            string? action = null, int page = 1, int pageSize = 50)
        {
            // Mặc định lấy 7 ngày gần nhất
            startDate = startDate ?? DateTime.Today.AddDays(-7);
            endDate = endDate ?? DateTime.Today;

            var logs = await _auditLogService.GetLogsAsync(startDate, endDate, action, page, pageSize);
            var totalCount = await _auditLogService.GetTotalCountAsync(startDate, endDate, action);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Action = action;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(logs);
        }
    }
}
