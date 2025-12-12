# 🐍 Python EasyOCR Microservice cho Nhận Dạng Biển Số

## 📚 Giới thiệu

Service này sử dụng **EasyOCR** - thư viện OCR mạnh mẽ, miễn phí và có độ chính xác cao (85-95%) cho nhận dạng biển số xe.

**Ưu điểm:**
- ✅ **FREE** và **Offline** hoàn toàn
- ✅ Độ chính xác: **85-95%** (tốt hơn Tesseract nhiều)
- ✅ Hỗ trợ nhiều ngôn ngữ (English, tiếng Việt)
- ✅ Không giới hạn số lượng requests
- ✅ Chạy local, không cần internet

---

## 🚀 Cài đặt

### Bước 1: Cài Python (nếu chưa có)

Download Python 3.8 - 3.11 tại: https://www.python.org/downloads/

**Lưu ý:** Tích chọn "Add Python to PATH" khi cài!

### Bước 2: Cài các thư viện cần thiết

Mở **Command Prompt** hoặc **PowerShell**, cd vào thư mục này:

```bash
cd "D:\C#\CĐTNDA_NhanDangBienSoXe\python_ocr_service"
```

Cài dependencies:

```bash
pip install -r requirements.txt
```

**Lưu ý:** Lần đầu cài sẽ mất khoảng **5-10 phút** (download models ~500MB)

---

## ▶️ Chạy service

### Khởi động Python OCR Service:

```bash
python app.py
```

Nếu thành công, bạn sẽ thấy:

```
Loading EasyOCR models...
EasyOCR ready!

============================================================
🚀 Python OCR Service Starting...
============================================================
📍 API URL: http://localhost:5001
❤️  Health Check: http://localhost:5001/health
🔍 Recognize: POST http://localhost:5001/recognize
============================================================

* Running on http://0.0.0.0:5001
```

**Lưu service chạy ở cửa sổ này!** Không tắt.

---

## 🎯 Sử dụng

### 1. **Kiểm tra service hoạt động:**

Mở browser, truy cập: http://localhost:5001/health

Nếu thấy:
```json
{
  "status": "healthy",
  "service": "EasyOCR License Plate Recognition"
}
```

→ Service đã sẵn sàng! ✅

### 2. **Chạy C# Application:**

Mở terminal mới (giữ Python service chạy), cd vào project C#:

```bash
cd "D:\C#\CĐTNDA_NhanDangBienSoXe\CĐTNDA_NhanDangBienSoXe"
dotnet run
```

Hoặc chạy từ Visual Studio (F5)

### 3. **Test nhận dạng:**

1. Truy cập: https://localhost:XXXX
2. Login vào hệ thống
3. Upload ảnh biển số
4. Nhấn "Nhận dạng"

→ EasyOCR sẽ xử lý và trả về kết quả! 🎉

---

## ⚙️ Cấu hình

### Thay đổi Provider trong `appsettings.json`:

```json
"Ocr": {
  "Provider": "EasyOCR",  // Hoặc "PlateRecognizer", "Tesseract"
  "EasyOcrServiceUrl": "http://localhost:5001"
}
```

### So sánh các Provider:

| Provider | Độ chính xác | Chi phí | Offline | Setup |
|----------|-------------|---------|---------|-------|
| **EasyOCR** | 85-95% | FREE | ✅ | 10 phút |
| PlateRecognizer | 95-99% | 2500 free/tháng | ❌ | 5 phút |
| Tesseract | 60-75% | FREE | ✅ | 5 phút |

---

## 🐛 Troubleshooting

### Lỗi: "Không kết nối được tới Python OCR Service"

**Nguyên nhân:** Python service chưa chạy

**Giải pháp:**
```bash
cd "D:\C#\CĐTNDA_NhanDangBienSoXe\python_ocr_service"
python app.py
```

### Lỗi: "ModuleNotFoundError: No module named 'easyocr'"

**Nguyên nhân:** Chưa cài thư viện

**Giải pháp:**
```bash
pip install -r requirements.txt
```

### Lỗi: Port 5001 đã được sử dụng

**Giải pháp:** Đổi port trong `app.py` (dòng cuối):
```python
app.run(host='0.0.0.0', port=5002, debug=False)  # Đổi 5001 -> 5002
```

Và update `appsettings.json`:
```json
"EasyOcrServiceUrl": "http://localhost:5002"
```

---

## 📊 Performance

- **Thời gian xử lý:** 2-5 giây/ảnh (CPU)
- **Thời gian xử lý:** 0.5-1 giây/ảnh (GPU)
- **RAM usage:** ~1-2GB
- **GPU:** Tùy chọn (nếu có NVIDIA GPU, set `gpu=True` trong `app.py`)

---

## 🆘 Support

Nếu gặp vấn đề, kiểm tra:
1. ✅ Python đã cài chưa? `python --version`
2. ✅ Thư viện đã cài chưa? `pip list | grep easyocr`
3. ✅ Service có chạy không? Truy cập http://localhost:5001/health
4. ✅ Firewall có block port 5001 không?

---

Chúc bạn thành công! 🚀
