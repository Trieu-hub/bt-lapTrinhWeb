# Nhật Ký Phát Triển Dự Án FILMIX - 16/05/2026

## 1. Các công việc đã hoàn thành

### 🖼️ Địa phương hóa hình ảnh (Localizing Assets)
- Tải toàn bộ ảnh phim từ các nguồn ngoài (Picsum, Wikipedia) về thư mục `wwwroot/images/`.
- Phân chia thư mục khoa học: `/hero`, `/movies`, `/tvshows`, `/auth`.
- Cập nhật toàn bộ mã nguồn để sử dụng đường dẫn cục bộ, giúp trang web chạy nhanh và ổn định hơn.

### 📺 Trang TV Shows (Netflix Style)
- Xây dựng trang `/TVShows` với giao diện cuộn ngang đặc trưng của Netflix.
- Có Hero section nổi bật cho phim "Stranger Things".
- Đã kết nối với Database để lấy danh sách phim động.

### 🔍 Tính năng lọc theo Danh mục (Product Filtering)
- Xây dựng trang `/Product/List` cho phép xem danh sách phim.
- Tích hợp bộ lọc theo `categoryId` (Hành động, Kinh dị, Viễn tưởng...).
- Giao diện lưới (Grid) đáp ứng, tự động co giãn theo màn hình.

### 🗄️ Tích hợp Cơ sở dữ liệu MySQL
- Kết nối thành công với MySQL (User: `root`, Pass: `123456`).
- Tự động tạo Database `filmix_db` và các bảng `Categories`, `Movies` khi khởi chạy ứng dụng.
- Đã nạp dữ liệu mẫu (Seeding) trực tiếp vào DB.
- Chuyển đổi logic từ dữ liệu "cứng" sang truy vấn SQL thực tế.

### 📁 Tổ chức lại thư mục (Refactoring)
- Sắp xếp lại mã nguồn theo chuẩn chuyên nghiệp:
    - `Models/Entities/`: Chứa các thực thể dữ liệu.
    - `Models/ViewModels/`: Chứa các Model phục vụ hiển thị.
- Cập nhật toàn bộ Namespace và các câu lệnh `using` để hệ thống không bị lỗi sau khi di chuyển file.

---

## 2. Các lỗi tiềm ẩn & Lưu ý (Potential Bugs)

### ⚠️ Khởi động ứng dụng (Startup Crash)
- **Vấn đề**: Nếu dịch vụ MySQL (Server) chưa được bật hoặc sai mật khẩu, ứng dụng sẽ báo lỗi ngay khi khởi động do lệnh `db.Database.EnsureCreated()` được gọi sớm.
- **Cách khắc phục**: Đảm bảo MySQL Server đang chạy trước khi F5 project.

### ⚠️ Xung đột dữ liệu mẫu (Seeding Conflicts)
- **Vấn đề**: Lệnh `EnsureCreated()` và `HasData` chỉ hoạt động tốt cho lần đầu tiên. Nếu bạn thay đổi cấu trúc bảng sau này, bạn nên chuyển sang dùng **Migrations** (`dotnet ef migrations`) thay vì cách khởi tạo tự động này.

### ⚠️ Đường dẫn ảnh (Image Paths)
- **Vấn đề**: Hiện tại ảnh được fix cứng đuôi `.jpg`. Nếu sau này bạn thay thế bằng các ảnh định dạng `.png` hoặc `.webp`, bạn cần cập nhật lại cột `ImageUrl` trong database.

### ⚠️ Hiển thị TV Shows
- **Vấn đề**: Trang TV Shows hiện đang lấy "tất cả phim" trong bảng `Movies` để hiển thị (chưa lọc riêng thể loại Series).
- **Cải thiện**: Sau này cần thêm một cột `IsTVSeries` vào bảng `Movies` để lọc chính xác hơn.

---
**Trạng thái hiện tại: 🟢 Ổn định (Stable)**
Project đã sẵn sàng để phát triển thêm các tính năng như: Đăng nhập thực tế, Xem chi tiết phim, hoặc Tìm kiếm phim.
