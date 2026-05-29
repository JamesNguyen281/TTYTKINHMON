# CHƯƠNG 4. LẬP KẾ HOẠCH KIỂM THỬ

## 4.1. Mục tiêu và phạm vi kiểm thử

Kế hoạch kiểm thử hướng tới năm mục tiêu. Mục tiêu thứ nhất là bảo đảm các luồng nghiệp vụ y tế gồm đặt lịch khám, duyệt lịch, check-in, tạo hồ sơ khám và hỏi đáp Q&A hoạt động đúng đặc tả use case ở Chương 2. Mục tiêu thứ hai là xác minh các quy tắc nghiệp vụ enforce ở tầng Service, gồm máy trạng thái lịch hẹn, giới hạn quota khoa và bác sĩ, độ dài chuỗi tối đa và bảo vệ chéo giữa các bác sĩ. Mục tiêu thứ ba liên quan tới bảo mật do đặc thù dữ liệu y tế: các kịch bản xác thực, phân quyền, chống CSRF và chống IDOR phải chặn được mọi truy cập trái phép.

Mục tiêu thứ tư là tính tương thích giao diện trên bốn viewport gồm desktop 1366 và 1920 pixel cùng hai cấu hình mobile iPhone X và iPhone SE, với yêu cầu hiển thị đúng tiếng Việt theo chuẩn Unicode và bố cục responsive đầy đủ. Mục tiêu cuối cùng là sự nhất quán giữa các tài liệu thesis (ERD, Use Case, Activity Diagram, Test Cases) và mã nguồn thực thi.

Phạm vi kiểm thử được phân chia rõ giữa phần trong phạm vi và phần ngoài phạm vi như Bảng 4.1.

Bảng 4.1. Phạm vi kiểm thử

| Phạm vi | Nội dung |
|:--|:--|
| Trong phạm vi | Public site (14 endpoint công khai), Patient flow, Cổng Lễ tân, Cổng Bác sĩ, AdminCP, Service layer, cross-cutting concerns (CSRF, IDOR guard, role-based redirect, session, audit log) |
| Ngoài phạm vi | Performance test thực tế với JMeter/k6, penetration test nâng cao, tích hợp SMS/Email gateway thật, mobile native app, đa ngôn ngữ |

## 4.2. Chiến lược và các mức kiểm thử

Kế hoạch kiểm thử tổ chức theo mô hình kim tự tháp (Testing Pyramid) với bốn mức, từ Unit ở đáy lên Manual ở đỉnh. Phân bổ kiểu kim tự tháp dồn phần lớn test case vào tầng đáy, qua đó hạn chế thời gian chạy của vòng kiểm thử khi lượng test tăng lên.

Mức Unit Test được triển khai bằng xUnit kết hợp EF Core InMemory provider nhằm tách logic nghiệp vụ khỏi cơ sở dữ liệu thật. Mã nguồn test đặt tại `WebsiteCore/tests/WebsiteCore.Tests/`, tập trung vào tầng Service. Mức này gồm 84 test case phủ bốn service chính `AppointmentService`, `MedicalRecordService`, `UserService` và `QnaService`, chạy bằng lệnh `dotnet test WebsiteCore/tests/WebsiteCore.Tests/`.

Mức Functional / Integration Test viết bằng Bash kết hợp `curl` với cookie jar và trích xuất anti-forgery token qua biểu thức chính quy. Script đặt tại `full_test.sh` ở thư mục gốc repository, kiểm tra các HTTP endpoint thông qua status code, redirect, cookie và body của response. Mức này gồm 94 test case, bao quát 14 endpoint public, 6 module AdminCP, ba luồng đăng nhập theo vai trò, cơ chế anti-CSRF và cross-portal guard.

Mức End-to-End Test dùng Playwright Test Runner với TypeScript, hỗ trợ đa trình duyệt và đa viewport. Mã nguồn đặt tại `tests/playwright/specs/`, kiểm thử luồng UI của người dùng thực thông qua tương tác chuột, bàn phím, kiểm tra DOM, ảnh chụp toàn trang và đo overflow trên mobile. Mức này gồm 279 test case, chia thành 78 UI smoke test, 110 functional test, 42 regression test và 49 mobile audit test.

Mức Manual / Exploratory Test thực hiện trực tiếp trên trình duyệt kết hợp Postman, dành cho các luồng khó tự động hoá. Mức này gồm 12 kịch bản end-to-end và 8 request smoke Postman, tập trung vào việc kiểm tra deploy public qua Cloudflare Tunnel, soát giao diện trên thiết bị thật và các luồng cần fixture nâng cao như PDF export hay gửi email SMTP thật.

Bảng 4.2. Phân bổ test case theo kim tự tháp

| Tầng | Số TC | Tỷ lệ | Thời gian chạy |
|:--|:--:|:--:|:--:|
| Unit (xUnit) | 84 | 18% | ≈ 8 giây |
| Functional HTTP (bash + curl) | 94 | 20% | ≈ 45 giây |
| E2E UI (Playwright) | 279 | 62% | ≈ 12.4 phút |

Tổng thời gian một vòng kiểm thử đầy đủ vào khoảng 13 phút 30 giây.

## 4.3. Môi trường và công cụ kiểm thử

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
| Tài khoản test | `admin / 123456`, `letan / Tanh2004@`, `bacsy / Tanh2004@`, `member01 / Member01@Test` |

Trình tự chạy đầy đủ một vòng kiểm thử được thực hiện qua bốn lệnh chính: khởi động server bằng `dotnet run --urls "http://localhost:5050"`, chạy unit test bằng `dotnet test WebsiteCore/tests/WebsiteCore.Tests/`, chạy functional test bằng `bash full_test.sh` và chạy E2E bằng `npx playwright test` trong thư mục `tests/playwright/`.


## 4.4. Tiêu chí Pass/Fail và tiêu chí ra

Một test case được tính Pass khi đồng thời thoả mãn các điều kiện sau: mã trả về HTTP đúng đặc tả (thường 200 với GET, 302 với redirect sau form submit, 400 với form lỗi), nội dung response chứa đúng từ khoá mong đợi, trạng thái cơ sở dữ liệu sau khi chạy khớp kỳ vọng, tài nguyên giao diện gồm CSS, JS và ảnh trả mã 200 hoặc 304, và tiếng Việt hiển thị đúng không bị encode sai thành chuỗi entity.

Một test case bị tính Fail nếu xảy ra một trong các trường hợp: HTTP status không khớp đặc tả, trả về mã 5xx ở bất kỳ endpoint nào, response thiếu từ khoá bắt buộc, hiển thị stack trace của Entity Framework Core hoặc NullReferenceException, hoặc xuất hiện mojibake khi chữ Việt bị encode sai thành `&#x...;`.

Kế hoạch kiểm thử được coi là đạt khi 100% test case unit và functional Pass không Fail, từ 95% trở lên test case E2E Pass với phần Skip có lý do rõ ràng, không tồn tại lỗi mức Critical hoặc High ở 14 endpoint public, và các tài liệu thesis ERD, Use Case, Test Cases đồng bộ với mã nguồn. Kế hoạch tạm dừng khi phát hiện lỗi Critical chặn quy trình chính, cơ sở dữ liệu bị hỏng cấu trúc hoặc server không khởi động được; sau khi khắc phục, toàn bộ test suite được chạy lại từ đầu.

## 4.5. Lịch trình thực thi

Bảng 4.4. Lịch trình thực thi kế hoạch kiểm thử

| Giai đoạn | Thời gian | Nội dung chính | Sản phẩm |
|:--|:--|:--|:--|
| Giai đoạn 1 | 01/03 – 15/03/2026 | Viết test cho 4 service chính bằng xUnit, đạt 84 TC | Báo cáo coverage |
| Giai đoạn 2 | 16/03 – 30/03/2026 | Viết ull_test.sh với 94 TC bash | File log và báo cáo |
| Giai đoạn 3 | 01/04 – 20/04/2026 | Viết 20 spec Playwright phủ 4 cổng | HTML report và trace |
| Giai đoạn 4 | 21/04 – 10/05/2026 | Bổ sung 49 mobile test và 42 regression | Screenshot 4 viewport |
| Giai đoạn 5 | 11/05 – 25/05/2026 | Phát hiện và khắc phục 18 lỗi (2C, 9H, 5M, 2L) | Bug tracker |
| Giai đoạn 6 | 26/05 – 31/05/2026 | Tổng hợp kết quả, hoàn tất viết Chương 4 và 5 | Báo cáo Word v5 |

\newpage