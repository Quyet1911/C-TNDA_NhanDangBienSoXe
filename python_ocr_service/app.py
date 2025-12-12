"""
Python OCR Microservice cho License Plate Recognition
Dùng EasyOCR - Free, Offline, Độ chính xác cao

Cài đặt:
pip install flask easyocr opencv-python pillow numpy

Chạy:
python app.py

API sẽ chạy tại: http://localhost:5001
"""

from flask import Flask, request, jsonify
import easyocr
import cv2
import numpy as np
import time
import os
from PIL import Image
import io

app = Flask(__name__)

# Khởi tạo EasyOCR reader (support tiếng Việt và English)
# Lần đầu chạy sẽ tải models (~ 1-2 phút)
print("Loading EasyOCR models...")
reader = easyocr.Reader(['en'], gpu=False)  # Dùng CPU (gpu=True nếu có GPU)
print("EasyOCR ready!")


def preprocess_image(image):
    """Xử lý ảnh để cải thiện OCR"""
    # Convert sang grayscale
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Tăng contrast
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    enhanced = clahe.apply(gray)

    # Denoise
    denoised = cv2.fastNlMeansDenoising(enhanced, h=10)

    # Threshold
    _, thresh = cv2.threshold(denoised, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)

    return thresh


def clean_plate_text(text):
    """Làm sạch text biển số"""
    import re

    # Loại bỏ khoảng trắng, ký tự đặc biệt
    text = re.sub(r'[^A-Z0-9]', '', text.upper())

    # Sửa các lỗi OCR phổ biến
    replacements = {
        'O': '0',  # Chữ O -> số 0
        'I': '1',  # Chữ I -> số 1
        'S': '5',  # Chữ S -> số 5 (nếu ở vị trí số)
        'B': '8',  # Chữ B -> số 8 (nếu ở vị trí số)
    }

    # Áp dụng rules cho biển số VN: 2 chữ cái đầu, sau đó là số
    if len(text) >= 6:
        # Giữ nguyên 2 ký tự đầu (chữ cái)
        prefix = text[:2]
        # Phần còn lại là số
        suffix = text[2:]
        for old, new in replacements.items():
            suffix = suffix.replace(old, new)
        text = prefix + suffix

    return text


@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "service": "EasyOCR License Plate Recognition"})


@app.route('/recognize', methods=['POST'])
def recognize():
    """
    Nhận dạng biển số từ ảnh upload

    Request: multipart/form-data với field 'image'
    Response: JSON với plate text, confidence, processing time
    """
    start_time = time.time()

    try:
        # Kiểm tra file upload
        if 'image' not in request.files:
            return jsonify({
                'success': False,
                'error': 'No image file provided'
            }), 400

        file = request.files['image']

        # Đọc ảnh
        image_bytes = file.read()
        nparr = np.frombuffer(image_bytes, np.uint8)
        image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        if image is None:
            return jsonify({
                'success': False,
                'error': 'Invalid image file'
            }), 400

        print(f"Image size: {image.shape[1]}x{image.shape[0]}")

        # Xử lý ảnh
        processed = preprocess_image(image)

        # OCR với EasyOCR
        results = reader.readtext(processed, detail=1, paragraph=False)

        if not results:
            # Thử lại với ảnh gốc
            print("No results from processed image, trying original...")
            results = reader.readtext(image, detail=1, paragraph=False)

        if not results:
            processing_time = int((time.time() - start_time) * 1000)
            return jsonify({
                'success': False,
                'error': 'No text detected',
                'processing_time_ms': processing_time
            })

        # Lấy kết quả có confidence cao nhất
        best_result = max(results, key=lambda x: x[2])  # x[2] là confidence
        text = best_result[1]
        confidence = best_result[2]
        bbox = best_result[0]  # [[x1,y1], [x2,y2], [x3,y3], [x4,y4]]

        # Làm sạch text
        cleaned_text = clean_plate_text(text)

        processing_time = int((time.time() - start_time) * 1000)

        print(f"OCR Result: '{text}' -> '{cleaned_text}' (confidence: {confidence:.2f})")

        return jsonify({
            'success': True,
            'plate_text': cleaned_text,
            'raw_text': text,
            'confidence': float(confidence * 100),  # Convert to percentage
            'processing_time_ms': processing_time,
            'engine': 'EasyOCR',
            'bbox': {
                'x': int(bbox[0][0]),
                'y': int(bbox[0][1]),
                'width': int(bbox[2][0] - bbox[0][0]),
                'height': int(bbox[2][1] - bbox[0][1])
            }
        })

    except Exception as e:
        processing_time = int((time.time() - start_time) * 1000)
        print(f"Error: {str(e)}")
        return jsonify({
            'success': False,
            'error': str(e),
            'processing_time_ms': processing_time
        }), 500


if __name__ == '__main__':
    # Chạy Flask server
    print("\n" + "="*60)
    print("🚀 Python OCR Service Starting...")
    print("="*60)
    print(f"📍 API URL: http://localhost:5001")
    print(f"❤️  Health Check: http://localhost:5001/health")
    print(f"🔍 Recognize: POST http://localhost:5001/recognize")
    print("="*60 + "\n")

    app.run(host='0.0.0.0', port=5001, debug=False)
