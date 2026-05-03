# KẾT LUẬN {.unnumbered}

Sau bốn tháng thực hiện đồ án tốt nghiệp dưới sự hướng dẫn tận tình của ThS. Nguyễn Thu Hường, em đã hoàn thành cả hai mục tiêu đề ra ban đầu: xây dựng website y tế cho Trung tâm Y tế phường Kinh Môn và triển khai bộ kịch bản kiểm thử tự động bằng Playwright. Kết quả cụ thể của đồ án được tổng kết như sau.

## Kết quả đạt được {.unnumbered}

**a. Về xây dựng website:**

- Hoàn thiện website y tế trên nền tảng ASP.NET Core 8 + EF Core 8 + SQL Server với kiến trúc 3 tầng (Web — Business — Data) phục vụ bốn vai trò: Bệnh nhân, Lễ tân, Bác sĩ, Quản trị;

- Triển khai 23 bảng cơ sở dữ liệu code-first phục vụ đầy đủ các nghiệp vụ ngoại trú: đặt lịch — duyệt — check-in — chẩn đoán — kê đơn — Q&A;

- Áp dụng đầy đủ bảy lớp bảo mật theo OWASP Top 10: PBKDF2-SHA256 600 000 vòng, anti-CSRF, sanitize XSS, lockout sau 5 lần đăng nhập sai, IDOR guard qua site scoping, audit log toàn bộ thay đổi state, kiểm soát độ dài chuỗi chống DoS;

- Thiết kế responsive đáp ứng đầy đủ trên desktop (1366 × 768, 1920 × 1080) và mobile (iPhone X 375, iPhone SE 320), tuân theo chuẩn Apple HIG về tap target ≥ 44 px;

- Tích hợp các tính năng nâng cao: tự động phân lịch trực bác sĩ hằng tháng (manual + cron ngày 28), bộ đếm khách trực tuyến với cửa sổ trượt 15 phút, Q&A có duyệt nội dung, audit log có thể export CSV;

- **Triển khai public thành công** qua Cloudflare Tunnel với tên miền riêng **`https://ttytkm.jamesnguyen28.io.vn`** — hệ thống có thể truy cập từ mọi thiết bị có kết nối Internet, có chứng chỉ TLS 1.3 hợp lệ, đạt điểm Lighthouse ≥ 80 ở cả bốn nhóm tiêu chí (Performance, Accessibility, Best Practices, SEO).

**b. Về kiểm thử tự động bằng Playwright:**

- Xây dựng bộ test **End-to-End với 251 kịch bản kiểm thử** Playwright + TypeScript (239 pass, 12 skip có lý do, 0 fail) — bao phủ đầy đủ ba loại kiểm thử theo định hướng đề cương: **UI Testing, Functional Testing và Regression Testing** trên toàn bộ nghiệp vụ public, portal và member, ở 4 viewport (desktop 1366 / 1920, mobile iPhone X / iPhone SE);

- Bổ sung 8 request smoke test bằng **Postman** cho các HTTP endpoint trọng yếu (đăng nhập, đặt lịch, duyệt lịch, IDOR guard) và 12 kịch bản kiểm thử thủ công cho các luồng end-to-end + deploy public;

- Phát hiện và khắc phục **18 lỗi** trong giai đoạn phát triển: 2 critical (mass-assignment SiteId, EF Core OPENJSON compatibility), 9 high (PBKDF2 yếu, IDOR, XSS, JOIN sai, race condition…), 5 medium, 2 low;

- Bộ test giảm thời gian kiểm thử từ **157 phút thủ công xuống còn 4 phút 31 giây tự động — tiết kiệm khoảng 35 lần**;

- Đóng gói website thành **Docker container** (Dockerfile multi-stage build + docker-compose web + SQL Server) đảm bảo môi trường nhất quán giữa máy phát triển và máy triển khai, sẵn sàng cho việc bàn giao chính thức.

**c. Về kiến thức và kỹ năng cá nhân:**

- Củng cố kiến thức tổng quan về thiết kế và phát triển website 3 tầng theo chuẩn ASP.NET Core 8;
- Hiểu rõ và nắm bắt được cách hoạt động của framework kiểm thử Playwright bao gồm Playwright Test Runner, Page Object Model, Trace Viewer, HTML Reporter;
- Nắm bắt được kỹ năng phân chia thời gian công việc, lập kế hoạch kiểm thử và viết test case theo chuẩn ISTQB;
- Biết cách viết test case, xây dựng kế hoạch kiểm thử, thực thi và thống kê báo cáo một cách hợp lý;
- Ứng dụng lý thuyết vào thực tiễn dự án phát triển website y tế có nghiệp vụ phức tạp và yêu cầu bảo mật cao.

## Hạn chế {.unnumbered}

- Chưa triển khai kiểm thử hiệu năng (performance / load test) với JMeter hoặc k6 — chưa đánh giá được số lượng người dùng đồng thời tối đa hệ thống có thể chịu;

- Chưa kiểm thử trên môi trường staging có dữ liệu thật của Trung tâm Y tế phường Kinh Môn — mới kiểm thử trên dữ liệu seed mô phỏng;

- Chưa ứng dụng được Playwright Component Test cho từng UI component (mới ở mức page-level);

- Chưa triển khai gửi tin nhắn SMS xác nhận lịch hẹn tới SĐT thật — đây là hướng mở rộng cần tích hợp với nhà cung cấp SMS Brand Name;

- Chưa tích hợp với hệ thống BHYT để đối soát tự động — hiện tại bệnh nhân BHYT vẫn cần xuất trình thẻ tại quầy lễ tân;

- Một số test case đánh dấu Skip do cần fixture nâng cao (PDF export, gửi mail SMTP thật) chưa được triển khai — không ảnh hưởng kết luận chính nhưng cần bổ sung trong giai đoạn vận hành thực tế;

- Bộ test mobile audit mới phủ hai mẫu iPhone (X, SE) — cần mở rộng thêm các mẫu Android phổ biến (Samsung Galaxy A, Xiaomi Redmi) để đảm bảo đa dạng thiết bị.

## Hướng phát triển {.unnumbered}

Trên cơ sở những kết quả và hạn chế nêu trên, đồ án có thể được phát triển tiếp theo các hướng sau:

- **Bổ sung kiểm thử hiệu năng** với JMeter / k6, mục tiêu hỗ trợ ≥ 500 lượt truy cập đồng thời;
- **Triển khai SMS Brand Name** thông qua các nhà cung cấp như VNPay SMS, Esms để gửi xác nhận lịch hẹn và nhắc lịch trước 1 giờ;
- **Tích hợp BHYT API** của Bảo hiểm Xã hội Việt Nam để đối soát tự động thẻ BHYT tại bước check-in;
- **Phát triển ứng dụng mobile native** (React Native hoặc Flutter) song song với website để cải thiện trải nghiệm người dùng;
- **Bổ sung Playwright Visual Regression Test** so sánh ảnh chụp giao diện giữa các phiên bản, phát hiện thay đổi UI ngoài ý muốn;
- **Triển khai mô hình multi-tenant đầy đủ** để một hệ thống duy nhất phục vụ tất cả TTYT cấp phường tại TP Hải Phòng (mô hình SaaS y tế công).

\newpage

# DANH MỤC TÀI LIỆU THAM KHẢO {.unnumbered}

**Văn bản pháp luật và hướng dẫn**

[1] Quốc hội nước Cộng hòa xã hội chủ nghĩa Việt Nam, *Luật Khám bệnh, chữa bệnh số 15/2023/QH15*, ban hành ngày 09/01/2023.

[2] Bộ Y tế, *Thông tư số 13/2025/TT-BYT quy định hồ sơ bệnh án điện tử*, ban hành năm 2025.

[3] Thủ tướng Chính phủ, *Quyết định số 749/QĐ-TTg ngày 03/6/2020 phê duyệt Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030*.

[4] Bộ Y tế, *Quyết định số 4858/QĐ-BYT ban hành Bộ tiêu chí chất lượng bệnh viện Việt Nam*.

**Tài liệu kỹ thuật về .NET và ASP.NET Core**

[5] Microsoft, *ASP.NET Core 8.0 Documentation*. Truy cập tại: <https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0>

[6] Microsoft, *Entity Framework Core 8 Documentation*. Truy cập tại: <https://learn.microsoft.com/en-us/ef/core/>

[7] Andrew Lock, *ASP.NET Core in Action — Third Edition*, Manning Publications, 2024.

**Tài liệu kỹ thuật về Playwright**

[8] Microsoft, *Playwright Documentation — Getting Started, Test Runner, Page Object Models*. Truy cập tại: <https://playwright.dev/docs/intro>

[9] Microsoft, *Playwright Test Runner — Best Practices, Fixtures and Auto-waiting*. Truy cập tại: <https://playwright.dev/docs/best-practices>

[10] Tutorialspoint, *Playwright Tutorial — End to End Testing with TypeScript*. Truy cập tại: <https://www.tutorialspoint.com/playwright/index.htm>

**Tài liệu về kiểm thử phần mềm và bảo mật**

[11] ISTQB Foundation Level Syllabus, *Certified Tester Foundation Level Syllabus v4.0*, International Software Testing Qualifications Board, 2023.

[12] OWASP Foundation, *OWASP Top 10 — 2021 Edition*. Truy cập tại: <https://owasp.org/www-project-top-ten/>

[13] OWASP Cheat Sheet Series, *Password Storage Cheat Sheet — PBKDF2 Recommendations*. Truy cập tại: <https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html>

**Tài liệu khảo sát các nền tảng tham khảo**

[14] Bệnh viện Đa khoa Quốc tế Hải Phòng (HIH). Truy cập tại: <https://hih.vn/>

[15] Bệnh viện Đa khoa Hải Dương. Truy cập tại: <https://benhviendakhoahaiduong.vn/>

[16] Bệnh viện Hữu nghị Việt Tiệp Hải Phòng. Truy cập tại: <https://viettiephospital.vn/>

[17] Trung tâm Y tế phường Kinh Môn (cơ sở khảo sát). Truy cập tại: <http://ttytthixakinhmon.vn/>

**Tài liệu về responsive web design và Apple Human Interface Guidelines**

[18] Apple Inc., *Human Interface Guidelines — Tap Targets and Touch Areas*. Truy cập tại: <https://developer.apple.com/design/human-interface-guidelines>

[19] Mozilla Developer Network (MDN), *Responsive Web Design — Mobile First Approach*. Truy cập tại: <https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design>
