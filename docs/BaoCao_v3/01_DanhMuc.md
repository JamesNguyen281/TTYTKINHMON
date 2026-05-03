\newpage

# DANH MỤC CÁC TỪ VIẾT TẮT {.unnumbered}

| **STT** | **Chữ viết tắt** | **Chữ viết đầy đủ** |
|:--:|:--|:--|
| 1 | TTYT | Trung tâm Y tế |
| 2 | HIS | Hospital Information System (Hệ thống thông tin bệnh viện) |
| 3 | EMR | Electronic Medical Record (Bệnh án điện tử) |
| 4 | BHYT | Bảo hiểm y tế |
| 5 | BYT | Bộ Y tế |
| 6 | CNTT | Công nghệ thông tin |
| 7 | CSDL | Cơ sở dữ liệu |
| 8 | ASP.NET | Active Server Pages .NET |
| 9 | MVC | Model – View – Controller |
| 10 | EF Core | Entity Framework Core |
| 11 | E2E | End-to-End (Kiểm thử đầu cuối) |
| 12 | ISTQB | International Software Testing Qualifications Board |
| 13 | OWASP | Open Web Application Security Project |
| 14 | CSRF | Cross-Site Request Forgery |
| 15 | XSS | Cross-Site Scripting |
| 16 | IDOR | Insecure Direct Object Reference |
| 17 | PBKDF2 | Password-Based Key Derivation Function 2 |
| 18 | CI/CD | Continuous Integration / Continuous Deployment |
| 19 | API | Application Programming Interface |
| 20 | UI/UX | User Interface / User Experience |
| 21 | SDK | Software Development Kit |
| 22 | TOC | Table of Contents (Mục lục) |
| 23 | URL | Uniform Resource Locator |
| 24 | HTML | HyperText Markup Language |
| 25 | CSS | Cascading Style Sheets |
| 26 | JS / TS | JavaScript / TypeScript |

\newpage

# LỜI MỞ ĐẦU {.unnumbered}

Trong giai đoạn chuyển đổi số mạnh mẽ của ngành y tế Việt Nam, các website đặt lịch khám trực tuyến đang dần trở thành kênh giao tiếp chính giữa cơ sở y tế và người dân. Mô hình này không chỉ giúp giảm thời gian chờ đợi, nâng cao trải nghiệm người bệnh, mà còn cho phép cơ sở y tế dự báo lưu lượng, tối ưu hóa nhân lực và bám sát các chương trình chuyển đổi số quốc gia theo Quyết định 749/QĐ-TTg ngày 03/6/2020 của Thủ tướng Chính phủ.

Tuy nhiên, hệ thống thông tin y tế là một hệ thống nghiệp vụ phức tạp: phục vụ đồng thời nhiều vai trò người dùng (bệnh nhân, lễ tân, bác sĩ, quản trị), có nhiều luồng nghiệp vụ liên quan tới nhau (đặt lịch — duyệt lịch — check-in — chẩn đoán — kê đơn — hỏi đáp), và phải đáp ứng các yêu cầu khắt khe về bảo mật, audit, sao lưu dữ liệu y tế. Việc kiểm thử thủ công cho hệ thống quy mô như vậy tốn rất nhiều nhân lực, dễ bỏ sót các lỗi hồi quy và khó đảm bảo tính nhất quán giữa các phiên bản.

Xuất phát từ thực tiễn đó, em chọn đề tài **"Nghiên cứu xây dựng website y tế và triển khai kiểm thử tự động bằng Playwright"** làm đồ án tốt nghiệp. Đề tài đặt ra hai mục tiêu song song:

- **Mục tiêu 1:** Xây dựng website y tế cho Trung tâm Y tế phường Kinh Môn (TP Hải Phòng) trên nền tảng ASP.NET Core 8 + Entity Framework Core 8 + SQL Server, đáp ứng các nghiệp vụ chính: đặt lịch khám, duyệt và xác nhận lịch, sinh mã booking, check-in, tạo hồ sơ khám và đơn thuốc, hỏi đáp giữa bệnh nhân và bác sĩ; áp dụng đầy đủ các lớp bảo mật theo OWASP Top 10.

- **Mục tiêu 2:** Thiết kế và triển khai bộ kịch bản kiểm thử tự động bằng Playwright + TypeScript theo mô hình Page Object — Fixture, đồng thời kết hợp với kiểm thử đơn vị (xUnit + EF Core InMemory) cho tầng nghiệp vụ và kiểm thử thủ công cho các luồng cần đánh giá định tính.

Đối tượng nghiên cứu của đồ án là quy trình nghiệp vụ ngoại trú của Trung tâm Y tế cấp phường, tương tác giữa bốn vai trò Bệnh nhân – Lễ tân – Bác sĩ – Quản trị, cùng với công nghệ kiểm thử tự động Playwright. Phạm vi nghiên cứu giới hạn ở các nghiệp vụ ngoại trú phổ biến (đặt lịch, khám, kê đơn, Q&A) — không bao gồm thanh toán BHYT phức tạp, dược nội trú, chẩn đoán hình ảnh.

**Kết cấu đồ án:**

Ngoài phần Lời mở đầu và Kết luận, nội dung đồ án được chia thành bốn chương:

- **Chương 1.** Công nghệ phát triển và kiểm thử website
- **Chương 2.** Khảo sát và thiết kế website y tế TTYT phường Kinh Môn
- **Chương 3.** Phát triển website y tế TTYT phường Kinh Môn
- **Chương 4.** Kiểm thử các chức năng của website bằng Playwright

\newpage
