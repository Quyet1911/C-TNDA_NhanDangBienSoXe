using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CĐTNDA_NhanDangBienSoXe.Models;
using QRCoder;

namespace CĐTNDA_NhanDangBienSoXe.Controllers
{
    [Authorize(Policy = "CanManageCamera")]
    public class CameraController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CameraController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Camera
        public async Task<IActionResult> Index()
        {
            var cameras = await _context.Cameras
                .OrderByDescending(c => c.IsActive)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(cameras);
        }

        // GET: Camera/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Camera/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Camera camera)
        {
            if (ModelState.IsValid)
            {
                camera.CreatedAt = DateTime.UtcNow;
                _context.Cameras.Add(camera);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Camera '{camera.Name}' đã được thêm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(camera);
        }

        // GET: Camera/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }

            return View(camera);
        }

        // POST: Camera/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Camera camera)
        {
            if (id != camera.CameraId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(camera);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Camera '{camera.Name}' đã được cập nhật!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CameraExists(camera.CameraId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(camera);
        }

        // POST: Camera/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera != null)
            {
                _context.Cameras.Remove(camera);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Camera '{camera.Name}' đã được xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Camera/GenerateQR/5
        [HttpGet]
        public IActionResult GenerateQR(int id)
        {
            var camera = _context.Cameras.Find(id);
            if (camera == null)
            {
                return NotFound();
            }

            // Tạo URL để quét camera
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var scanUrl = $"{baseUrl}/Camera/MobileScan?cameraId={id}";

            // Tạo QR Code
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(scanUrl, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return File(qrCodeImage, "image/png");
                }
            }
        }

        // GET: Camera/LiveView/5 - Xem camera trực tiếp qua webcam máy tính
        [HttpGet]
        public async Task<IActionResult> LiveView(int? id)
        {
            if (id == null) return NotFound();

            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null) return NotFound();

            return View(camera);
        }

        // GET: Camera/MobileScan?cameraId=5
        [HttpGet]
        public async Task<IActionResult> MobileScan(int cameraId)
        {
            var camera = await _context.Cameras.FindAsync(cameraId);
            if (camera == null)
            {
                return NotFound();
            }

            ViewBag.CameraId = cameraId;
            ViewBag.CameraName = camera.Name;
            return View(camera);
        }

        // POST: Camera/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return Json(new { success = false, message = "Camera không tồn tại" });
            }

            camera.IsActive = !camera.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = camera.IsActive });
        }

        private bool CameraExists(int id)
        {
            return _context.Cameras.Any(e => e.CameraId == id);
        }
    }
}
