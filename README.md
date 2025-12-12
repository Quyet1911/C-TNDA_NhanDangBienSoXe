# Hệ Thống Nhận Dạng Biển Số Xe

Ứng dụng web ASP.NET Core MVC để nhận dạng biển số xe tự động sử dụng công nghệ OCR (Optical Character Recognition) và xử lý hình ảnh. Đây là một dự án tốt nghiệp của tôi - một dự án nhỏ.

## Mục Lục

- [Giới thiệu](#giới-thiệu)
- [Tính năng](#tính-năng)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt](#cài-đặt)
- [Cấu hình](#cấu-hình)
- [Sử dụng](#sử-dụng)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [API Endpoints](#api-endpoints)
- [Troubleshooting](#troubleshooting)
- [Đóng góp](#đóng-góp)
- [License](#license)

## Giới thiệu

Hệ thống nhận dạng biển số xe tự động được phát triển nhằm hỗ trợ quản lý và giám sát phương tiện ra vào các khu vực như bãi đỗ xe, khu dân cư, công ty, trường học, v.v.

Ứng dụng sử dụng công nghệ OCR (Tesseract) kết hợp với xử lý hình ảnh để tự động nhận dạng biển số xe từ hình ảnh camera và lưu trữ thông tin vào cơ sở dữ liệu, giúp tự động hóa quy trình quản lý và giám sát phương tiện.

## Tính năng

### Khu vực Public (Nhân viên / Bảo vệ)

- **Trang chủ**
  - Dashboard nhỏ hiển thị thống kê cơ bản
  - Số lượng xe ra/vào trong ngày
  - Tình trạng camera hoạt động

- **Quản lý Camera**
  - Xem live stream từ camera
  - Chụp ảnh snapshot từ camera
  - Xem trạng thái kết nối camera

- **Nhận dạng biển số**
  - Quét biển số từ camera hoặc upload ảnh
  - Hiển thị kết quả nhận dạng real-time
  - Lưu kết quả vào cơ sở dữ liệu

- **Thống kê cơ bản**
  - Thống kê theo ngày
  - Thống kê theo khu vực
  - Lọc theo thời gian

- **Đăng nhập / Đăng xuất**
  - Xác thực người dùng
  - Phân quyền truy cập

### Khu vực Admin (Quản trị viên)

- **Dashboard Quản trị**
  - Tổng quan toàn hệ thống
  - Biểu đồ thống kê chi tiết
  - Giám sát real-time

- **Quản lý Người dùng**
  - Thêm / Sửa / Xóa người dùng
  - Xem chi tiết thông tin người dùng
  - Gán vai trò cho người dùng

- **Quản lý Vai trò** (Roles)
  - Tạo và quản lý vai trò
  - Phân quyền chi tiết cho từng vai trò

- **Lịch sử Nhận dạng**
  - Xem toàn bộ lịch sử nhận diện
  - Lọc theo ngày / camera / biển số
  - Xem chi tiết từng lần nhận dạng (ảnh + metadata + khu vực)

- **Báo cáo & Thống kê**
  - Báo cáo tổng hợp
  - Báo cáo theo loại xe (nếu có)
  - Báo cáo theo thời gian
  - Biểu đồ trực quan (Chart.js)

- **Xuất dữ liệu**
  - Form lọc dữ liệu cần xuất
  - Xuất Excel / PDF
  - Quản lý các file đã xuất
  - Danh sách jobs xuất dữ liệu

- **Nhật ký Audit**
  - Ghi lại mọi thao tác của người dùng
  - Lọc theo người dùng / hành động / thời gian
  - Giúp truy vết và bảo mật

## Công nghệ sử dụng

- **Framework**: ASP.NET Core MVC 8.0
- **Language**: C# 12
- **Database**: SQL Server (Entity Framework Core)
- **Frontend**:
  - HTML5, CSS3, JavaScript
  - Bootstrap 5 (UI Framework)
  - Chart.js (Biểu đồ thống kê)
  - SignalR (Real-time communication - tùy chọn)
- **OCR Engine**: Tesseract OCR
- **Image Processing**: OpenCV / Emgu.CV
- **Export Libraries**:
  - EPPlus / ClosedXML (Excel export)
  - iTextSharp / PdfSharp (PDF export)
- **Authentication**: ASP.NET Core Identity
- **Design Pattern**: MVC (Model-View-Controller)
- **Architecture**: Areas-based structure (Public / Admin)

## Yêu cầu hệ thống

### Phần mềm cần thiết

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) hoặc cao hơn
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (khuyến nghị) hoặc Visual Studio Code
- [SQL Server 2019](https://www.microsoft.com/sql-server) hoặc SQL Server Express
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract) (cài đặt và cấu hình)
- [Git](https://git-scm.com/) (tùy chọn)

### Cấu hình tối thiểu

- **OS**: Windows 10/11, Linux, macOS
- **RAM**: 4GB (khuyến nghị 8GB)
- **CPU**: 2 cores (khuyến nghị 4 cores)
- **Ổ cứng**: 1GB trống (cho ứng dụng và dữ liệu)
- **Camera**: IP Camera hoặc USB Camera (hỗ trợ RTSP/HTTP stream)

## Cài đặt

### 1. Clone repository

```bash
git clone [URL_REPOSITORY]
cd CĐTNDA_NhanDangBienSoXe
```

### 2. Restore packages

```bash
cd CĐTNDA_NhanDangBienSoXe
dotnet restore
```

### 3. Cài đặt Tesseract OCR

**Windows:**
- Download từ: https://github.com/UB-Mannheim/tesseract/wiki
- Cài đặt và thêm path vào biến môi trường
- Download language data (vie.traineddata) cho tiếng Việt

**Linux:**
```bash
sudo apt-get install tesseract-ocr
sudo apt-get install tesseract-ocr-vie
```

### 4. Cấu hình database

Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NGUYENKIENQUYET\\QUYET;Database=NhanDienBienSoXe;User Id=sa;Password=191104;TrustServerCertificate=True"
}
}
```

### 5. Chạy migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Chạy ứng dụng

```bash
dotnet run
```

Hoặc sử dụng Visual Studio: Nhấn `F5` hoặc `Ctrl + F5`

### 7. Truy cập ứng dụng

Mở trình duyệt và truy cập:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### 8. Đăng nhập lần đầu

Tài khoản mặc định (nếu có seed data):
- **Username**: admin
- **Password**: Admin@123

## Cấu hình

### Cấu hình Camera

Thêm thông tin camera trong mục **Quản lý Camera**:
- **Tên camera**: Ví dụ "Cổng chính", "Bãi đỗ A"
- **URL stream**: `rtsp://username:password@ip:port/stream` hoặc `http://ip/video.mjpeg`
- **Vị trí**: Mô tả vị trí lắp đặt
- **Khu vực**: Phân loại theo khu vực quản lý
- **Trạng thái**: Bật/Tắt camera

### Cấu hình OCR

Điều chỉnh các tham số OCR trong file `appsettings.json`:

```json
{
  "OcrSettings": {
    "TesseractPath": "C:/Program Files/Tesseract-OCR/tesseract.exe",
    "TessDataPath": "C:/Program Files/Tesseract-OCR/tessdata",
    "Language": "vie",
    "ConfidenceThreshold": 0.75,
    "MaxRetries": 3,
    "EnablePreprocessing": true
  }
}
```

### Cấu hình Upload

```json
{
  "UploadSettings": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".bmp"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

### Phân quyền người dùng

Hệ thống hỗ trợ các vai trò:
- **Admin**: Toàn quyền quản trị hệ thống
- **Manager**: Quản lý và xem báo cáo
- **User**: Nhân viên/bảo vệ - quét biển số và xem thống kê cơ bản
- **Viewer**: Chỉ xem (read-only)

## Sử dụng

### Đăng nhập

1. Truy cập trang chủ
2. Nhấn nút "Đăng nhập"
3. Nhập tên đăng nhập và mật khẩu
4. Hệ thống chuyển đến dashboard tương ứng với vai trò

### Nhận dạng biển số (User)

**Cách 1: Upload ảnh**
1. Vào mục "Nhận dạng" (`/Recognition`)
2. Chọn "Upload ảnh"
3. Chọn file ảnh từ máy tính
4. Nhấn "Nhận dạng"
5. Xem kết quả hiển thị

**Cách 2: Từ camera**
1. Vào mục "Camera" (`/Camera`)
2. Chọn camera cần xem
3. Xem live stream
4. Nhấn "Chụp ảnh" để capture
5. Hệ thống tự động nhận dạng

### Xem thống kê (User)

1. Vào mục "Thống kê" (`/Stats`)
2. Chọn khoảng thời gian
3. Chọn khu vực (nếu cần)
4. Xem biểu đồ và số liệu

### Quản lý người dùng (Admin)

1. Đăng nhập với tài khoản Admin
2. Vào "Admin" > "Người dùng" (`/Admin/Users`)
3. Nhấn "Thêm người dùng" để tạo mới
4. Điền thông tin và chọn vai trò
5. Lưu thông tin

### Xem báo cáo (Admin)

1. Vào "Admin" > "Báo cáo" (`/Admin/Reports`)
2. Chọn loại báo cáo:
   - Tổng quan (Overview)
   - Theo loại xe (ByVehicleType)
   - Theo thời gian (ByDate)
3. Chọn khoảng thời gian
4. Nhấn "Xem báo cáo" hoặc "Xuất file"

### Xuất dữ liệu (Admin)

1. Vào "Admin" > "Xuất dữ liệu" (`/Admin/Exports`)
2. Chọn điều kiện lọc:
   - Khoảng thời gian
   - Camera
   - Biển số (nếu có)
3. Chọn định dạng: Excel hoặc PDF
4. Nhấn "Xuất dữ liệu"
5. Download file khi hoàn thành

## Cấu trúc dự án

```
CĐTNDA_NhanDangBienSoXe/
│
├── Controllers/                          # Phần Public (Nhân viên / Bảo vệ)
│   ├── HomeController.cs                 # Trang chủ, dashboard nhỏ
│   ├── AccountController.cs              # Đăng nhập / đăng xuất
│   ├── RecognitionController.cs          # Quét biển số, hiển thị kết quả
│   ├── CameraController.cs               # Xem live camera, chụp ảnh
│   └── StatsController.cs                # Thống kê cơ bản (theo ngày, khu vực)
│
├── Areas/
│   └── Admin/
│       ├── Controllers/
│       │   ├── DashboardController.cs        # Tổng quan hệ thống (Dashboard)
│       │   ├── UsersController.cs            # Quản lý người dùng (Admin)
│       │   ├── RolesController.cs            # (Tuỳ chọn) phân quyền chi tiết
│       │   ├── RecognitionsController.cs     # Lịch sử nhận diện (lọc, xem chi tiết)
│       │   ├── ReportsController.cs          # Báo cáo, biểu đồ thống kê
│       │   ├── ExportsController.cs          # Xuất dữ liệu Excel / PDF
│       │   └── AuditController.cs            # Nhật ký thao tác người dùng
│       │
│       ├── Views/
│       │   ├── Shared/
│       │   │   └── _LayoutAdmin.cshtml       # Layout khu vực Admin
│       │   │
│       │   ├── Dashboard/
│       │   │   └── Index.cshtml              # Biểu đồ, tổng hợp hệ thống
│       │   │
│       │   ├── Users/
│       │   │   ├── Index.cshtml
│       │   │   ├── Create.cshtml
│       │   │   ├── Edit.cshtml
│       │   │   └── Details.cshtml
│       │   │
│       │   ├── Roles/
│       │   │   ├── Index.cshtml
│       │   │   ├── Create.cshtml
│       │   │   └── Edit.cshtml
│       │   │
│       │   ├── Recognitions/
│       │   │   ├── Index.cshtml              # Lọc theo ngày / camera / biển số
│       │   │   └── Details.cshtml            # Ảnh + meta + khu vực
│       │   │
│       │   ├── Reports/
│       │   │   ├── Overview.cshtml           # Tổng hợp thống kê
│       │   │   ├── ByVehicleType.cshtml      # Báo cáo theo loại xe (nếu cần)
│       │   │   └── ByDate.cshtml             # Báo cáo theo thời gian
│       │   │
│       │   ├── Exports/
│       │   │   ├── Index.cshtml              # Form lọc + nút xuất Excel/PDF
│       │   │   └── Jobs.cshtml               # Danh sách file đã xuất
│       │   │
│       │   └── Audit/
│       │       └── Index.cshtml              # Nhật ký thao tác người dùng
│       │
│       ├── _ViewStart.cshtml
│       └── _ViewImports.cshtml
│
├── Models/
│   ├── Recognition.cs                     # Bảng lưu kết quả nhận diện
│   ├── Camera.cs                          # Bảng thông tin camera
│   ├── User.cs                            # Bảng người dùng
│   ├── Role.cs                            # Bảng vai trò (Admin / User)
│   ├── AuditLog.cs                        # Nhật ký thao tác
│   └── AppDbContext.cs                    # DbContext (EF Core)
│
├── Services/
│   ├── PlateRecognitionService.cs         # Xử lý ảnh, detect biển số
│   ├── OcrService.cs                      # Đọc ký tự bằng Tesseract
│   └── ExportService.cs                   # Xử lý xuất Excel / PDF
│
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml                 # Layout Public
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Recognition/
│   │   └── Index.cshtml
│   ├── Stats/
│   │   └── Dashboard.cshtml
│   └── Account/
│       └── Login.cshtml
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/                               # Bootstrap, Chart.js, SignalR
│   └── uploads/                           # Lưu ảnh snapshot
│
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── CĐTNDA_NhanDangBienSoXe.csproj
```

## API Endpoints

### Public Area (User)

#### Authentication
- `GET /Account/Login` - Trang đăng nhập
- `POST /Account/Login` - Xử lý đăng nhập
- `POST /Account/Logout` - Đăng xuất
- `GET /Account/Register` - Đăng ký (nếu cho phép)

#### Home & Dashboard
- `GET /` hoặc `/Home` - Trang chủ
- `GET /Home/Index` - Dashboard nhỏ

#### Camera
- `GET /Camera` - Danh sách camera
- `GET /Camera/Live/{id}` - Xem live stream
- `POST /Camera/Snapshot/{id}` - Chụp ảnh từ camera

#### Recognition
- `GET /Recognition` - Trang nhận dạng
- `POST /Recognition/Upload` - Upload ảnh để nhận dạng
- `POST /Recognition/Process` - Xử lý nhận dạng
- `GET /Recognition/History` - Lịch sử nhận dạng (user hiện tại)

#### Stats
- `GET /Stats` - Trang thống kê
- `GET /Stats/Dashboard` - Dashboard thống kê
- `GET /Stats/ByDate` - Thống kê theo ngày
- `GET /Stats/ByArea` - Thống kê theo khu vực

### Admin Area

#### Dashboard
- `GET /Admin/Dashboard` - Dashboard tổng quan hệ thống
- `GET /Admin/Dashboard/Stats` - API lấy số liệu thống kê

#### Users Management
- `GET /Admin/Users` - Danh sách người dùng
- `GET /Admin/Users/Create` - Form tạo người dùng
- `POST /Admin/Users/Create` - Xử lý tạo người dùng
- `GET /Admin/Users/Edit/{id}` - Form sửa người dùng
- `POST /Admin/Users/Edit/{id}` - Cập nhật người dùng
- `GET /Admin/Users/Details/{id}` - Chi tiết người dùng
- `POST /Admin/Users/Delete/{id}` - Xóa người dùng

#### Roles Management
- `GET /Admin/Roles` - Danh sách vai trò
- `GET /Admin/Roles/Create` - Form tạo vai trò
- `POST /Admin/Roles/Create` - Xử lý tạo vai trò
- `GET /Admin/Roles/Edit/{id}` - Form sửa vai trò
- `POST /Admin/Roles/Edit/{id}` - Cập nhật vai trò
- `POST /Admin/Roles/Delete/{id}` - Xóa vai trò

#### Recognitions History
- `GET /Admin/Recognitions` - Danh sách lịch sử nhận diện
- `GET /Admin/Recognitions/Details/{id}` - Chi tiết nhận diện
- `POST /Admin/Recognitions/Filter` - Lọc kết quả (theo ngày/camera/biển số)
- `POST /Admin/Recognitions/Delete/{id}` - Xóa kết quả

#### Reports
- `GET /Admin/Reports` - Trang báo cáo
- `GET /Admin/Reports/Overview` - Báo cáo tổng hợp
- `GET /Admin/Reports/ByVehicleType` - Báo cáo theo loại xe
- `GET /Admin/Reports/ByDate` - Báo cáo theo thời gian
- `POST /Admin/Reports/Generate` - Tạo báo cáo tùy chỉnh

#### Exports
- `GET /Admin/Exports` - Form xuất dữ liệu
- `POST /Admin/Exports/Excel` - Xuất Excel
- `POST /Admin/Exports/Pdf` - Xuất PDF
- `GET /Admin/Exports/Jobs` - Danh sách file đã xuất
- `GET /Admin/Exports/Download/{id}` - Download file

#### Audit Logs
- `GET /Admin/Audit` - Nhật ký thao tác
- `POST /Admin/Audit/Filter` - Lọc theo user/hành động/thời gian

## Troubleshooting

### Lỗi kết nối database

**Vấn đề**: Không thể kết nối đến SQL Server

**Giải pháp**:
- Kiểm tra SQL Server đã chạy chưa (`services.msc`)
- Kiểm tra connection string trong `appsettings.json`
- Kiểm tra tên server và database
- Chạy lại migration: `dotnet ef database update`
- Kiểm tra firewall có block SQL Server không

### Lỗi Tesseract OCR

**Vấn đề**: OCR không nhận dạng được hoặc báo lỗi

**Giải pháp**:
- Kiểm tra Tesseract đã cài đặt đúng chưa
- Kiểm tra path trong `appsettings.json`
- Download language data file (vie.traineddata)
- Đặt file vào thư mục tessdata
- Kiểm tra quyền truy cập thư mục

### Lỗi nhận dạng sai

**Vấn đề**: OCR trả về kết quả sai hoặc không chính xác

**Giải pháp**:
- Kiểm tra chất lượng ảnh đầu vào
- Đảm bảo ảnh có độ sáng và độ tương phản tốt
- Điều chỉnh `ConfidenceThreshold` trong cấu hình (giảm xuống 0.6-0.7)
- Bật preprocessing: `EnablePreprocessing: true`
- Crop ảnh chỉ lấy vùng biển số
- Tăng resolution ảnh

### Lỗi camera không kết nối

**Vấn đề**: Không thể kết nối đến camera hoặc không hiển thị stream

**Giải pháp**:
- Kiểm tra URL/IP camera có đúng không
- Test URL bằng VLC Player trước
- Kiểm tra username/password camera
- Kiểm tra firewall có block không
- Kiểm tra camera có đang hoạt động không
- Thử sử dụng protocol khác (RTSP/HTTP)

### Lỗi upload file

**Vấn đề**: Không thể upload ảnh

**Giải pháp**:
- Kiểm tra dung lượng file (mặc định max 10MB)
- Kiểm tra định dạng file (.jpg, .jpeg, .png, .bmp)
- Kiểm tra quyền ghi thư mục `wwwroot/uploads`
- Tạo thư mục uploads nếu chưa có
- Kiểm tra cấu hình `MaxFileSize` trong appsettings.json

### Lỗi xuất Excel/PDF

**Vấn đề**: Không thể xuất file hoặc file bị lỗi

**Giải pháp**:
- Kiểm tra thư viện EPPlus/ClosedXML đã cài đặt chưa
- Kiểm tra license (EPPlus cần license cho commercial use)
- Kiểm tra quyền ghi file
- Kiểm tra dung lượng dữ liệu (quá lớn có thể timeout)

## Database Schema

Hệ thống sử dụng schema `pr` (Plate Recognition) để tổ chức các bảng.

### 1. Quản lý Người dùng & Phân quyền

#### pr.Roles
- **RoleId** (PK, INT IDENTITY)
- **Name** (NVARCHAR(50), UNIQUE) - Tên vai trò: Admin, User
- **Description** (NVARCHAR(255)) - Mô tả vai trò

#### pr.Users
- **UserId** (PK, INT IDENTITY)
- **UserName** (NVARCHAR(100), UNIQUE) - Tên đăng nhập
- **PasswordHash** (NVARCHAR(256)) - Mật khẩu đã hash (bcrypt/argon2)
- **FullName** (NVARCHAR(150)) - Họ tên
- **Email** (NVARCHAR(150)) - Email
- **Phone** (NVARCHAR(50)) - Số điện thoại
- **IsActive** (BIT, DEFAULT 1) - Trạng thái hoạt động
- **LastLoginAt** (DATETIME2(0)) - Lần đăng nhập cuối
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())

#### pr.UserRoles
- **UserId** (PK, FK → Users)
- **RoleId** (PK, FK → Roles)

#### pr.AuditLog
- **AuditId** (PK, BIGINT IDENTITY)
- **UserId** (FK → Users) - Người thực hiện
- **Action** (NVARCHAR(100)) - CREATE/UPDATE/DELETE/EXPORT/LOGIN
- **Entity** (NVARCHAR(100)) - Camera, Recognition, Vehicle...
- **EntityId** (NVARCHAR(64)) - ID đối tượng
- **Detail** (NVARCHAR(MAX)) - JSON chi tiết trước/sau
- **IpAddress** (NVARCHAR(64))
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())

### 2. Quản lý Khu vực & Camera

#### pr.Areas
- **AreaId** (PK, INT IDENTITY)
- **Name** (NVARCHAR(150)) - Tên khu vực
- **ParentAreaId** (FK → Areas) - Khu vực cha (phân cấp)

#### pr.Cameras
- **CameraId** (PK, INT IDENTITY)
- **Name** (NVARCHAR(150)) - Tên camera
- **AreaId** (FK → Areas) - Khu vực lắp đặt
- **LocationNote** (NVARCHAR(255)) - Mô tả vị trí
- **IpAddress** (NVARCHAR(64)) - Địa chỉ IP
- **StreamUrl** (NVARCHAR(500)) - URL stream (RTSP/HTTP)
- **IsActive** (BIT, DEFAULT 1) - Trạng thái hoạt động
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())

### 3. Quản lý Phương tiện

#### pr.Vehicles
- **VehicleId** (PK, BIGINT IDENTITY)
- **PlateNumber** (NVARCHAR(32)) - Biển số gốc (hiển thị)
- **PlateNorm** (NVARCHAR(32), UNIQUE) - Biển số chuẩn hóa (bỏ dấu, viết liền)
- **Type** (NVARCHAR(30)) - Loại xe: Car/Motorbike/Truck
- **Color** (NVARCHAR(30)) - Màu xe
- **Make** (NVARCHAR(60)) - Hãng sản xuất
- **Model** (NVARCHAR(60)) - Model xe
- **OwnerName** (NVARCHAR(150)) - Tên chủ xe
- **OwnerPhone** (NVARCHAR(50)) - SĐT chủ xe
- **Notes** (NVARCHAR(255)) - Ghi chú
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())

#### pr.VehicleTags
- **TagId** (PK, INT IDENTITY)
- **Name** (NVARCHAR(50), UNIQUE) - Internal, VIP, Blacklist, Guest
- **ColorHex** (CHAR(7)) - Mã màu hiển thị (#1abc9c)
- **Description** (NVARCHAR(255)) - Mô tả tag

#### pr.VehicleTagMaps
- **VehicleId** (PK, FK → Vehicles)
- **TagId** (PK, FK → VehicleTags)
- **ValidFrom** (PK, DATETIME2(0)) - Hiệu lực từ
- **ValidTo** (DATETIME2(0)) - Hiệu lực đến

### 4. Kết quả Nhận dạng (Core)

#### pr.Recognitions
- **RecognitionId** (PK, BIGINT IDENTITY)
- **CameraId** (FK → Cameras) - Camera ghi nhận
- **DetectedAt** (DATETIME2(0)) - Thời gian phát hiện (UTC)
- **PlateTextRaw** (NVARCHAR(64)) - Chuỗi OCR thô
- **PlateNorm** (NVARCHAR(32)) - Biển số chuẩn hóa
- **Confidence** (DECIMAL(5,2)) - Độ tin cậy (0-100)
- **Direction** (NVARCHAR(10)) - Hướng: In/Out
- **Region** (NVARCHAR(10)) - VN-std
- **OcrEngine** (NVARCHAR(50)) - Tesseract, Paddle...
- **OcrVersion** (NVARCHAR(50)) - Version OCR engine
- **ProcessingMs** (INT) - Thời gian xử lý (ms)
- **VehicleId** (FK → Vehicles) - Xe khớp trong danh mục
- **BestTagId** (FK → VehicleTags) - Tag ưu tiên (VIP/Blacklist)
- **ImagePath** (NVARCHAR(500)) - Đường dẫn ảnh toàn khung
- **PlateCropPath** (NVARCHAR(500)) - Đường dẫn ảnh crop biển số
- **BBoxesJson** (NVARCHAR(MAX)) - Tọa độ hộp (JSON)
- **HashDedup** (VARBINARY(32), UNIQUE) - Hash chống trùng
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())

**Indexes:**
- `IX_Rec_DetectedAt` - Sắp xếp theo thời gian
- `IX_Rec_Camera_Time` - Lọc theo camera + thời gian
- `IX_Rec_PlateNorm_Time` - Tìm theo biển số + thời gian
- `UX_Rec_HashDedup` - Unique hash chống duplicate

### 5. Cảnh báo & Thông báo

#### pr.Alerts
- **AlertId** (PK, BIGINT IDENTITY)
- **RecognitionId** (FK → Recognitions) - Kết quả nhận dạng
- **AlertType** (NVARCHAR(50)) - BlacklistHit, VVIPArrived...
- **Severity** (TINYINT) - Mức độ nghiêm trọng (1-5)
- **Message** (NVARCHAR(255)) - Nội dung cảnh báo
- **CreatedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())
- **AcknowledgedBy** (FK → Users) - Người xác nhận
- **AcknowledgedAt** (DATETIME2(0)) - Thời gian xác nhận

### 6. Xuất dữ liệu & Thống kê

#### pr.ExportJobs
- **JobId** (PK, BIGINT IDENTITY)
- **RequestedBy** (FK → Users) - Người yêu cầu
- **RequestedAt** (DATETIME2(0), DEFAULT SYSUTCDATETIME())
- **FiltersJson** (NVARCHAR(MAX)) - Điều kiện lọc (JSON)
- **FilePath** (NVARCHAR(500)) - Đường dẫn file xuất
- **Format** (NVARCHAR(10)) - XLSX/PDF
- **Status** (NVARCHAR(20), DEFAULT 'Pending') - Pending/Done/Failed
- **CompletedAt** (DATETIME2(0)) - Thời gian hoàn thành

#### pr.StatsDaily
- **StatDate** (PK, DATE) - Ngày thống kê
- **CameraId** (PK, FK → Cameras) - Camera (0 = tất cả)
- **AreaId** (PK, FK → Areas) - Khu vực (0 = tất cả)
- **TotalCount** (INT) - Tổng số lượt quét
- **UniquePlates** (INT) - Số biển số unique
- **BlacklistHits** (INT, DEFAULT 0) - Số lần trúng blacklist

### Quan hệ giữa các bảng

```
Users ──┬─→ UserRoles ←── Roles
        ├─→ AuditLog
        ├─→ ExportJobs
        └─→ Alerts (AcknowledgedBy)

Areas ──┬─→ Cameras ──→ Recognitions ──┬─→ Alerts
        └─→ StatsDaily                  │
                                        ├─→ Vehicles ──→ VehicleTagMaps ←── VehicleTags
                                        └─→ VehicleTags (BestTagId)
```

## Đóng góp

Nếu bạn muốn đóng góp cho dự án:

1. Fork repository
2. Tạo branch mới (`git checkout -b feature/TenTinhNang`)
3. Commit thay đổi (`git commit -m 'Thêm tính năng xyz'`)
4. Push lên branch (`git push origin feature/TenTinhNang`)
5. Tạo Pull Request

## Roadmap

### Phiên bản tương lai

- [ ] Hỗ trợ nhận dạng real-time từ video stream
- [ ] Tích hợp AI/ML để cải thiện độ chính xác
- [ ] Hỗ trợ nhiều loại biển số (quốc tế)
- [ ] Mobile app (iOS/Android)
- [ ] API RESTful cho tích hợp hệ thống khác
- [ ] Hỗ trợ đa ngôn ngữ (i18n)
- [ ] Báo cáo nâng cao với AI insights
- [ ] Tích hợp với hệ thống barrier/cửa tự động

## Tác giả

[Tên của bạn/Nhóm]

Dự án đồ án tốt nghiệp - [Tên trường/Khoa]

## License

Dự án này được phát triển cho mục đích học tập và nghiên cứu.

**Lưu ý**: Vui lòng không sử dụng cho mục đích thương mại khi chưa có sự cho phép.

## Liên hệ

- Email: [email@example.com]
- GitHub: [github-username]
- Website: [website]

## Tài liệu tham khảo

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Tesseract OCR Documentation](https://github.com/tesseract-ocr/tesseract)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5](https://getbootstrap.com/docs/5.0)
- [Chart.js](https://www.chartjs.org/docs/latest/)

---

**Copyright © 2024 - Hệ Thống Nhận Dạng Biển Số Xe**
