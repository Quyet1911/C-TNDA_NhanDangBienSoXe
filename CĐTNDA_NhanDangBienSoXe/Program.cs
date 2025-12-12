using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using CĐTNDA_NhanDangBienSoXe.Models;
using CĐTNDA_NhanDangBienSoXe.Services;
using CĐTNDA_NhanDangBienSoXe.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Services
builder.Services.AddHttpClient(); // For PlateRecognizerApiService

// Đăng ký OCR Service dựa trên config
var ocrProvider = builder.Configuration["Ocr:Provider"] ?? "Tesseract";
if (ocrProvider.Equals("PlateRecognizer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IOcrService, PlateRecognizerApiService>();
}
else if (ocrProvider.Equals("EasyOCR", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IOcrService, EasyOcrService>();
}
else if (ocrProvider.Equals("IronOCR", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IOcrService, IronOcrService>();
}
else
{
    // Default: Tesseract
    builder.Services.AddScoped<IOcrService, TesseractOcrService>();
}

builder.Services.AddScoped<PlateRecognitionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddHttpContextAccessor(); // Required for AuditLogService

// Đăng ký Authorization Handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Cấu hình Authentication với Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Cấu hình Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Policy cho quyền nhận dạng biển số
    options.AddPolicy("CanRecognize", policy =>
        policy.Requirements.Add(new PermissionRequirement("RECOGNIZE")));

    // Policy cho quyền xem thống kê
    options.AddPolicy("CanViewStats", policy =>
        policy.Requirements.Add(new PermissionRequirement("VIEW_STATS")));

    // Policy cho quyền quản lý camera
    options.AddPolicy("CanManageCamera", policy =>
        policy.Requirements.Add(new PermissionRequirement("MANAGE_CAMERA")));
});

// Thêm Session (tùy chọn)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route cho Admin Area
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
