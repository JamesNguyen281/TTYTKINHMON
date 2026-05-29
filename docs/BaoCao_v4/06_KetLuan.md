# KẾT LUẬN {.unnumbered}

Sau bốn tháng thực hiện đồ án tốt nghiệp dưới sự hướng dẫn của ThS. Nguyễn Thu Hường, đồ án đã hoàn thành hai mục tiêu đặt ra ban đầu: xây dựng website y tế cho Trung tâm Y tế phường Kinh Môn và triển khai bộ kịch bản kiểm thử tự động bằng Playwright. Kết quả cụ thể được tổng kết dưới đây.

## Kết quả đạt được {.unnumbered}

**a) Về xây dựng website**

Website đã được hoàn thiện và triển khai công khai, đáp ứng các nghiệp vụ ngoại trú của trung tâm y tế cấp phường. Các kết quả chi tiết gồm:

- Hoàn thiện website y tế trên nền tảng ASP.NET Core 8 + EF Core 8 + SQL Server với kiến trúc 3 tầng (Web, Business, Data) phục vụ bốn vai trò: Bệnh nhân, Lễ tân, Bác sĩ, Quản trị;

- Triển khai 25 bảng cơ sở dữ liệu code-first phục vụ đầy đủ các nghiệp vụ ngoại trú: đặt lịch, duyệt, check-in, chẩn đoán, kê đơn, Q&A;

- Áp dụng đầy đủ bảy lớp bảo mật theo OWASP Top 10: PBKDF2-SHA256 600 000 vòng, anti-CSRF, sanitize XSS, lockout sau 5 lần đăng nhập sai, IDOR guard qua site scoping, audit log toàn bộ thay đổi state, kiểm soát độ dài chuỗi chống DoS;

- Thiết kế responsive đáp ứng đầy đủ trên desktop (1366 × 768, 1920 × 1080) và mobile (iPhone X 375, iPhone SE 320), tuân theo chuẩn Apple HIG về tap target ≥ 44 px;

- Tích hợp các tính năng nâng cao: tự động phân lịch trực bác sĩ hằng tháng (manual và cron ngày 28), bộ đếm khách trực tuyến với cửa sổ trượt 15 phút, Q&A có duyệt nội dung, audit log có thể export CSV;

- Triển khai public thành công qua Cloudflare Tunnel với tên miền riêng `https://ttytkm.jamesnguyen28.io.vn`. Hệ thống có thể truy cập từ mọi thiết bị có kết nối Internet, có chứng chỉ TLS 1.3 hợp lệ, đạt điểm Lighthouse ≥ 80 ở cả bốn nhóm tiêu chí (Performance, Accessibility, Best Practices, SEO).

**b) Về kiểm thử tự động bằng Playwright**

Bộ kiểm thử được xây dựng song song với quá trình phát triển website, bao phủ các nghiệp vụ chính và phát hiện một số lỗi trong giai đoạn cài đặt. Các kết quả chi tiết gồm:

- Xây dựng bộ test End-to-End với 279 kịch bản kiểm thử Playwright + TypeScript (264 pass, 15 skip có lý do, 0 fail), bao phủ đầy đủ ba loại kiểm thử theo định hướng đề cương là UI Testing, Functional Testing và Regression Testing trên toàn bộ nghiệp vụ public, portal và member, ở 4 viewport (desktop 1366 / 1920, mobile iPhone X / iPhone SE);

- Bổ sung 8 request smoke test bằng Postman cho các HTTP endpoint trọng yếu (đăng nhập, đặt lịch, duyệt lịch, IDOR guard) và 12 kịch bản kiểm thử thủ công cho các luồng end-to-end và deploy public;

- Phát hiện và khắc phục 18 lỗi trong giai đoạn phát triển: 2 critical (mass-assignment SiteId, EF Core OPENJSON compatibility), 9 high (PBKDF2 yếu, IDOR, XSS, JOIN sai, race condition), 5 medium, 2 low;

- Bộ test giúp giảm thời gian kiểm thử từ 157 phút thủ công xuống còn 12 phút tự động, tiết kiệm khoảng 13 lần;

- Đóng gói website thành Docker container (Dockerfile multi-stage build và docker-compose web + SQL Server), đảm bảo môi trường nhất quán giữa máy phát triển và máy triển khai, sẵn sàng cho việc bàn giao chính thức.

**c) Về kiến thức và kỹ năng cá nhân**

Quá trình thực hiện đồ án góp phần củng cố kiến thức nền và rèn luyện kỹ năng thực hành phục vụ công việc phát triển, kiểm thử phần mềm sau khi tốt nghiệp:

- Củng cố kiến thức về thiết kế và phát triển website ba tầng trên nền ASP.NET Core 8;
- Nắm cách vận hành framework Playwright gồm Playwright Test Runner, Page Object Model, Trace Viewer, HTML Reporter;
- Rèn kỹ năng lập kế hoạch kiểm thử và viết test case theo chuẩn ISTQB;
- Thực hành đầy đủ chu trình kiểm thử từ thiết kế test case, thực thi đến thống kê kết quả;
- Áp dụng lý thuyết vào dự án thực tế có nghiệp vụ y tế và yêu cầu bảo mật cụ thể.

## Hạn chế {.unnumbered}

Bên cạnh các kết quả đạt được, đồ án vẫn còn một số hạn chế cần được khắc phục trong giai đoạn vận hành thực tế:

- Chưa triển khai kiểm thử hiệu năng (performance test, load test) với JMeter hoặc k6, do đó chưa đánh giá được số lượng người dùng đồng thời tối đa hệ thống có thể chịu;

- Chưa kiểm thử trên môi trường staging có dữ liệu thật của Trung tâm Y tế phường Kinh Môn, mới kiểm thử trên dữ liệu seed mô phỏng;

- Chưa ứng dụng được Playwright Component Test cho từng UI component, mới dừng ở mức page-level;

- Chưa triển khai gửi tin nhắn SMS xác nhận lịch hẹn tới số điện thoại thật, đây là hướng mở rộng cần tích hợp với nhà cung cấp SMS Brand Name;

- Chưa tích hợp với hệ thống BHYT để đối soát tự động, hiện tại bệnh nhân BHYT vẫn cần xuất trình thẻ tại quầy lễ tân;

- Một số test case đánh dấu Skip do cần fixture nâng cao (PDF export, gửi mail SMTP thật) chưa được triển khai, không ảnh hưởng kết luận chính nhưng cần bổ sung trong giai đoạn vận hành thực tế;

- Bộ test mobile audit mới phủ hai mẫu iPhone (X, SE), cần mở rộng thêm các mẫu Android phổ biến (Samsung Galaxy A, Xiaomi Redmi) để đảm bảo đa dạng thiết bị.

## Hướng phát triển {.unnumbered}

Từ các kết quả và hạn chế nêu trên, đồ án có thể được phát triển tiếp theo các hướng sau:

- Bổ sung kiểm thử hiệu năng với JMeter hoặc k6, hướng tới mục tiêu hỗ trợ ≥ 500 lượt truy cập đồng thời;
- Triển khai SMS Brand Name thông qua các nhà cung cấp như VNPay SMS, Esms để gửi xác nhận lịch hẹn và nhắc lịch trước 1 giờ;
- Tích hợp BHYT API của Bảo hiểm Xã hội Việt Nam nhằm đối soát tự động thẻ BHYT tại bước check-in;
- Phát triển ứng dụng mobile native bằng React Native hoặc Flutter song song với website để cải thiện trải nghiệm người dùng;
- Bổ sung Playwright Visual Regression Test so sánh ảnh chụp giao diện giữa các phiên bản, phát hiện thay đổi UI ngoài ý muốn;
- Triển khai mô hình multi-tenant đầy đủ để một hệ thống duy nhất phục vụ tất cả TTYT cấp phường tại TP Hải Phòng theo mô hình SaaS y tế công.

\newpage

# TÀI LIỆU THAM KHẢO {.unnumbered}

**Tiếng Việt**

[1] Bộ Y tế, *Quyết định số 4858/QĐ-BYT ban hành Bộ tiêu chí chất lượng bệnh viện Việt Nam*, Hà Nội, 2013.

[2] Bộ Y tế, *Thông tư số 13/2025/TT-BYT quy định hồ sơ bệnh án điện tử*, Hà Nội, 2025.

[3] Bệnh viện Đa khoa Hải Dương, *Đăng ký khám bệnh trực tuyến*, truy cập tại <https://benhviendakhoahaiduong.vn/>, 2026.

[4] Bệnh viện Đa khoa Quốc tế Hải Phòng (HIH), *Hệ thống đặt lịch khám trực tuyến HIH*, truy cập tại <https://hih.vn/>, 2026.

[5] Bệnh viện Hữu nghị Việt Tiệp Hải Phòng, *Đặt lịch khám trực tuyến (Appointment Booking)*, truy cập tại <https://viettiephospital.vn/>, 2026.

[6] Quốc hội nước Cộng hoà xã hội chủ nghĩa Việt Nam, *Luật Khám bệnh, chữa bệnh số 15/2023/QH15*, ban hành ngày 09/01/2023.

[7] Thủ tướng Chính phủ, *Quyết định số 749/QĐ-TTg phê duyệt Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030*, ban hành ngày 03/6/2020.

[8] Trung tâm Y tế phường Kinh Môn, *Trang thông tin điện tử của Trung tâm*, truy cập tại <http://ttytthixakinhmon.vn/>, 2026.

**Tiếng Anh**

[9] Apple Inc., *Human Interface Guidelines — Tap Targets and Touch Areas*, retrieved from <https://developer.apple.com/design/human-interface-guidelines>, 2024.

[10] ISTQB Foundation Level, *Certified Tester Foundation Level Syllabus v4.0*, International Software Testing Qualifications Board, Brussels, 2023.

[11] Lock A., *ASP.NET Core in Action*, Third Edition, Manning Publications, New York, 2024, pp. 1-850.

[12] Microsoft, *ASP.NET Core 8.0 Documentation*, retrieved from <https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0>, 2024.

[13] Microsoft, *Entity Framework Core 8 Documentation*, retrieved from <https://learn.microsoft.com/en-us/ef/core/>, 2024.

[14] Microsoft, *Playwright Documentation — Getting Started, Test Runner, Page Object Models*, retrieved from <https://playwright.dev/docs/intro>, 2024.

[15] Microsoft, *Playwright Test Runner — Best Practices, Fixtures and Auto-waiting*, retrieved from <https://playwright.dev/docs/best-practices>, 2024.

[16] Mozilla Developer Network, *Responsive Web Design — Mobile First Approach*, retrieved from <https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design>, 2024.

[17] OWASP Foundation, *Password Storage Cheat Sheet — PBKDF2 Recommendations*, retrieved from <https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html>, 2023.

[18] OWASP Foundation, *OWASP Top 10 — 2021 Edition*, retrieved from <https://owasp.org/www-project-top-ten/>, 2021.

[19] Tutorialspoint, *Playwright Tutorial — End to End Testing with TypeScript*, retrieved from <https://www.tutorialspoint.com/playwright/index.htm>, 2024.
