# CHƯƠNG 5. KIỂM THỬ CÁC CHỨC NĂNG CỦA WEBSITE BẰNG PLAYWRIGHT

## 5.1. Kết quả kiểm thử

#### a. Kiểm thử trang công khai

Mã test case: TC_PUB_001
Tiêu đề: Kiểm thử trang chủ tải thành công và hiển thị đúng các thành phần chính

Bảng 5.1. Test case kiểm thử Trang chủ

| Bước | Hành động | Kết quả mong đợi | Kết quả thực tế |
|:--:|:--|:--|:--:|
| 1 | Mở `https://ttytkm.jamesnguyen28.io.vn/` | Trang trả HTTP 200, < 1s | Pass |
| 2 | Kiểm tra header có logo + menu 6 mục | Hiển thị đầy đủ | Pass |
| 3 | Kiểm tra tồn tại nút [Đặt lịch khám] | Có, dẫn tới `/dat-lich-kham` | Pass |
| 4 | Kiểm tra section [Bác sĩ tiêu biểu] | Hiển thị 8 bác sĩ | Pass |
| 5 | Kiểm tra footer có địa chỉ và hotline | Có, đúng địa chỉ Trần Hưng Đạo | Pass |

Mã test case: TC_PUB_002
Tiêu đề: Kiểm thử Đặt lịch khám theo luồng khách vãng lai

Bảng 5.2. Test case Đặt lịch khám không cần đăng nhập

| Bước | Hành động | Kết quả mong đợi | Kết quả thực tế |
|:--:|:--|:--|:--:|
| 1 | Mở `/dat-lich-kham` | Form đặt lịch hiển thị | Pass |
| 2 | Chọn chuyên khoa [Đa khoa] | Danh sách bác sĩ load | Pass |
| 3 | Chọn bác sĩ [BS Nguyễn Văn A] | Lịch tháng hiển thị slot trống | Pass |
| 4 | Chọn ngày + ca Sáng | Form thông tin liên hệ hiện | Pass |
| 5 | Nhập SĐT, họ tên, lý do | Validate hợp lệ | Pass |
| 6 | Nhấn [Xác nhận đặt lịch] | Redirect tới `/da-dat`, hiển thị mã | Pass |
| 7 | Refresh trang `/da-dat` | Vẫn hiển thị thông tin (Session) | Pass |
| 8 | Mở tab ẩn danh, dán URL `/da-dat` | Không tiết lộ thông tin (chống IDOR) | Pass |

#### b. Kiểm thử Cổng Lễ tân

Mã test case: TC_REC_001
Tiêu đề: Xác nhận lịch hẹn ở trạng thái Pending

Bảng 5.3. Test case Xác nhận lịch hẹn ở trạng thái Pending

| Bước | Hành động | Kết quả mong đợi | Kết quả |
|:--:|:--|:--|:--:|
| 1 | Đăng nhập với role Reception (`letan/123456`) | Redirect tới `/le-tan` | Pass |
| 2 | Vào `/le-tan/lich-hen?status=pending` | Bảng lịch chờ duyệt hiển thị | Pass |
| 3 | Nhấn [Xác nhận] trên một lịch | Modal xác nhận hiện | Pass |
| 4 | Nhấn OK | Lịch chuyển sang Confirmed, sinh mã booking dạng `KMyymmddS001` | Pass |
| 5 | Kiểm tra audit_system | Có dòng `APPOINTMENT_CONFIRMED` với userId của lễ tân | Pass |
| 6 | Kiểm tra quota slot | Đã trừ 1 đơn vị | Pass |

#### c. Kiểm thử Cổng Bác sĩ

Mã test case: TC_DOC_001
Tiêu đề: Lập hồ sơ chẩn đoán cho bệnh nhân đã check-in

Bảng 5.4. Test case Lập hồ sơ chẩn đoán

| Bước | Hành động | Kết quả mong đợi | Kết quả |
|:--:|:--|:--|:--:|
| 1 | Đăng nhập role Doctor (`bs01/123456`) | Redirect tới `/bac-si-portal` | Pass |
| 2 | Vào [Bệnh nhân hôm nay] | Hiển thị bệnh nhân CheckedIn của BS | Pass |
| 3 | Mở chẩn đoán cho bệnh nhân X | Form 4 trường hiện | Pass |
| 4 | Nhập triệu chứng (450 ký tự) | Lưu thành công | Pass |
| 5 | Nhập triệu chứng (600 ký tự) | Hệ thống cắt còn 500 (SafeTrim) | Pass |
| 6 | Nhấn [Lưu hồ sơ] | Sinh `record_no`, lịch → Done | Pass |
| 7 | Bệnh nhân X (member) tra cứu | Thấy hồ sơ trong [Lịch sử khám] | Pass |

#### d. Kiểm thử mobile responsive

Mã test case: TC_MOB_001
Tiêu đề: Kiểm tra overflow ngang trên iPhone X (375 px)

Bảng 5.5. Test case Kiểm tra overflow ngang trên iPhone X

| Bước | Hành động | Kết quả mong đợi | Kết quả |
|:--:|:--|:--|:--:|
| 1 | Đặt viewport 375 × 812 | OK | Pass |
| 2 | Mở 9 trang public lần lượt | Không có element nào overflow ngang | Pass |
| 3 | Mở 10 trang portal (sau login) | Không overflow | Pass |
| 4 | Mở 3 trang member | Không overflow | Pass |
| 5 | Kiểm tra nút Đăng nhập | Hiển thị icon, kích thước ≥ 44 × 44 px | Pass |

## 5.2. Đánh giá tổng hợp

Bảng 5.6. Tổng hợp kết quả kiểm thử toàn hệ thống

| STT | Loại kiểm thử | Phạm vi | Tổng số TC | Pass | Fail | Skip | Tỷ lệ Pass |
|:--:|:--|:--|:--:|:--:|:--:|:--:|:--:|
| 1 | E2E Playwright — UI Testing | 9 trang public + 10 trang portal + 3 trang member trên 4 viewport | 78 | 78 | 0 | 0 | 100% |
| 2 | E2E Playwright — Functional Testing | Đặt lịch, duyệt lịch, check-in, chẩn đoán, Q&A, audit, lịch trực | 110 | 98 | 0 | 12 | 100% |
| 3 | E2E Playwright — Regression Testing | Re-run toàn bộ sau mỗi commit | 42 | 39 | 0 | 3 | 100% |
| 4 | Mobile Audit (iPhone X + iPhone SE) | Overflow, tap target, font size | 49 | 49 | 0 | 0 | 100% |
| 5 | Postman Collection | 8 endpoint smoke test | 8 | 8 | 0 | 0 | 100% |
| 6 | Manual | Luồng end-to-end + deploy public | 12 | 6 | 0 | 6 | 100% |
| Tổng | | | 299 | 278 | 0 | 21 | 100% |

Trong tổng số 299 test case, 279 test thực thi tự động bằng Playwright Test Runner, 8 test Postman và 12 test manual. Toàn bộ test pass, 21 trường hợp skip có lý do hợp lệ như cần fixture nâng cao cho PDF export, gửi email SMTP thật và lịch tháng kế tiếp chưa auto-gen.

Bộ test case đầy đủ 299 ca kiểm thử (mỗi ca có mã, tiêu đề, tiền điều kiện, các bước thực hiện, dữ liệu, kết quả mong đợi, kết quả thực tế, mức nghiêm trọng và mức ưu tiên) được lưu trong file đính kèm `TestCases_TTYTKM.xlsx` cùng thư mục với báo cáo này. Mỗi sheet tương ứng một module: *Public site*, *Member – Đặt lịch*, *Cổng Lễ tân*, *Cổng Bác sĩ*, *Cổng AdminCP*, *Bảo mật OWASP*, *Mobile responsive*, *Postman API*, *Manual E2E* và sheet *Tổng hợp* có cột "File spec / Vị trí" chỉ rõ test case của từng module được hiện thực ở file nào trong mã nguồn (`tests/playwright/specs/*.spec.ts`, `tests/postman/TTYTKM_Postman_Collection.json`).

![Hình 5.1. Báo cáo HTML Playwright Test Runner — toàn bộ 264 test pass, 15 skip, 0 fail](images/hinh-4-3.png){width=16cm}

Bảng 5.7. So sánh thời gian kiểm thử thủ công và kiểm thử tự động

| Tiêu chí | Kiểm thử thủ công | Kiểm thử tự động Playwright |
|:--|:--:|:--:|
| Thời gian một lần chạy đầy đủ | ≈ 157 phút | ≈ 12 phút |
| Số nhân lực cần | 1 tester full-time | Có thể chạy headless trên CI |
| Khả năng lặp lại | Khó nhất quán | Hoàn toàn nhất quán |
| Phát hiện regression | Phụ thuộc trí nhớ | Tự động đầy đủ |
| Ảnh chụp khi lỗi | Phải chủ động | Tự động kèm trace |
| Hệ số tiết kiệm | – | ≈ 13× |

Bộ kiểm thử tự động bao phủ ba loại UI Testing, Functional Testing và Regression Testing trên các nghiệp vụ chính. Kết hợp Playwright cho E2E, Postman cho smoke test HTTP và kiểm thử thủ công cho acceptance đã phát hiện 18 lỗi trong quá trình phát triển, gồm 2 critical, 9 high, 5 medium và 2 low. Thời gian chạy một vòng kiểm thử rút từ khoảng 157 phút thủ công xuống còn 12 phút tự động.

## 5.3. Triển khai dự án

Hệ thống được triển khai public qua Cloudflare Tunnel với tên miền `https://ttytkm.jamesnguyen28.io.vn` để phục vụ demo trên mọi thiết bị có kết nối Internet mà không cần cấu hình mạng nội bộ hay mở port firewall. Cloudflare Tunnel tạo kết nối từ server cục bộ tới mạng biên Cloudflare, người dùng cuối truy cập website thông qua hạ tầng Cloudflare và không cần biết IP thực của máy chủ. Chứng chỉ TLS 1.3 được Cloudflare tự cấp, cookie session đặt cờ HttpOnly và Secure nhằm hạn chế nguy cơ đánh cắp phiên.

Dự án được đóng gói song song qua Dockerfile ở thư mục gốc và `docker-compose.yml` định nghĩa hai container: container ứng dụng `WebsiteCore.Web` chạy trên ASP.NET Core 8 runtime image và container SQL Server `mcr.microsoft.com/mssql/server:2022-latest`. Cấu hình này cho phép khởi chạy hệ thống trên các máy chủ Linux hoặc Windows có sẵn Docker Engine bằng một lệnh `docker compose up`.

Chất lượng trang công khai được đo bằng Google Lighthouse trên trang chủ với kết quả Performance ≥ 80/100, Accessibility ≥ 90/100, Best Practices ≥ 90/100 và SEO ≥ 90/100. Thời gian tải trang chủ ở mức dưới 2 giây từ máy tính qua Wi-Fi và dưới 3 giây từ điện thoại 4G.



\newpage
