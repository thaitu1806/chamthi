# 🎓 MathExamGrader - Chấm Bài Thi Toán bằng Gemini AI

Tool desktop dành cho giáo viên Toán, sử dụng Gemini Web (miễn phí, không giới hạn) để chấm bài thi học sinh tự động.

## ✨ Tính năng

- 📁 Kéo thả nhiều file bài thi (DOCX, PDF, ảnh PNG/JPG)
- 🤖 Gemini AI tự đọc và chấm bài Toán
- 📋 Hỗ trợ chấm có đáp án hoặc AI tự chấm
- 📊 Bảng kết quả + xuất CSV
- 🔗 Tùy chọn submit lên Google Form (lưu Google Sheets)
- 🆓 Hoàn toàn miễn phí (dùng Gemini Web qua Playwright)

## 🚀 Cài đặt & Chạy

### Yêu cầu
- .NET 8 SDK trở lên
- Microsoft Edge (đã cài sẵn trên Windows)
- Tài khoản Google (đã đăng nhập trên Edge)

### Bước 1: Cài Playwright browsers
```bash
dotnet build
pwsh bin/Debug/net8.0-windows/playwright.ps1 install
```

### Bước 2: Đăng nhập Google trên Edge
Mở Edge, đăng nhập Google tại https://gemini.google.com và chấp nhận điều khoản.

### Bước 3: Chạy ứng dụng
```bash
dotnet run
```

## 📖 Hướng dẫn sử dụng

### 1. Nhập tên bài thi
Ví dụ: "Kiểm tra 1 tiết Đại số lớp 10"

### 2. Nhập đáp án (tùy chọn)
- Nếu có đáp án: Nhập vào ô "Đáp án" (VD: Câu 1: A, Câu 2: x=3, ...)
- Nếu không có: Để trống, Gemini sẽ tự đọc đề và chấm

### 3. Thêm bài thi học sinh
- **Kéo thả** file vào vùng kéo thả
- Hoặc click **📂 Thêm** để chọn file
- Hỗ trợ: .docx, .pdf, .png, .jpg, .jpeg, .bmp

### 4. Google Form (tùy chọn)
Nếu muốn lưu kết quả lên Google Sheets:
- Tạo Google Form với các field: Tên HS, Bài thi, Điểm, Nhận xét, Chi tiết, Ngày
- Dán URL form vào ô "Google Form URL"

### 5. Nhấn "Bắt đầu chấm bài"
- Browser Edge sẽ mở tự động
- Tool sẽ lần lượt upload từng bài lên Gemini
- Kết quả hiển thị trên bảng + log

### 6. Xuất kết quả
Click **💾 CSV** để xuất file CSV (mở được bằng Excel)

## ⚠️ Lưu ý

- Lần đầu chạy, browser có thể yêu cầu đăng nhập Google
- Mỗi bài mất khoảng 30-60 giây để chấm
- Nên đặt tên file = tên học sinh (VD: `NguyenVanA.jpg`, `TranThiB.pdf`)
- Gemini có thể đọc sai chữ viết tay khó → nên dùng ảnh rõ nét
- Nếu chấm bài tự luận, nên có đáp án để kết quả chính xác hơn

## 📁 Cấu trúc project

```
MathExamGrader/
├── Form1.cs                    # UI chính + logic điều khiển
├── Form1.Designer.cs           # Layout giao diện
├── Program.cs                  # Entry point
├── Models/
│   ├── ExamResult.cs          # Model kết quả chấm
│   └── GradingConfig.cs       # Cấu hình
├── Services/
│   ├── GeminiWebService.cs    # Playwright → Gemini Web
│   ├── GoogleFormService.cs   # Submit Google Form
│   └── ResultParserService.cs # Parse kết quả từ Gemini
└── README.md
```

## 🔧 Tùy chỉnh

### Thay đổi prompt chấm bài
Mở file `Services/GeminiWebService.cs`, sửa method `BuildGradingPrompt()`.

### Cấu hình Google Form
Mở file `Services/GoogleFormService.cs`, cập nhật các `entry.X` theo form thực tế.
(Mở Google Form → F12 → Inspect từng field → Tìm `entry.XXXXXXX`)
