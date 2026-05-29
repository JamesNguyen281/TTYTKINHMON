# CHƯƠNG 4. LẬP KẾ HOẠCH KIỂM THỬ

Kế hoạch kiểm thử của website Trung tâm Y tế phường Kinh Môn hướng tới việc bảo đảm chất lượng phần mềm trước khi triển khai vận hành. Nội dung trình bày bao gồm mục tiêu, phạm vi, các mức kiểm thử, tiêu chí Pass/Fail, môi trường thực hiện, chiến lược và lịch trình thực thi.

## 4.1. Mục tiêu kiểm thử

Kế hoạch kiểm thử hướng tới năm mục tiêu chính, bám sát yêu cầu đề cương đồ án và đặc thù nghiệp vụ y tế.

Các luồng nghiệp vụ y tế gồm đặt lịch khám, duyệt lịch, check-in, tạo hồ sơ khám và hỏi đáp Q&A phải hoạt động đúng theo đặc tả use case đã trình bày ở Chương 2. Mỗi bước trong luồng được đối chiếu trực tiếp với mô tả nghiệp vụ tương ứng nhằm bảo đảm không phát sinh sai lệch giữa thiết kế và mã nguồn thực thi.

Các quy tắc nghiệp vụ được enforce ở tầng Service cần được kiểm tra đầy đủ, bao gồm máy trạng thái lịch hẹn, giới hạn quota khoa và bác sĩ, độ dài chuỗi tối đa và bảo vệ chéo giữa các bác sĩ. Việc kiểm thử các ràng buộc này giúp phát hiện sớm các lỗi nghiệp vụ tiềm ẩn trước khi đưa vào vận hành.

Yêu cầu bảo mật chiếm vị trí đặc biệt quan trọng do đặc thù dữ liệu y tế. Các kịch bản xác thực, phân quyền, chống CSRF và chống IDOR phải chặn được mọi truy cập trái phép, bảo vệ thông tin sức khỏe của người dân theo Luật Khám bệnh, chữa bệnh năm 2023.

Tính tương thích giao diện được kiểm tra trên bốn viewport gồm desktop 1366 và 1920 pixel cùng hai cấu hình mobile iPhone X và iPhone SE. Giao diện phải hiển thị đúng tiếng Việt theo chuẩn Unicode, bố cục responsive đầy đủ, đồng thời không phát sinh lỗi 4xx hoặc 5xx ở 14 endpoint công khai và 6 module AdminCP.

Tài liệu thesis bao gồm ERD, Use Case, Activity Diagram và Test Cases phải nhất quán với mã nguồn thực thi. Sự đồng bộ giữa tài liệu và mã nguồn bảo đảm tính đầy đủ và tin cậy của đồ án khi bảo vệ trước hội đồng.

## 4.2. Phạm vi kiểm thử

### 4.2.1. Trong phạm vi (In-scope)

Bảng 4.1. Các cụm chức năng nằm trong phạm vi kiểm thử

| Cụm chức năng | Chi tiết |
|:--|:--|
| Public site | Trang chủ, danh mục bài viết, chi tiết bài viết, danh sách chuyên khoa, đăng ký, đăng nhập, Q&A công khai |
| Patient flow (Bệnh nhân) | Đăng ký tài khoản, đăng nhập, đặt lịch khám, lịch của tôi, hồ sơ cá nhân, lịch sử khám, Q&A của tôi |
| Cổng Lễ tân | Login, danh sách lịch theo trạng thái, duyệt/từ chối/đổi lịch, check-in bệnh nhân, quản lý quota khoa và bác sĩ |
| Cổng Bác sĩ | Login, danh sách bệnh nhân hôm nay, lịch trực bác sĩ (có banner xem tháng kế tiếp), tạo hồ sơ khám + kê đơn thuốc, duyệt và trả lời câu hỏi Q&A |
| Cổng AdminCP | Login, dashboard, CRUD Departments/News/Users/ClinicRooms, audit log có thể export CSV, quản lý lịch trực với month picker, MedicalRecords (chế độ chỉ đọc) |
| Service layer | AppointmentService, MedicalRecordService, UserService, QnaService, AuditService — kiểm tra logic nghiệp vụ thuần không phụ thuộc HTTP/DB thật |
| Cross-cutting concerns | Anti-CSRF token, cross-portal guard, role-based redirect, quản lý session, ghi audit log toàn bộ thay đổi state |

### 4.2.2. Ngoài phạm vi (Out-of-scope)

Một số hạng mục được loại khỏi phạm vi đồ án do giới hạn thời gian hoặc không nằm trong đề cương. Kiểm thử hiệu năng chỉ dừng ở mức smoke test với một request mỗi lần, không đo throughput hay số người dùng đồng thời tối đa; đây là hướng mở rộng dành cho giai đoạn vận hành thực tế. Kiểm thử thâm nhập chỉ dừng ở mức cơ bản gồm CSRF, xác thực và IDOR, không thực hiện SQL Injection nâng cao hay phân tích mã nguồn tự động. Tích hợp dịch vụ ngoài như thanh toán, SMS Brand Name và email gateway thật chưa được triển khai mà chỉ dùng mock. Ứng dụng web responsive được lựa chọn thay cho mobile native app, do đó không có bản iOS hay Android riêng. UI culture cố định `vi-VN` nên bản tiếng Anh không nằm trong phạm vi kiểm thử.

## 4.3. Mức độ kiểm thử

Kế hoạch kiểm thử tuân theo mô hình kim tự tháp kiểm thử (Testing Pyramid) với bốn mức rõ rệt từ chi tiết nhất là Unit đến tổng quát nhất là Manual.

### 4.3.1. Mức Unit Test

Mức Unit Test được triển khai bằng xUnit kết hợp EF Core InMemory provider nhằm tách biệt logic nghiệp vụ khỏi cơ sở dữ liệu thật. Mã nguồn test đặt tại `WebsiteCore/tests/WebsiteCore.Tests/`, tập trung vào tầng Service với toàn bộ logic nghiệp vụ thuần không phụ thuộc giao tiếp HTTP hay cơ sở dữ liệu vật lý. Tổng số test case ở mức này là 84, phủ bốn service chính. `AppointmentService` được kiểm tra với máy trạng thái 12 transition, phép tính tăng giảm quota và định dạng mã booking; `MedicalRecordService` được kiểm tra với cơ chế cấp số hồ sơ an toàn khi có race condition (retry tối đa 5 lần) cùng các giới hạn độ dài; `UserService` được kiểm tra với hash mật khẩu PBKDF2, fallback MD5 cho tài khoản cũ và validation đăng ký; `QnaService` được kiểm tra với luồng đặt câu hỏi và trả lời. Lệnh thực thi toàn bộ unit test là `dotnet test WebsiteCore/tests/WebsiteCore.Tests/`.

### 4.3.2. Mức Functional / Integration Test (HTTP)

Mức Functional Test sử dụng Bash kết hợp `curl` với cookie jar và trích xuất anti-forgery token bằng biểu thức chính quy. Script kiểm thử đặt tại tệp `full_test.sh` ở thư mục gốc repository, đối tượng kiểm tra là các HTTP endpoint thông qua status code, redirect, cookie và body của response. Tổng số test case ở mức này là 94, bao quát các nhóm kịch bản chính. 14 endpoint public được kiểm tra trả về HTTP 200; 6 module AdminCP trả HTTP 200 sau khi đăng nhập admin; ba luồng đăng nhập theo vai trò member, lễ tân và bác sĩ được kiểm tra cùng cơ chế redirect đúng portal. Cơ chế anti-CSRF được kiểm tra qua tình huống form thiếu token bị từ chối với HTTP 400; cross-portal guard được kiểm tra qua tình huống tài khoản MEMBER đăng nhập tại `/AdminCP/Login` bị từ chối với thông báo "Tài khoản không tồn tại". Workflow đầu cuối từ đăng ký, đặt lịch, duyệt, check-in, tạo hồ sơ tới kiểm tra audit cũng được phủ trong cùng script. Lệnh thực thi là `bash full_test.sh` với điều kiện server đang chạy ở cổng 5050.

### 4.3.3. Mức End-to-End Test (E2E UI)

Mức E2E sử dụng Playwright Test Runner kết hợp TypeScript, framework do Microsoft phát triển với khả năng hỗ trợ đa trình duyệt và đa viewport. Mã nguồn test đặt tại `tests/playwright/specs/`, đối tượng kiểm tra là luồng UI của người dùng thực thông qua tương tác chuột, bàn phím, kiểm tra DOM, ảnh chụp toàn trang và đo overflow trên mobile. Tổng số test case ở mức này là 279, trong đó 264 pass, 15 skip và không có test fail. Phân bổ test gồm 78 UI smoke test trên bốn viewport (desktop 1366/1920, mobile iPhone X/SE), 110 functional test cho các nghiệp vụ public, member, lễ tân, bác sĩ và admin, 42 regression test chạy lại sau mỗi commit để phát hiện thay đổi không mong muốn, cùng 49 mobile audit test phát hiện overflow ngang, tap target nhỏ hơn 44 px và kích thước font không đạt chuẩn. Lệnh thực thi là `npx playwright test` ở thư mục `tests/playwright/`.

### 4.3.4. Mức Manual / Exploratory Test

Mức Manual Test được thực hiện trực tiếp trên trình duyệt kết hợp Postman, dành cho các luồng phức tạp khó tự động hoá đầy đủ. Đối tượng kiểm thử gồm xác minh dữ liệu thực, kiểm thử deploy public và soát giao diện trên thiết bị thật. Tổng số kịch bản gồm 12 kịch bản end-to-end và 8 request smoke Postman. Trọng tâm kiểm thử bao gồm triển khai public qua Cloudflare Tunnel kèm kiểm tra chứng chỉ TLS và đo điểm Lighthouse, soát ảnh chụp giao diện trên iPhone thật, cùng các luồng cần fixture nâng cao chưa tự động được như PDF export hoặc gửi email SMTP thật.

## 4.4. Loại kiểm thử

Bộ kiểm thử kết hợp năm loại kiểm thử khác nhau, mỗi loại phục vụ một mục tiêu chất lượng riêng.

Bảng 4.2. Các loại kiểm thử được áp dụng

| Loại | Mục tiêu | Công cụ | Số TC |
|:--|:--|:--|:--:|
| Functional Testing | Xác minh nghiệp vụ đúng đặc tả | xUnit + Playwright | 188 |
| UI Testing | Kiểm tra bố cục, overflow, tap target | Playwright (visual) | 78 |
| Regression Testing | Phát hiện thay đổi không mong muốn sau commit | Playwright (re-run toàn bộ) | 42 |
| Security Testing | Phát hiện CSRF/IDOR/lockout/XSS | full_test.sh + Playwright | 16 |
| Mobile Responsive | Audit overflow, tap target, font size | Playwright (custom audit) | 49 |

## 4.5. Tiêu chí Pass / Fail (Test Specification)

### 4.5.1. Tiêu chí Pass cho một test case

Một test case được tính là Pass khi đồng thời thoả mãn các điều kiện sau. Mã trả về HTTP phải đúng đặc tả, thường là 200 với GET, 302 với redirect sau form submit và 400 với form lỗi. Nội dung response phải chứa đúng từ khoá hoặc thoả biểu thức chính quy mong đợi, chẳng hạn form đăng nhập phải chứa input `UserName` và `Password`. Trạng thái cơ sở dữ liệu sau khi chạy phải đúng kỳ vọng đối với các test case có ghi dữ liệu. Tài nguyên giao diện như CSS, JS và ảnh logo phải trả 200 hoặc 304, không phát sinh lỗi 4xx hoặc 5xx ở tab Network. Tiếng Việt phải hiển thị đúng, không bị Razor encode thành chuỗi `&#x...;`.

### 4.5.2. Tiêu chí Fail

Test case bị tính Fail khi xuất hiện một trong các tình huống sau. HTTP status không khớp đặc tả, chẳng hạn kỳ vọng 200 nhưng nhận 500. Bất kỳ endpoint nào trả về 5xx đều bị tính lỗi server-side. Response thiếu từ khoá bắt buộc cũng bị tính Fail. Lỗi crash của framework như stack trace của EF Core hay NullReferenceException xuất hiện trong response cũng nằm trong nhóm này. Trường hợp mojibake với chữ Việt bị encode sai thành `&#x110;` hoặc question mark cũng bị tính Fail.

### 4.5.3. Tiêu chí ra (Exit Criteria) toàn bộ kế hoạch

Toàn bộ kế hoạch kiểm thử được coi là đạt khi đồng thời thoả mãn các điều kiện sau. 100% test case unit và functional phải Pass, không cho phép Fail. Tỷ lệ Pass của test case E2E phải đạt từ 95% trở lên, các test case Skip phải có lý do rõ ràng như cần fixture nâng cao. Không tồn tại lỗi mức Critical hoặc High ở 14 endpoint public. Tài liệu thesis gồm ERD, Use Case và Test Cases phải đồng bộ với mã nguồn, không có sự khác biệt về tên bảng, số bảng và mã use case.

### 4.5.4. Tiêu chí dừng (Suspension Criteria)

Kế hoạch kiểm thử sẽ tạm dừng khi phát hiện lỗi mức Critical chặn quy trình chính, chẳng hạn không đăng nhập được hoặc xảy ra mass-assignment SiteId. Trường hợp cơ sở dữ liệu bị hỏng cấu trúc do schema mismatch hoặc EF migration lỗi cũng buộc dừng kế hoạch. Tình huống server không khởi động được do port conflict hoặc thiếu dependency cũng nằm trong nhóm tiêu chí dừng. Sau khi khắc phục, toàn bộ test suite phải được chạy lại từ đầu để bảo đảm không phát sinh hồi quy.

## 4.6. Môi trường kiểm thử

### 4.6.1. Cấu hình môi trường

Bảng 4.3. Cấu hình môi trường kiểm thử

| Hạng mục | Giá trị |
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

Trình tự thiết lập môi trường trước mỗi vòng kiểm thử đầy đủ được mô tả qua các lệnh sau:

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

Sau mỗi vòng kiểm thử, môi trường được reset theo ba bước. Các bảng `appointment`, `medical_record`, `qa_question` và `audit_system` được xoá nhưng vẫn giữ nguyên dữ liệu seed bệnh nhân, bác sĩ và khoa. Session người dùng được reset thông qua việc xoá cookie ở Playwright `globalSetup`. Server được khởi động lại để xoá cache in-memory gồm counter online và anti-CSRF token cache.

## 4.7. Chiến lược kiểm thử

### 4.7.1. Mô hình kim tự tháp (Testing Pyramid)

Số lượng test case được phân bổ theo nguyên tắc kim tự tháp với nhiều unit test nhỏ ở tầng đáy và ít test E2E lớn, chậm ở tầng đỉnh. Cách phân bổ này giúp giảm chi phí bảo trì và rút ngắn thời gian phản hồi của vòng kiểm thử.

Bảng 4.4. Phân bổ test theo kim tự tháp

| Tầng | Số TC | Tỷ lệ | Thời gian chạy |
|:--|:--:|:--:|:--:|
| Unit (xUnit) | 84 | 18% | ≈ 8 giây |
| Functional HTTP (bash + curl) | 94 | 20% | ≈ 45 giây |
| E2E UI (Playwright) | 279 | 62% | ≈ 12.4 phút |

Tổng thời gian một vòng kiểm thử đầy đủ vào khoảng 13 phút 30 giây.

### 4.7.2. Chiến lược ưu tiên

Test case được phân loại theo độ quan trọng và rủi ro thành bốn mức ưu tiên.

- P0 (Critical): đăng nhập, đặt lịch khám, duyệt lịch, tạo hồ sơ khám — phải Pass ở mọi commit.
- P1 (High): check-in, Q&A, lịch trực bác sĩ, audit log — phải Pass trước mỗi release.
- P2 (Medium): UI smoke, mobile responsive — chạy hằng đêm.
- P3 (Low): tài liệu helper, tooltip — chạy theo yêu cầu.

### 4.7.3. Kiểm thử theo rủi ro (Risk-based testing)

Nguồn lực kiểm thử được tập trung vào các vùng rủi ro cao của hệ thống. Vùng bảo mật gồm chống IDOR cross-doctor, cross-site và cross-portal được kiểm tra ở mọi commit. Vùng race condition khi cấp số hồ sơ `NextRecordNo` chạy song song 10 thread đã được cover ở unit test. Vùng tiếng Việt với yêu cầu Razor không encode nhầm thành numeric entity được kiểm tra ở mọi E2E. Vùng quota của khoa và bác sĩ được kiểm soát để không bị overflow khi confirm đồng thời.

## 4.8. Lịch trình thực thi

Bảng 4.5. Lịch trình thực thi kế hoạch kiểm thử (tháng 03/2026 — 05/2026)

| Giai đoạn | Thời gian | Nội dung chính | Sản phẩm |
|:--|:--|:--|:--|
| Giai đoạn 1: Unit test | 01/03 — 15/03/2026 | Viết test cho 4 service chính, đạt 68 TC | Báo cáo coverage |
| Giai đoạn 2: Functional HTTP | 16/03 — 30/03/2026 | Viết `full_test.sh` với 96 TC bash | File log + báo cáo |
| Giai đoạn 3: E2E baseline | 01/04 — 20/04/2026 | Viết 20 spec Playwright phủ 4 cổng | HTML report + trace |
| Giai đoạn 4: Mobile audit + regression | 21/04 — 10/05/2026 | Bổ sung 49 mobile test + 42 regression | Screenshot 4 viewport |
| Giai đoạn 5: Soát lỗi + sửa | 11/05 — 25/05/2026 | Phát hiện và khắc phục 18 lỗi (2 critical, 9 high, 5 medium, 2 low) | Bug tracker |
| Giai đoạn 6: Hoàn tất + báo cáo | 26/05 — 31/05/2026 | Tổng hợp Bảng 4.14, viết Chương 4–5 báo cáo | Báo cáo Word v5 |

---

Kế hoạch kiểm thử trên định hình toàn bộ công tác kiểm thử cho website TTYT Kinh Môn. Chương 5 tiếp theo trình bày kết quả thực thi kế hoạch, bao gồm tổng hợp số lượng test case, đánh giá thời gian rút ngắn nhờ tự động hoá và phân loại các lỗi đã phát hiện.
