# CHƯƠNG 1. CÔNG NGHỆ PHÁT TRIỂN VÀ KIỂM THỬ WEBSITE

## 1.1. Căn cứ pháp lý và sự cần thiết triển khai

### 1.1.1. Căn cứ pháp lý

Việc xây dựng và triển khai website đặt lịch khám trực tuyến cho Trung tâm Y tế phường Kinh Môn (TTYT Kinh Môn) được căn cứ trên các văn bản quy phạm pháp luật và chỉ đạo sau:

- **Luật Khám bệnh, chữa bệnh năm 2023** và các văn bản hướng dẫn thi hành — quy định quyền và nghĩa vụ của người bệnh, trách nhiệm của cơ sở khám chữa bệnh trong tổ chức tiếp nhận, khám chữa bệnh và quản lý hồ sơ y tế;

- **Thông tư 13/2025/TT-BYT** của Bộ Y tế quy định về hồ sơ bệnh án điện tử — làm cơ sở để website lưu trữ hồ sơ khám, đơn thuốc dưới dạng điện tử thay cho bệnh án giấy;

- **Quyết định 749/QĐ-TTg ngày 03/6/2020** của Thủ tướng Chính phủ phê duyệt Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030 — đặt mục tiêu y tế là một trong những lĩnh vực ưu tiên chuyển đổi số;

- **Quyết định 4858/QĐ-BYT** của Bộ Y tế ban hành Bộ tiêu chí chất lượng bệnh viện Việt Nam — trong đó có các tiêu chí về ứng dụng công nghệ thông tin, cải tiến quy trình tiếp nhận và rút ngắn thời gian chờ khám;

- **Kế hoạch hoạt động năm 2026** của Trung tâm Y tế phường Kinh Môn về nâng cao chất lượng dịch vụ khám chữa bệnh, đẩy mạnh ứng dụng công nghệ thông tin;

- **Chủ trương của Ban Giám đốc Trung tâm** về nâng cao chất lượng khám chữa bệnh và ứng dụng công nghệ thông tin trong tiếp nhận bệnh nhân ngoại trú.

### 1.1.2. Sự cần thiết triển khai

Trung tâm Y tế phường Kinh Môn — tiền thân là **Bệnh viện Đa khoa Kinh Môn**, cơ sở y tế công lập **hạng II đạt chuẩn quốc gia** về quy mô và trang thiết bị, từng trực thuộc Sở Y tế Hải Dương — sau khi sáp nhập đơn vị hành chính được tổ chức lại thành TTYT phường, tập trung vào khám chữa bệnh ngoại trú và y tế cộng đồng. Hiện Trung tâm vẫn duy trì đầy đủ các khoa phòng chức năng theo chuẩn hạng II, song toàn bộ quy trình tiếp nhận bệnh nhân vẫn theo hình thức đến trực tiếp, **chưa có hệ thống đặt lịch hẹn trước**. Thực trạng này dẫn đến một số hạn chế cần khắc phục:

- **Bệnh nhân phải chờ đợi lâu**, không chủ động được thời gian, ảnh hưởng đến trải nghiệm và sự hài lòng của người bệnh;

- **Trung tâm chưa dự báo được lưu lượng** bệnh nhân theo ngày / theo ca khám, khó tối ưu hóa phân bổ nhân lực bác sĩ và lễ tân;

- **Năng lực cạnh tranh** của Trung tâm trong bối cảnh chuyển đổi số y tế còn hạn chế so với các cơ sở y tế tư nhân và bệnh viện tuyến trên đã có ứng dụng đặt lịch online;

- **Chưa khai thác được tiềm năng** từ BHYT và các dịch vụ theo yêu cầu — nhiều bệnh nhân có nhu cầu khám theo yêu cầu nhưng không biết được lịch trống của bác sĩ;

- **Xu thế người dân ngày càng quen** với việc đặt lịch khám online qua app, website tương tự như đặt vé máy bay, đặt khách sạn — đặc biệt với nhóm tuổi lao động (19–55 tuổi).

Việc triển khai hệ thống đặt lịch hẹn từ xa là bước đi phù hợp với lộ trình chuyển đổi số ngành y tế, nâng cao chất lượng dịch vụ, đồng thời phù hợp với quy mô và điều kiện thực tế của Trung tâm Y tế cấp phường. Bên cạnh đó, bộ kịch bản kiểm thử tự động được xây dựng song song với hệ thống là cơ sở quan trọng đảm bảo chất lượng phần mềm trong các vòng cập nhật, bảo trì sau này.

## 1.2. Yêu cầu xây dựng website

Trên cơ sở khảo sát thực trạng và căn cứ pháp lý nêu trên, website y tế của TTYT phường Kinh Môn cần đáp ứng các yêu cầu sau:

- Cho phép **bệnh nhân** (kể cả khách vãng lai chưa có tài khoản) đặt lịch khám trực tuyến theo bác sĩ, theo chuyên khoa, theo ngày và ca khám (sáng / chiều); xem trạng thái lịch hẹn (chờ duyệt / đã xác nhận / đã khám); xem lại lịch sử khám bệnh và đơn thuốc khi đã đăng nhập tài khoản;

- Cho phép **lễ tân** duyệt và xác nhận các lịch hẹn ở trạng thái chờ, sinh mã booking, từ chối lịch không hợp lệ, tìm kiếm lịch theo số điện thoại hoặc theo ngày, thực hiện check-in tại cơ sở;

- Cho phép **bác sĩ** xem danh sách bệnh nhân được phân công, chẩn đoán và lập hồ sơ khám, kê đơn thuốc với kiểm soát độ dài chuỗi ký tự, xem lịch trực, trả lời câu hỏi của bệnh nhân;

- Cho phép **quản trị viên** quản lý toàn bộ hệ thống: tài khoản người dùng, danh mục bác sĩ – chuyên khoa – dịch vụ, tin tức, hỏi đáp, sao lưu dữ liệu, xem nhật ký kiểm toán (audit log), tự động phân lịch trực hằng tháng;

- Hỗ trợ giao diện thân thiện trên cả máy tính (1366 × 768, 1920 × 1080) và thiết bị di động (iPhone X 375 × 812, iPhone SE 320 × 568), theo chuẩn responsive web design;

- Đảm bảo bảo mật theo OWASP Top 10: chống tấn công CSRF, XSS, SQL Injection, IDOR; mã hóa mật khẩu bằng PBKDF2-SHA256 với 600 000 vòng lặp; khóa tài khoản sau 5 lần đăng nhập sai liên tiếp.

## 1.3. Yêu cầu hệ thống

### 1.3.1. Yêu cầu phi chức năng

**a. Yêu cầu về sản phẩm:**

- Tốc độ truy cập trang nhanh, thời gian phản hồi của các thao tác chính dưới 1 giây trong điều kiện mạng nội bộ và dưới 3 giây qua kết nối Internet thông thường;
- Độ tin cậy cao, hệ thống chạy ổn định 24/7, không có lỗi 500 trong các nghiệp vụ chuẩn;
- Bộ nhớ và tài nguyên server được tối ưu, có thể chạy được trên cấu hình thấp (4 GB RAM, 2 vCPU, SSD 40 GB);
- Giao diện thân thiện, sử dụng tiếng Việt làm ngôn ngữ chính, có thể mở rộng song ngữ Việt – Anh nhờ thiết kế cột song song `name_l` / `name_e`.

**b. Yêu cầu về quá trình phát triển:**

- Tuân thủ chuẩn lập trình **ASP.NET Core MVC** và mô hình kiến trúc 3 tầng (Web — Business — Data);
- Sử dụng **Entity Framework Core** với mô hình code-first, có quản lý migrations;
- Mã nguồn được quản lý phiên bản bằng **Git**, đẩy lên GitHub;
- Sử dụng các công cụ phát triển và đóng gói tiêu chuẩn: Visual Studio 2022, Visual Studio Code, Postman (kiểm thử HTTP endpoint), Docker (đóng gói triển khai).

**c. Yêu cầu bảo mật:**

Vì website lưu trữ thông tin sức khỏe của người dân — thuộc dạng dữ liệu nhạy cảm được pháp luật bảo vệ — đồ án đặt ra các yêu cầu cụ thể như sau:

- **Lưu trữ mật khẩu an toàn:** không lưu mật khẩu dạng plain text; mật khẩu được băm bằng thuật toán PBKDF2 kết hợp SHA-256 với salt riêng cho từng tài khoản và 600.000 vòng lặp theo khuyến nghị mới nhất của OWASP năm 2023. Cookie phiên có chữ ký số chống giả mạo;

- **Sao lưu định kỳ:** lập lịch sao lưu CSDL hằng tuần ra thiết bị lưu trữ rời; trong trường hợp sự cố phần cứng có thể khôi phục lại trong vòng vài giờ;

- **Phục hồi nhanh:** kết hợp tệp backup hằng tuần với cơ chế EF Core Migration giúp tái tạo schema từ đầu khi cần — bảo đảm thời gian gián đoạn dịch vụ ở mức tối thiểu;

- **Lưu vết kiểm toán (audit log):** mỗi khi trạng thái lịch hẹn, hồ sơ khám hay đơn thuốc thay đổi, hệ thống tự ghi nhận một bản ghi vào bảng `audit_system` gồm giá trị cũ, giá trị mới, người thực hiện và lý do (nếu có). Cơ chế này hỗ trợ truy vết khi xảy ra tranh chấp giữa bệnh nhân và Trung tâm.

### 1.3.2. Yêu cầu chức năng hệ thống

**a. Chức năng cho khách vãng lai (chưa đăng nhập):**

- Xem trang chủ, danh sách bác sĩ, danh sách chuyên khoa, tin tức, hỏi đáp, liên hệ;
- Đặt lịch khám không cần đăng ký tài khoản (nhập thông tin cá nhân tại form);
- Xem mã đặt lịch và trạng thái sau khi đặt thành công nhờ Session bảo mật;
- Đăng ký tài khoản mới hoặc đăng nhập để truy cập các tính năng nâng cao.

**b. Chức năng cho bệnh nhân (đã đăng nhập — Member):**

- Đặt lịch khám trực tuyến với thông tin được lưu sẵn từ hồ sơ;
- Xem danh sách lịch hẹn của mình theo trạng thái: Chờ duyệt — Đã xác nhận — Đã khám — Đã hủy;
- Xem lại lịch sử khám bệnh, hồ sơ khám và đơn thuốc;
- Đặt câu hỏi cho bác sĩ và xem trả lời;
- Cập nhật thông tin cá nhân, đổi mật khẩu.

**c. Chức năng cho lễ tân (Reception):**

- Xem danh sách lịch hẹn theo trạng thái, sắp xếp theo ngày — ca — bác sĩ;
- Xác nhận hoặc từ chối lịch hẹn (có yêu cầu lý do từ chối);
- Tìm kiếm lịch hẹn theo số điện thoại bệnh nhân;
- Tra cứu lịch theo ngày bất kỳ trong vòng ± 30 ngày;
- Check-in bệnh nhân tại quầy.

**d. Chức năng cho bác sĩ (Doctor):**

- Xem danh sách bệnh nhân được phân công khám;
- Lập hồ sơ chẩn đoán và kê đơn thuốc với kiểm soát độ dài chuỗi (chống tấn công DoS);
- Xem lịch trực của mình;
- Trả lời các câu hỏi mà bệnh nhân gửi tới.

**e. Chức năng cho quản trị viên (Admin):**

- Quản lý toàn bộ tài khoản, phân quyền theo nhóm (Admin / Reception / Doctor / Member);
- Quản lý danh mục: bác sĩ, chuyên khoa, dịch vụ, slot khám, tin tức, hỏi đáp;
- Tự động phân lịch trực hằng tháng cho bác sĩ (manual trigger + cron tự động ngày 28);
- Xem nhật ký kiểm toán (audit log) toàn hệ thống;
- Sao lưu — phục hồi cơ sở dữ liệu;
- Cấu hình thông tin chung: tên đơn vị, logo, địa chỉ, hotline, email, banner.

## 1.4. Kiểm thử phần mềm bằng Playwright

### 1.4.1. Cơ sở lựa chọn Playwright

Qua khảo sát lý thuyết kiểm thử và thực nghiệm sơ bộ ba công cụ kiểm thử tự động phổ biến nhất hiện nay (Selenium, Cypress, Playwright), Playwright được lựa chọn cho đồ án dựa trên các tiêu chí kỹ thuật sau: hỗ trợ đồng thời ba nhân trình duyệt thông dụng (Chromium, Firefox, WebKit) chỉ với một bộ kịch bản; tích hợp sẵn TypeScript với hệ kiểu tĩnh chặt chẽ; có cơ chế đợi phần tử tự động giúp giảm các lỗi không ổn định do thời gian.

Playwright là sản phẩm mã nguồn mở do nhóm phát triển trình duyệt Edge của Microsoft công bố ra cộng đồng đầu năm 2020 dưới giấy phép Apache 2.0. Điểm khác biệt quan trọng so với Selenium nằm ở giao thức truyền tin: Selenium giao tiếp gián tiếp qua chuẩn WebDriver, trong khi Playwright kết nối thẳng tới trình duyệt qua DevTools Protocol. Cách tiếp cận này giúp giảm độ trễ giữa lệnh kịch bản và thao tác thực tế trên trang. Ngoài ra, mỗi `locator` của Playwright tự động chờ phần tử đạt trạng thái khả dụng (xuất hiện, ổn định, có thể tương tác) trước khi thực thi action, qua đó hạn chế lỗi "tìm thấy phần tử nhưng chưa kịp render" thường xuất hiện khi kiểm thử các trang có nhiều thao tác bất đồng bộ.

### 1.4.2. Các đặc điểm chính được khai thác trong đồ án

Trong quá trình triển khai bộ test End-to-End cho website TTYT phường Kinh Môn, các đặc điểm sau của Playwright được khai thác trực tiếp:

- **Auto-wait:** mọi action `locator.click()`, `locator.fill()` đều chờ phần tử ở trạng thái `stable`, `visible`, `enabled` trước khi thực thi, không cần khai báo `setTimeout` hay vòng lặp polling thủ công như Selenium;

- **Đa trình duyệt và mô phỏng thiết bị:** chỉ với một tệp `playwright.config.ts`, cùng một bộ test có thể chạy lần lượt trên Chromium, Firefox, WebKit; đồng thời mô phỏng các viewport iPhone X, iPhone SE, iPad ngay trong test runner — điểm này được tận dụng triệt để cho phần Mobile Audit ở Chương 4;

- **Trace Viewer:** mỗi test fail tự sinh tệp `trace.zip` chứa DOM snapshot, ảnh chụp từng action và lưu lượng mạng; lệnh `npx playwright show-trace` cho phép xem lại toàn bộ kịch bản dưới dạng "tua video", hỗ trợ tốt cho công tác debug;

- **Code generation:** công cụ `npx playwright codegen` ghi nhận thao tác trên trình duyệt và sinh ra mã TypeScript tương đương, hữu ích trong giai đoạn làm quen API ban đầu trước khi tổ chức lại theo Page Object;

- **Thực thi song song:** mặc định mỗi tệp spec chạy trong một worker riêng. Trên máy phát triển 8 lõi, bộ 269 test case hoàn thành trong khoảng 12 phút — rút ngắn đáng kể so với phương án tuần tự.

### 1.4.3. Tổ chức bộ test theo Page Object Model

Để tránh việc các locator (chuỗi CSS / XPath / text) nằm rải rác trong nhiều tệp spec — gây khó khăn khi giao diện thay đổi (ví dụ đổi tên class `.btn-confirm` thành `.btn-primary` sẽ kéo theo phải sửa hàng chục chỗ) — đồ án áp dụng mô hình **Page Object Model (POM)** kết hợp **Fixture pattern**:

- Mỗi nhóm trang chính (Trang chủ, Đặt lịch khám, Cổng Lễ tân, Cổng Bác sĩ, AdminCP) được đóng gói thành một lớp TypeScript ở thư mục `pages/`. Lớp này chứa locator và các phương thức nghiệp vụ kiểu `bookAppointment(payload)` thay vì để spec gọi click trực tiếp;

- Các luồng tái sử dụng nhiều lần như đăng nhập lễ tân, đăng nhập bác sĩ được đóng gói thành các fixture `loginStaff`, `loginMember`. Mỗi spec chỉ cần khai báo `test.use({ ...loginStaff })` là có sẵn phiên đăng nhập, không phải lặp lại đoạn code đăng nhập 5–6 dòng.

Các thành phần kỹ thuật chính của Playwright được tham chiếu trong đồ án gồm:

- `Browser` — instance trình duyệt (Chromium / Firefox / WebKit);
- `BrowserContext` — phiên trình duyệt độc lập tương đương cửa sổ ẩn danh; mỗi test sử dụng context riêng để tránh chia sẻ cookie / localStorage gây nhiễu kết quả;
- `Page` — tab trình duyệt nơi các thao tác kiểm thử diễn ra;
- `Locator` — cơ chế trỏ tới phần tử có hỗ trợ retry tự động và assertion `expect(locator).toBeVisible()` tự đợi đến khi phần tử khả dụng.

### 1.4.4. Đánh giá ưu — nhược điểm

**Ưu điểm:**

- Tốc độ thực thi cao — bộ 269 test E2E hoàn thành trong khoảng 12 phút trên máy phát triển cá nhân;
- Cú pháp test ngắn gọn nhờ cơ chế auto-wait, không cần khai báo `WebDriverWait` rườm rà như Selenium;
- TypeScript strict mode phát hiện sớm các lỗi gõ sai locator ngay tại bước biên dịch, không phải chờ đến lúc chạy;
- Trace Viewer cung cấp tư liệu debug đầy đủ cho từng test fail, đặc biệt giá trị khi xảy ra lỗi giao diện do phần tử bị che phủ (overlay, loading spinner);
- HTML Reporter tích hợp sẵn, không cần cài đặt thêm thư viện báo cáo bên ngoài như Allure hay Mocha Reporter.

**Nhược điểm:**

- Tài liệu tiếng Việt còn hạn chế; quá trình tra cứu chủ yếu phải dựa vào tài liệu tiếng Anh và các issue trên GitHub;
- Không hỗ trợ các trình duyệt thế hệ cũ (Internet Explorer, Edge Legacy) — không gây trở ngại lớn cho đề tài hiện tại nhưng cần lưu ý nếu mở rộng phạm vi kiểm thử;
- Dung lượng cài đặt cho ba browser khoảng 500 MB, lớn hơn nhiều so với mức ~50 MB của Selenium WebDriver;
- Phần Mobile Audit chỉ dừng ở mức mô phỏng viewport; muốn kiểm thử trên thiết bị Android / iOS vật lý cần kết hợp thêm Appium hoặc dịch vụ BrowserStack.

## 1.5. Công nghệ và công cụ phát triển website

Trên cơ sở phân tích yêu cầu hệ thống và đặc điểm nghiệp vụ y tế, đồ án sử dụng nhóm công nghệ và công cụ sau đây — đúng theo định hướng đề cương đã được phê duyệt:

### 1.5.1. Ngôn ngữ lập trình

- **C#** — ngôn ngữ chính phát triển backend website. C# là ngôn ngữ hướng đối tượng do Microsoft phát triển, chạy trên nền tảng .NET Core 8 đa nền tảng (Windows / Linux / macOS), có hệ thống kiểu mạnh giúp phát hiện sớm lỗi tại thời điểm biên dịch;

- **TypeScript** — ngôn ngữ chính viết kịch bản kiểm thử Playwright. TypeScript là superset của JavaScript bổ sung kiểm tra kiểu tĩnh, IntelliSense, hỗ trợ refactor an toàn — phù hợp cho các bộ test có quy mô lớn cần dễ bảo trì.

### 1.5.2. Công nghệ frontend

- **Razor View Engine** — bộ render template đi kèm ASP.NET Core MVC. Cú pháp `@` của Razor cho phép trộn mã C# vào HTML một cách tự nhiên; trong đồ án, các tính năng tag helper, partial view và layout chung được sử dụng để giảm trùng lặp giữa nhiều trang;

- **HTML 5** + **CSS 3** — bộ chuẩn nền tảng cho ứng dụng web hiện nay. Đồ án sử dụng các thẻ semantic (`<section>`, `<nav>`, `<article>`) nhằm hỗ trợ công cụ trợ giúp truy cập đọc đúng nội dung; layout chính dựa trên Flexbox và CSS Grid để thuận tiện điều chỉnh theo từng kích thước màn hình;

- **Bootstrap 5** — thư viện CSS được nhiều dự án website công lập tại Việt Nam sử dụng. Bootstrap được chọn thay vì viết CSS từ đầu nhờ hệ grid 12 cột cùng các thành phần dựng sẵn (navbar, modal, form-control, card), giúp dựng giao diện nhanh và bao phủ đầy đủ các viewport cần kiểm thử (320 → 1920 px). Phần CSS riêng phục vụ nhận diện thương hiệu của Trung tâm được bổ sung trong tệp `site.css`;

- **JavaScript (ES2022)** — đảm nhận các tương tác phía client như xác thực form, gọi AJAX cho bộ đếm khách trực tuyến, đóng/mở modal xác nhận hủy lịch và lazy-load ảnh tin tức.

### 1.5.3. Công nghệ backend

- **ASP.NET Core MVC (.NET 8)** — phiên bản LTS mới nhất của Microsoft tại thời điểm bắt đầu đồ án (tháng 02/2026), hỗ trợ chạy đa nền tảng Windows / Linux. Đồ án khai thác cơ chế Dependency Injection có sẵn để inject các Service vào Controller, sử dụng Middleware pipeline cho các yêu cầu chéo (logging, exception, authorization), đồng thời bật chế độ Razor runtime compilation để chỉnh sửa tệp `.cshtml` không cần build lại;

- **Mô hình kiến trúc 3 tầng** — solution được tách thành ba project tương ứng ba lớp: `WebsiteCore.Data` chứa entity và `DbContext`; `WebsiteCore.Business` chứa các Service và view model; `WebsiteCore.Web` là lớp giao diện gồm controller và Razor view. Cách phân tách này thuận tiện cho việc viết unit test cho tầng nghiệp vụ mà không cần khởi động web server, đồng thời mở khả năng tái sử dụng tầng Data và Business cho các kênh giao tiếp khác (ứng dụng mobile, public API…) trong tương lai.

### 1.5.4. Cơ sở dữ liệu và ORM

- **SQL Server (Express Edition)** — hệ quản trị cơ sở dữ liệu quan hệ của Microsoft. Lý do chọn SQL Server thay vì MySQL hay PostgreSQL gồm ba điểm: (1) phần lớn cơ sở y tế công lập tại Việt Nam đang vận hành trên SQL Server hoặc Oracle, do đó deploy thực tế ít gặp rủi ro tương thích; (2) khả năng tích hợp với .NET tốt nhất trong các hệ quản trị phổ biến; (3) bản Express miễn phí và đi kèm SQL Server Management Studio cho công tác quản trị giao diện đồ họa. Toàn bộ schema sử dụng kiểu `NVARCHAR` Unicode để bảo đảm lưu trữ tiếng Việt có dấu chính xác;

- **Entity Framework Core (EF Core)** — thư viện ORM chính thức của Microsoft cho nền tảng .NET. Đồ án áp dụng phương pháp **code-first**: định nghĩa các lớp entity bằng C# trước, sau đó dùng lệnh `dotnet ef migrations add` để sinh tệp migration tương ứng và `dotnet ef database update` để áp dụng schema vào CSDL. EF Core hỗ trợ truy vấn LINQ với kiểm tra kiểu chặt chẽ (sai tên cột báo lỗi tại bước biên dịch thay vì lúc chạy), kèm các tính năng change tracking, transaction và lazy loading tích hợp sẵn.

### 1.5.5. Công cụ kiểm thử

- **Playwright Test Runner** — framework kiểm thử E2E mã nguồn mở do Microsoft phát triển (đã trình bày chi tiết ở mục 1.4). Đồ án sử dụng Playwright + TypeScript để bao phủ ba loại kiểm thử: **UI Testing**, **Functional Testing** và **Regression Testing** theo định hướng đề cương.

### 1.5.6. Công cụ hỗ trợ phát triển

**Bảng 1.1. Danh sách công cụ hỗ trợ phát triển và đóng gói**

| **STT** | **Công cụ** | **Phiên bản** | **Vai trò trong đồ án** |
|:--:|:--|:--|:--|
| 1 | Visual Studio 2022 Community | 17.12 | IDE chính cho phát triển ASP.NET Core, cung cấp debugger, hot-reload, IntelliSense, scaffolding migrations |
| 2 | Visual Studio Code | 1.95 | Editor nhẹ phục vụ chỉnh sửa file `.ts` (Playwright), file cấu hình YAML/JSON, viết tài liệu Markdown |
| 3 | Git | 2.46 | Hệ thống quản lý phiên bản phân tán, đồng bộ mã nguồn lên GitHub, hỗ trợ branch / merge / blame |
| 4 | **Postman** | 11.x | Công cụ kiểm thử HTTP endpoint thủ công — gửi request GET/POST/PUT/DELETE tới các route MVC, kiểm tra mã trạng thái, header, body. Sử dụng để xác minh nhanh các luồng đăng nhập, đặt lịch, duyệt lịch trước khi viết test Playwright |
| 5 | **Docker** | Desktop 4.x | Đóng gói website thành container chuẩn — gồm `Dockerfile` multi-stage build (SDK build → runtime ASP.NET Core 8) và `docker-compose.yml` chạy đồng thời container web + container SQL Server, đảm bảo môi trường nhất quán giữa máy phát triển và máy triển khai |
| 6 | SQL Server Management Studio | 20.x | Công cụ quản trị CSDL: chạy migration, kiểm tra schema, tra cứu dữ liệu, tối ưu index |
| 7 | Node.js + npm | 20 LTS | Runtime cho Playwright Test Runner, quản lý gói TypeScript |

Việc sử dụng **Postman** rút ngắn đáng kể thời gian khi cần kiểm tra nhanh một endpoint mới chưa kịp viết test tự động — ví dụ kiểm tra mã trạng thái khi POST thiếu CSRF token, hoặc kiểm tra header `Set-Cookie` sau khi đăng nhập. Bộ collection được lưu thành tệp `.postman_collection.json` để có thể tái sử dụng.

Việc sử dụng **Docker** giúp đóng gói website thành ảnh container có thể chạy độc lập trên bất kỳ máy chủ nào có Docker Engine, không phụ thuộc cấu hình .NET runtime của máy host. Đây là bước chuẩn bị quan trọng cho việc triển khai chính thức trên môi trường thật của Trung tâm Y tế phường Kinh Môn trong tương lai.

\newpage
