# CHƯƠNG 3. PHÁT TRIỂN WEBSITE Y TẾ TTYT PHƯỜNG KINH MÔN

Trên cơ sở phần thiết kế đã trình bày ở Chương 2, đồ án tiến hành phát triển website trên nền tảng ASP.NET Core 8 kết hợp Razor MVC, Entity Framework Core 8 và SQL Server. Toàn bộ giao diện được phát triển theo chuẩn responsive web design, được kiểm thử song song trên cấu hình desktop (1366 × 768) và hai mẫu điện thoại phổ biến iPhone X (375 × 812) và iPhone SE (320 × 568). Chương này mô tả bảy nhóm trang chính của hệ thống.

## 3.1. Trang chủ (Public)

### 3.1.1. Bố cục

Trang chủ được thiết kế theo bố cục chuẩn của các website y tế công lập tại Việt Nam: Header (logo + menu) → Hero banner → Khu vực giới thiệu nhanh → Danh sách dịch vụ → Tin tức mới nhất → Footer (thông tin liên hệ).

### 3.1.2. Giao diện thực tế

![Hình 3.1. Giao diện Trang chủ trên desktop 1366 × 768](images/hinh-3-2.png){width=14cm}

![Hình 3.2. Giao diện Trang chủ trên iPhone X 375 × 812](images/hinh-3-3.png){width=7.5cm}

### 3.1.3. Đặc tả chức năng

**Bảng 3.1. Đặc tả chức năng Trang chủ**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Header logo | Hiển thị logo + tên *"Trung tâm Y tế phường Kinh Môn"*, click trở về trang chủ |
| 2 | Menu chính | 6 mục: Trang chủ, Bác sĩ, Chuyên khoa, Tin tức, Hỏi đáp, Liên hệ |
| 3 | Nút *"Đặt lịch khám"* | Nổi bật ở header, dẫn tới `/dat-lich-kham` |
| 4 | Nút Đăng nhập / Đăng ký | Hiển thị bên phải header khi chưa đăng nhập |
| 5 | Hero banner | Ảnh kích thước lớn của Trung tâm, kèm slogan |
| 6 | Khu vực giới thiệu | 4 ô icon: Khám tổng quát, Sản – Nhi, Tiêm chủng, Cấp cứu |
| 7 | Danh sách bác sĩ tiêu biểu | Hiển thị 8 bác sĩ đang hoạt động, click vào xem chi tiết |
| 8 | Tin tức mới nhất | 6 bài viết mới nhất, sắp xếp theo `published_at` giảm dần |
| 9 | Bộ đếm khách trực tuyến | Hiển thị số phiên đang hoạt động trong 15 phút gần nhất |
| 10 | Footer | Địa chỉ 294 Trần Hưng Đạo, hotline 0220.3.822.205, email, bản đồ Google Maps |

## 3.2. Trang Đặt lịch khám

### 3.2.1. Bố cục

Giao diện đặt lịch được thiết kế dạng *step-by-step* gồm 4 bước: (1) Chọn chuyên khoa → (2) Chọn bác sĩ và ngày → (3) Chọn ca khám → (4) Nhập thông tin liên hệ và xác nhận.

### 3.2.2. Giao diện thực tế

![Hình 3.3. Giao diện Đặt lịch khám trên desktop](images/hinh-3-5.png){width=14cm}

![Hình 3.4. Giao diện Đặt lịch khám trên iPhone SE 320 × 568](images/hinh-3-6.png){width=7cm}

### 3.2.3. Đặc tả chức năng

**Bảng 3.2. Đặc tả chức năng Đặt lịch khám**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Dropdown chuyên khoa | Lấy từ bảng `department`, lọc các khoa đang hoạt động |
| 2 | Card bác sĩ | Hiển thị ảnh + tên + bằng cấp, lọc theo chuyên khoa đã chọn |
| 3 | Lịch tháng | Đánh dấu ngày có slot trống (xanh) / đã đầy (xám) / không trực (mờ) |
| 4 | Radio chọn ca | Sáng (07:00 – 11:30) / Chiều (13:30 – 17:00) |
| 5 | Form thông tin liên hệ | Họ tên, SĐT (10 chữ số), email, lý do khám |
| 6 | Token CSRF ẩn | Tự sinh bởi `@Html.AntiForgeryToken()` ở Razor |
| 7 | Nút *"Xác nhận đặt lịch"* | POST tới `/dat-lich-kham`, redirect tới trang xác nhận |
| 8 | Trang xác nhận | Hiển thị thông tin lịch + ghi chú *"Vui lòng đến trước 15 phút"* |
| 9 | Mã đặt tạm | UUID rút gọn, lưu trong Session với key `LastAnonBookingId` |
| 10 | Cảnh báo trùng lịch | Nếu cùng SĐT đã có lịch trong ngày, hiển thị thông báo confirm lại |

## 3.3. Trang Đăng ký và Đăng nhập

### 3.3.1. Giao diện Đăng ký

![Hình 3.5. Giao diện trang Đăng ký](images/hinh-3-7.png){width=14cm}

### 3.3.2. Giao diện Đăng nhập

![Hình 3.6. Giao diện trang Đăng nhập](images/hinh-3-8.png){width=14cm}

### 3.3.3. Đặc tả chức năng

**Bảng 3.3. Đặc tả chức năng Đăng ký / Đăng nhập**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Trường họ tên | NVARCHAR(150), bắt buộc, ≥ 2 ký tự |
| 2 | Trường SĐT | Bắt buộc, regex `^(0|\+84)[0-9]{9}$`, unique trong site |
| 3 | Trường email | Tùy chọn, regex chuẩn email, unique nếu nhập |
| 4 | Trường mật khẩu | ≥ 8 ký tự, có chữ và số, hiện/ẩn bằng icon mắt |
| 5 | Trường xác nhận MK | Phải khớp mật khẩu, kiểm tra ngay tại client |
| 6 | Băng tin Lợi ích | 5 dòng giới thiệu lợi ích khi có tài khoản |
| 7 | Cảnh báo bảo mật | Mật khẩu mã hóa PBKDF2, khóa 15 phút sau 5 lần sai |
| 8 | Nút Đăng nhập | POST tới `/dang-nhap`, kiểm tra trên server |
| 9 | Link Đăng ký | Dẫn tới `/dang-ky` nếu chưa có tài khoản |
| 10 | Captcha (tùy chọn) | Hiển thị sau 3 lần đăng nhập sai để chống brute force |

## 3.4. Trang Lịch của tôi (Member)

### 3.4.1. Giao diện

![Hình 3.7. Giao diện trang Lịch của tôi](images/hinh-3-9.png){width=14cm}

### 3.4.2. Đặc tả chức năng

**Bảng 3.4. Đặc tả chức năng Lịch của tôi**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Tab trạng thái | 5 tab: Tất cả / Chờ duyệt / Đã xác nhận / Đã khám / Đã hủy |
| 2 | Card lịch hẹn | Mã booking + ngày + ca + bác sĩ + chuyên khoa + trạng thái |
| 3 | Nút Hủy lịch | Chỉ hiển thị khi trạng thái = Pending hoặc Confirmed (chưa quá ngày khám) |
| 4 | Nút Xem hồ sơ khám | Chỉ hiển thị khi trạng thái = Done |
| 5 | Phân trang | Mỗi trang 10 lịch, sắp xếp mới nhất trước |
| 6 | Bộ lọc theo ngày | Date picker, mặc định 30 ngày gần nhất |

## 3.5. Cổng Lễ tân

### 3.5.1. Trang chủ Cổng Lễ tân

![Hình 3.8. Giao diện Trang chủ Cổng Lễ tân](images/hinh-3-11.png){width=14cm}

### 3.5.2. Trang Lịch hẹn (Chờ duyệt)

![Hình 3.9. Giao diện Lịch hẹn — Chờ duyệt](images/hinh-3-12.png){width=14cm}

### 3.5.3. Trang Tìm theo SĐT

![Hình 3.10. Giao diện Tìm lịch theo số điện thoại](images/hinh-3-13.png){width=14cm}

### 3.5.4. Trang Lịch theo ngày

![Hình 3.11. Giao diện Tra cứu lịch theo ngày](images/hinh-3-14.png){width=14cm}

### 3.5.5. Đặc tả chức năng

**Bảng 3.5. Đặc tả chức năng Cổng Lễ tân**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Banner thông báo | Hiển thị số lịch chờ duyệt mới (realtime) |
| 2 | Bảng lịch hẹn | 6 cột: Mã / Bệnh nhân / SĐT / Bác sĩ / Ngày khám / Trạng thái |
| 3 | Nút Xác nhận | POST tới `/le-tan/lich-hen/confirm/{id}`, sinh mã booking, ghi audit |
| 4 | Nút Từ chối | Mở modal yêu cầu lý do (≥ 5 ký tự), POST tới endpoint reject |
| 5 | Tìm theo SĐT | Input + Enter, hiển thị tất cả lịch của SĐT trong site |
| 6 | Tra cứu theo ngày | Date picker, giới hạn ± 30 ngày, hiển thị toàn bộ lịch trong ngày |
| 7 | Check-in | Quét mã booking hoặc nhập tay, chuyển trạng thái → CheckedIn |

## 3.6. Cổng Bác sĩ

### 3.6.1. Trang chủ Cổng Bác sĩ

![Hình 3.12. Giao diện trang chủ Cổng Bác sĩ](images/hinh-3-15.png){width=14cm}

### 3.6.2. Trang Bệnh nhân hôm nay

![Hình 3.13. Giao diện Danh sách bệnh nhân hôm nay](images/hinh-3-16.png){width=14cm}

### 3.6.3. Đặc tả chức năng

**Bảng 3.6. Đặc tả chức năng Cổng Bác sĩ**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Tổng quan trong ngày | 4 ô: Số bệnh nhân chờ / đang khám / đã khám / chuyển tuyến |
| 2 | Danh sách bệnh nhân | Sắp xếp theo thời gian check-in tăng dần, đánh dấu khẩn |
| 3 | Form chẩn đoán | 4 trường: Triệu chứng / Chẩn đoán / Đơn thuốc / Ghi chú |
| 4 | Auto-save | Lưu nháp mỗi 30 giây vào localStorage chống mất khi refresh |
| 5 | Nút Lưu hồ sơ | Sinh `record_no` tự động, ghi audit, chuyển trạng thái → Done |
| 6 | Lịch trực | Hiển thị lịch của bác sĩ trong tháng hiện tại |
| 7 | Cross-doctor guard | BS A không được mở chẩn đoán bệnh nhân của BS B (chặn từ controller) |

## 3.7. Cổng Quản trị (AdminCP)

### 3.7.1. Dashboard

![Hình 3.14. Giao diện Dashboard AdminCP](images/hinh-3-18.png){width=14cm}

### 3.7.2. Trang Quản lý Lịch trực bác sĩ

![Hình 3.15. Giao diện Quản lý Lịch trực bác sĩ](images/hinh-3-19.png){width=14cm}

### 3.7.3. Trang Tự động phân lịch tháng

![Hình 3.16. Giao diện Tự động phân lịch tháng](images/hinh-3-20.png){width=14cm}

### 3.7.4. Đặc tả chức năng

**Bảng 3.7. Đặc tả chức năng AdminCP**

| **STT** | **Thành phần** | **Mô tả** |
|:--:|:--|:--|
| 1 | Sidebar quản trị | 8 mục: Dashboard, Tài khoản, Bác sĩ, Khoa, Lịch trực, Tin tức, Hỏi đáp, Audit |
| 2 | Dashboard cards | Tổng số tài khoản / lịch trong tháng / hồ sơ đã lập / câu hỏi chờ duyệt |
| 3 | Quản lý tài khoản | CRUD tài khoản nhân viên, phân nhóm Admin / Reception / Doctor |
| 4 | Tự động phân lịch | Sinh 10 slot/bác sĩ/tháng (Mon-Fri × 2 ca), idempotent |
| 5 | Cron auto-gen | BackgroundService chạy mỗi giờ, kích hoạt vào ngày 28 hằng tháng |
| 6 | Bảng audit log | Filter theo action / userId / khoảng thời gian, export CSV |
| 7 | Cấu hình site | Đổi tên cơ sở, logo, địa chỉ, hotline, email — file logo upload trực tiếp |

\newpage
