# CHƯƠNG 4. LẬP KẾ HOẠCH KIỂM THỬ

Sau khi hoàn thành cài đặt hệ thống ở Chương 3, công đoạn quan trọng tiếp theo là thiết lập kế hoạch kiểm thử nhằm bảo đảm chất lượng phần mềm trước khi triển khai. Chương này trình bày toàn bộ kế hoạch kiểm thử cho website Trung tâm Y tế phường Kinh Môn — bao gồm mục tiêu, phạm vi, các mức kiểm thử, tiêu chí Pass/Fail, môi trường thực hiện, chiến lược và lịch trình thực thi.

## 4.1. Mục tiêu kiểm thử

Kế hoạch kiểm thử hướng tới năm mục tiêu chính, bám sát yêu cầu đề cương đồ án và đặc thù nghiệp vụ y tế:

1. **Đúng đặc tả nghiệp vụ:** các luồng nghiệp vụ y tế (đặt lịch khám, duyệt lịch, check-in, tạo hồ sơ khám, hỏi đáp Q&A) phải hoạt động đúng theo đặc tả use case ở Chương 2.
2. **Đảm bảo ràng buộc dữ liệu:** kiểm tra các quy tắc nghiệp vụ được enforce ở tầng Service như máy trạng thái lịch hẹn (state machine), giới hạn quota khoa và bác sĩ, độ dài chuỗi tối đa (length cap), bảo vệ chéo bác sĩ (cross-doctor guard).
3. **Bảo mật:** các kịch bản xác thực, phân quyền, chống CSRF, chống IDOR phải chặn được truy cập trái phép — đặc biệt quan trọng với dữ liệu y tế.
4. **Tương thích giao diện:** giao diện hiển thị đúng tiếng Việt (Unicode), bố cục responsive trên 4 viewport (desktop 1366/1920, mobile iPhone X/SE), không lỗi 4xx/5xx ở 14 endpoint công khai và 6 module AdminCP.
5. **Đồng bộ tài liệu:** các tài liệu thesis (ERD, Use Case, Activity Diagram, Test Cases) phải nhất quán với mã nguồn thực thi để bảo đảm tính đầy đủ của đồ án.

## 4.2. Phạm vi kiểm thử

### 4.2.1. Trong phạm vi (In-scope)

**Bảng 4.1. Các cụm chức năng nằm trong phạm vi kiểm thử**

| **Cụm chức năng** | **Chi tiết** |
|:--|:--|
| **Public site** | Trang chủ, danh mục bài viết, chi tiết bài viết, danh sách chuyên khoa, đăng ký, đăng nhập, Q&A công khai |
| **Patient flow (Bệnh nhân)** | Đăng ký tài khoản, đăng nhập, đặt lịch khám, lịch của tôi, hồ sơ cá nhân, lịch sử khám, Q&A của tôi |
| **Cổng Lễ tân** | Login, danh sách lịch theo trạng thái, duyệt/từ chối/đổi lịch, check-in bệnh nhân, quản lý quota khoa và bác sĩ |
| **Cổng Bác sĩ** | Login, danh sách bệnh nhân hôm nay, lịch trực bác sĩ (có banner xem tháng kế tiếp), tạo hồ sơ khám + kê đơn thuốc, duyệt và trả lời câu hỏi Q&A |
| **Cổng AdminCP** | Login, dashboard, CRUD Departments/News/Users/ClinicRooms, audit log có thể export CSV, quản lý lịch trực với month picker, MedicalRecords (chế độ chỉ đọc) |
| **Service layer** | AppointmentService, MedicalRecordService, UserService, QnaService, AuditService — kiểm tra logic nghiệp vụ thuần không phụ thuộc HTTP/DB thật |
| **Cross-cutting concerns** | Anti-CSRF token, cross-portal guard, role-based redirect, quản lý session, ghi audit log toàn bộ thay đổi state |

### 4.2.2. Ngoài phạm vi (Out-of-scope)

Một số mục được loại khỏi phạm vi đồ án do giới hạn thời gian hoặc không nằm trong đề cương:

- **Kiểm thử hiệu năng (performance/load test):** chỉ thực hiện smoke test với một request mỗi lần, không đo throughput hoặc số người dùng đồng thời tối đa. Đây là hướng mở rộng trong giai đoạn vận hành thực tế.
- **Kiểm thử thâm nhập (penetration test):** kiểm thử bảo mật chỉ dừng ở mức cơ bản (CSRF + xác thực + IDOR), không thực hiện SQL Injection nâng cao hay phân tích mã nguồn tự động.
- **Tích hợp dịch vụ ngoài:** chưa tích hợp thanh toán, SMS Brand Name, email gateway thật (chỉ mock).
- **Mobile native app:** chỉ kiểm thử ứng dụng web responsive, không xây dựng ứng dụng iOS/Android riêng.
- **Localization tiếng Anh:** UI culture cố định `vi-VN`, không kiểm thử bản tiếng Anh.

## 4.3. Mức độ kiểm thử

Kế hoạch kiểm thử tuân theo mô hình kim tự tháp kiểm thử (Testing Pyramid) với bốn mức rõ rệt từ chi tiết nhất (Unit) đến tổng quát nhất (Manual).

### 4.3.1. Mức Unit Test

- **Công cụ:** xUnit kết hợp với EF Core InMemory provider để tách biệt logic nghiệp vụ khỏi cơ sở dữ liệu thật.
- **Vị trí mã nguồn:** `WebsiteCore/tests/WebsiteCore.Tests/`.
- **Đối tượng kiểm thử:** tầng Service — toàn bộ logic nghiệp vụ thuần không phụ thuộc giao tiếp HTTP hay cơ sở dữ liệu vật lý.
- **Số lượng:** 84 test case.
- **Trọng tâm:**
  - `AppointmentService`: máy trạng thái 12 transition, phép tính tăng/giảm quota, định dạng mã booking.
  - `MedicalRecordService`: cấp số hồ sơ an toàn với race condition (retry tối đa 5 lần), giới hạn độ dài.
  - `UserService`: hash mật khẩu PBKDF2, fallback MD5 cho tài khoản cũ, validation đăng ký.
  - `QnaService`: luồng đặt câu hỏi và trả lời.
- **Lệnh thực thi:** `dotnet test WebsiteCore/tests/WebsiteCore.Tests/`.

### 4.3.2. Mức Functional / Integration Test (HTTP)

- **Công cụ:** Bash + `curl` với cookie jar, trích xuất anti-forgery token bằng biểu thức chính quy.
- **Vị trí mã nguồn:** `full_test.sh` ở thư mục gốc repository.
- **Đối tượng kiểm thử:** HTTP endpoint — kiểm tra status code, redirect, cookie, body của response.
- **Số lượng:** 94 test case.
- **Trọng tâm:**
  - 14 endpoint public trả về HTTP 200.
  - 6 module AdminCP trả HTTP 200 sau khi đăng nhập admin.
  - 3 luồng đăng nhập theo vai trò (member/letan/bacsy) + redirect đúng portal.
  - Anti-CSRF: form thiếu token bị từ chối với HTTP 400.
  - Cross-portal guard: tài khoản MEMBER đăng nhập tại `/AdminCP/Login` bị từ chối với thông báo "Tài khoản không tồn tại".
  - Workflow đầu cuối: đăng ký → đặt lịch → duyệt → check-in → tạo hồ sơ → kiểm tra audit.
- **Lệnh thực thi:** `bash full_test.sh` (yêu cầu server đang chạy ở cổng 5050).

### 4.3.3. Mức End-to-End Test (E2E UI)

- **Công cụ:** Playwright Test Runner kết hợp TypeScript — framework hiện đại của Microsoft, hỗ trợ đa trình duyệt và đa viewport.
- **Vị trí mã nguồn:** `tests/playwright/specs/`.
- **Đối tượng kiểm thử:** luồng UI người dùng thực — tương tác chuột/bàn phím, kiểm tra DOM, ảnh chụp toàn trang, đo overflow trên mobile.
- **Số lượng:** 279 test case (264 pass, 15 skip, 0 fail).
- **Trọng tâm:**
  - 78 UI smoke test trên 4 viewport (desktop 1366/1920, mobile iPhone X/SE).
  - 110 functional test cho các nghiệp vụ public, member, lễ tân, bác sĩ, admin.
  - 42 regression test chạy lại sau mỗi commit để phát hiện thay đổi không mong muốn.
  - 49 mobile audit test phát hiện overflow ngang, tap target nhỏ hơn 44 px, kích thước font không đạt chuẩn.
- **Lệnh thực thi:** `npx playwright test` ở thư mục `tests/playwright/`.

### 4.3.4. Mức Manual / Exploratory Test

- **Công cụ:** trực tiếp trên trình duyệt + Postman.
- **Đối tượng:** các luồng phức tạp khó tự động hoá đầy đủ (xác minh dữ liệu thực, kiểm thử deploy public, soát giao diện trên thiết bị thật).
- **Số lượng:** 12 kịch bản end-to-end + 8 request smoke Postman.
- **Trọng tâm:**
  - Triển khai public qua Cloudflare Tunnel, kiểm tra chứng chỉ TLS, đo điểm Lighthouse.
  - Soát ảnh chụp giao diện trên iPhone thật.
  - Test các luồng cần fixture nâng cao chưa tự động được (PDF export, gửi email SMTP thật).

## 4.4. Loại kiểm thử

Bộ kiểm thử kết hợp năm loại kiểm thử khác nhau, mỗi loại phục vụ một mục tiêu chất lượng riêng:

**Bảng 4.2. Các loại kiểm thử được áp dụng**

| **Loại** | **Mục tiêu** | **Công cụ** | **Số TC** |
|:--|:--|:--|:--:|
| Functional Testing | Xác minh nghiệp vụ đúng đặc tả | xUnit + Playwright | 188 |
| UI Testing | Kiểm tra bố cục, overflow, tap target | Playwright (visual) | 78 |
| Regression Testing | Phát hiện thay đổi không mong muốn sau commit | Playwright (re-run toàn bộ) | 42 |
| Security Testing | Phát hiện CSRF/IDOR/lockout/XSS | full_test.sh + Playwright | 16 |
| Mobile Responsive | Audit overflow, tap target, font size | Playwright (custom audit) | 49 |

## 4.5. Tiêu chí Pass / Fail (Test Specification)

### 4.5.1. Tiêu chí Pass cho một test case

Một test case được tính là **Pass** khi và chỉ khi đồng thời thoả mãn:

1. Mã trả về HTTP đúng đặc tả (thường là 200 với GET, 302 với redirect sau form submit, 400 với form lỗi).
2. Nội dung response chứa đúng từ khoá hoặc thoả biểu thức chính quy mong đợi (ví dụ: form đăng nhập có chứa input `UserName` và `Password`).
3. Trạng thái cơ sở dữ liệu sau khi chạy đúng kỳ vọng (đối với test case có ghi dữ liệu).
4. Tài nguyên giao diện (CSS, JS, ảnh logo) trả 200 hoặc 304, không có lỗi 4xx/5xx ở tab Network.
5. Tiếng Việt hiển thị đúng — không bị encode thành chuỗi `&#x...;` ở Razor.

### 4.5.2. Tiêu chí Fail

Test case bị tính **Fail** nếu:

- HTTP status không khớp đặc tả (ví dụ kỳ vọng 200 nhưng nhận 500).
- Trả về 5xx ở bất kỳ endpoint nào (server-side error).
- Response thiếu từ khoá bắt buộc.
- Hiển thị lỗi crash của framework (vd: stack trace của EF Core, NullReferenceException).
- Có lỗi mojibake — chữ Việt bị encode sai thành `&#x110;` hoặc question mark.

### 4.5.3. Tiêu chí ra (Exit Criteria) toàn bộ kế hoạch

Toàn bộ kế hoạch kiểm thử được coi là **đạt** khi đạt đồng thời các tiêu chí sau:

- 100% test case unit và functional Pass; không cho phép Fail.
- ≥ 95% test case E2E Pass; phần Skip có lý do rõ ràng (cần fixture nâng cao).
- Không tồn tại lỗi mức **Critical** hoặc **High** ở 14 endpoint public.
- Tài liệu thesis (ERD, Use Case, Test Cases) đồng bộ với mã nguồn — không có sự khác biệt về tên bảng, số bảng, mã use case.

### 4.5.4. Tiêu chí dừng (Suspension Criteria)

Kế hoạch kiểm thử sẽ tạm dừng khi phát hiện:

- Lỗi mức **Critical** chặn quy trình chính (ví dụ: không đăng nhập được, mass-assignment SiteId).
- Cơ sở dữ liệu bị hỏng cấu trúc (schema mismatch, EF migration lỗi).
- Server không khởi động được (port conflict, missing dependency).

Sau khi khắc phục, toàn bộ test suite phải được chạy lại từ đầu (re-run full regression).

## 4.6. Môi trường kiểm thử

### 4.6.1. Cấu hình môi trường

**Bảng 4.3. Cấu hình môi trường kiểm thử**

| **Hạng mục** | **Giá trị** |
|:--|:--|
| Hệ điều hành | Windows 11 Pro 24H2 |
| Runtime | .NET 8.0.10 SDK + ASP.NET Core Runtime |
| Cơ sở dữ liệu | SQL Server Express 2022, schema `ttytlp`, connection `.\SQLEXPRESS` |
| Trình duyệt | Chromium (Playwright), Firefox 130, Edge 130 |
| Mobile emulator | Playwright device profile — iPhone X (375×812), iPhone SE (320×568) |
| Port server | 5050 (`dotnet run --urls "http://localhost:5050"`) |
| Domain public | `https://ttytkm.jamesnguyen28.io.vn` (Cloudflare Tunnel) |
| Tài khoản test | `admin / 123456` (sau đó đổi qua `Tanh2004@`), `letan / Tanh2004@`, `bacsy / Tanh2004@`, `member01 / Member01@Test` |

### 4.6.2. Quy trình setup

Trình tự thiết lập môi trường trước mỗi vòng kiểm thử đầy đủ:

```bash
# 1. Khôi phục cơ sở dữ liệu seed
dotnet ef database update --project WebsiteCore/src/WebsiteCore.Web

# 2. Build solution
dotnet build WebsiteCore/src/WebsiteCore.Web/WebsiteCore.Web.csproj

# 3. Khởi động server
cd WebsiteCore/src/WebsiteCore.Web
dotnet run --urls "http://localhost:5050"

# 4. Chạy unit test ở terminal song song
dotnet test WebsiteCore/tests/WebsiteCore.Tests/

# 5. Chạy functional test
bash full_test.sh

# 6. Chạy E2E
cd tests/playwright && npx playwright test
```

### 4.6.3. Reset môi trường giữa các vòng

Sau mỗi vòng kiểm thử, môi trường được reset bằng cách:

- Xoá bảng `appointment`, `medical_record`, `qa_question`, `audit_system` (giữ nguyên seed bệnh nhân, bác sĩ, khoa).
- Reset session người dùng (xoá cookie ở Playwright `globalSetup`).
- Restart server để xoá cache in-memory (counter online, anti-CSRF token cache).

## 4.7. Chiến lược kiểm thử

### 4.7.1. Mô hình kim tự tháp (Testing Pyramid)

Phân bổ số lượng test case theo nguyên tắc kim tự tháp đảo ngược: nhiều unit test nhỏ, ít hơn các test E2E lớn và chậm.

**Bảng 4.4. Phân bổ test theo kim tự tháp**

| **Tầng** | **Số TC** | **Tỷ lệ** | **Thời gian chạy** |
|:--|:--:|:--:|:--:|
| Unit (xUnit) | 84 | 18% | ≈ 8 giây |
| Functional HTTP (bash + curl) | 94 | 20% | ≈ 45 giây |
| E2E UI (Playwright) | 279 | 62% | ≈ 12.4 phút |

Tổng thời gian một vòng kiểm thử đầy đủ ≈ 13 phút 30 giây.

### 4.7.2. Chiến lược ưu tiên

Phân loại mức độ ưu tiên test case theo độ quan trọng và rủi ro:

- **P0 (Critical):** đăng nhập, đặt lịch khám, duyệt lịch, tạo hồ sơ khám — phải Pass ở mọi commit.
- **P1 (High):** check-in, Q&A, lịch trực bác sĩ, audit log — phải Pass trước mỗi release.
- **P2 (Medium):** UI smoke, mobile responsive — chạy hằng đêm.
- **P3 (Low):** tài liệu helper, tooltip — chạy theo yêu cầu.

### 4.7.3. Kiểm thử theo rủi ro (Risk-based testing)

Tập trung nguồn lực vào các vùng rủi ro cao của hệ thống:

- **Bảo mật:** chống IDOR cross-doctor, cross-site, cross-portal — kiểm tra ở mọi commit.
- **Race condition:** cấp số hồ sơ `NextRecordNo` chạy song song 10 thread — đã cover ở unit test.
- **Tiếng Việt:** đảm bảo Razor không encode nhầm thành numeric entity — check ở mọi E2E.
- **Quota:** kiểm soát quota khoa và bác sĩ không bị overflow khi confirm đồng thời.

## 4.8. Lịch trình thực thi

**Bảng 4.5. Lịch trình thực thi kế hoạch kiểm thử (tháng 03/2026 — 05/2026)**

| **Giai đoạn** | **Thời gian** | **Nội dung chính** | **Sản phẩm** |
|:--|:--|:--|:--|
| Giai đoạn 1: Unit test | 01/03 — 15/03/2026 | Viết test cho 4 service chính, đạt 68 TC | Báo cáo coverage |
| Giai đoạn 2: Functional HTTP | 16/03 — 30/03/2026 | Viết `full_test.sh` với 96 TC bash | File log + báo cáo |
| Giai đoạn 3: E2E baseline | 01/04 — 20/04/2026 | Viết 20 spec Playwright phủ 4 cổng | HTML report + trace |
| Giai đoạn 4: Mobile audit + regression | 21/04 — 10/05/2026 | Bổ sung 49 mobile test + 42 regression | Screenshot 4 viewport |
| Giai đoạn 5: Soát lỗi + sửa | 11/05 — 25/05/2026 | Phát hiện và khắc phục 18 lỗi (2 critical, 9 high, 5 medium, 2 low) | Bug tracker |
| Giai đoạn 6: Hoàn tất + báo cáo | 26/05 — 31/05/2026 | Tổng hợp Bảng 4.14, viết Chương 4–5 báo cáo | Báo cáo Word v5 |

---

Kế hoạch kiểm thử trên định hình toàn bộ công tác kiểm thử cho website TTYT Kinh Môn. Chương 5 tiếp theo trình bày kết quả thực thi kế hoạch — bao gồm tổng hợp số lượng test case, đánh giá thời gian rút ngắn nhờ tự động hoá và phân loại các lỗi đã phát hiện.
