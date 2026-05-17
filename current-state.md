# Nhật Ký Phát Triển Dự Án FILMIX - Cập nhật ngày 17/05/2026

## 1. Các công việc và chỉnh sửa đã hoàn thành hôm nay

### 🆕 Thêm Danh mục mới ("Phim Lẻ" & "Mới & Hot")
- Tạo `MoviesController.cs` và `NewHotController.cs`.
- Thiết kế giao diện riêng biệt với `movies.css` và `newhot.css`.
- Xây dựng Razor Views (`Views/Movies/Index.cshtml`, `Views/NewHot/Index.cshtml`).
- Cập nhật liên kết trên thanh điều hướng (`_Layout.cshtml`).

### 🛠 Cấu trúc Database & Kiến trúc mới (Movies Entity)
- Sửa lỗi Namespace không đồng nhất gây lỗi biên dịch.
- Hỗ trợ đa nền tảng cơ sở dữ liệu: Thêm tùy chọn `DbProvider` trong `appsettings.json` để chuyển đổi dễ dàng giữa **MySQL** và **SQL Server** cho các thành viên trong team.
- Tái cấu trúc thực thể `Movie`: 
  - Thêm quan hệ nhiều-nhiều (Many-to-Many) với `Category` thông qua bảng `MovieCategory`.
  - Thêm quan hệ một-nhiều (One-to-Many) với `Episode` để hỗ trợ hiển thị danh sách các tập phim cho TV Series.
- Đồng bộ dữ liệu hạt giống (Seed Data) của DB với các phim hiển thị ngoài trang chủ để không bị lỗi.

### 🎬 Nâng cấp giao diện & Tính năng (Detail Page & Watchlist)
- Tạo trang **Chi tiết phim chuẩn Netflix** (`/Product/Detail/{id}`) với hero banner lớn, danh sách thể loại động, bộ chọn Season và các tập phim.
- Khắc phục lỗi **không click được vào thẻ phim**: Đã bọc các thẻ phim ở Trang Chủ, trang TV Shows và Danh Sách bằng thẻ `<a>`.
- Thiết kế giao diện **Tab 2 cột chuyên nghiệp** cho trang `/Product/List`:
  - **Tab 1:** Khám Phá Phim (mặc định, lọc theo thể loại).
  - **Tab 2:** Danh Sách Của Tôi (hiển thị phim được lưu trong bộ nhớ máy khách).

### 🐛 Fix Bugs (Các lỗi ẩn đã xử lý)
- **Lỗi Cartesian Explosion trong EF Core**: Khi fetch Movies kèm theo `Episodes` và `MovieCategories`, ứng dụng bị crash. Đã fix triệt để bằng cách thêm `.AsSplitQuery()` vào ProductController.
- **Lỗi LocalStorage "Danh Sách Của Tôi" (Watchlist)**: Nút "Lưu Danh Sách" trước đó chỉ có hiệu ứng UI mà không lưu ID vào bộ nhớ trình duyệt, khiến Danh Sách Của Tôi luôn trống rỗng. Đã sửa lại JavaScript trong `Detail.cshtml` để đọc/ghi vào `localStorage` chính xác.

---

## 2. Các Bug / Yêu cầu tính năng còn tồn đọng cần xử lý tiếp

### ⚠️ Chưa đồng bộ thiết kế Nút "Lưu vào danh sách"
- **Vấn đề**: Các nút thêm vào danh sách ở phần **"Mới & Hot"** và **"Phim Lẻ"** hiện chưa có thiết kế đồng nhất với nút lưu danh sách của mục **TV Shows**.
- **Yêu cầu**: Cần cập nhật lại mã HTML/CSS của các nút này để thiết kế giống với giao diện của TV Shows.

### 🌐 Thiếu tính năng Chuyển đổi Ngôn ngữ (Language Switcher)
- **Vấn đề**: Hệ thống hiện tại đang hiển thị ngôn ngữ mặc định và chưa có cơ chế thay đổi ngôn ngữ động toàn cục.
- **Yêu cầu**: Cần tích hợp nút chuyển đổi ngôn ngữ, khi người dùng đổi ngôn ngữ thì **toàn bộ UI của project sẽ thay đổi tức thì** (sử dụng thư viện đa ngôn ngữ hoặc i18n js).

---
**Trạng thái hiện tại: 🟢 Ổn định (Stable)** nhưng còn một số tính năng Frontend chưa hoàn thành theo yêu cầu mới nhất.
