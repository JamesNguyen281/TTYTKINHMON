# CHƯƠNG 1. GIỚI THIỆU ĐỀ TÀI

## 1.1. Đặt vấn đề

### 1.1.1. Bối cảnh chuyển đổi số ngành y tế

Ngành y tế Việt Nam đang trong giai đoạn chuyển đổi số mạnh mẽ theo *Quyết định 749/QĐ-TTg ngày 03/6/2020* của Thủ tướng Chính phủ phê duyệt *Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030*, trong đó y tế được xác định là một trong tám lĩnh vực ưu tiên. Cùng với *Luật Khám bệnh, chữa bệnh năm 2023*, *Thông tư 13/2025/TT-BYT* về hồ sơ bệnh án điện tử và *Quyết định 4858/QĐ-BYT* về Bộ tiêu chí chất lượng bệnh viện, các văn bản pháp lý đã hình thành khung pháp lý đầy đủ cho việc số hóa quy trình khám chữa bệnh tại các cơ sở y tế công lập.

Trong bối cảnh này, các website đặt lịch khám trực tuyến trở thành kênh giao tiếp chính giữa cơ sở y tế và người dân. Mô hình đặt lịch trực tuyến giúp giảm thời gian chờ đợi, nâng cao trải nghiệm bệnh nhân, đồng thời cho phép cơ sở y tế dự báo lưu lượng và tối ưu hóa nhân lực. Người dân ngày càng quen với việc đặt lịch online qua app/website tương tự cách đặt vé máy bay hay khách sạn — đặc biệt là nhóm tuổi lao động (19–55 tuổi).

### 1.1.2. Thực trạng tại Trung tâm Y tế phường Kinh Môn

Trung tâm Y tế phường Kinh Môn — tiền thân là **Bệnh viện Đa khoa Kinh Môn**, cơ sở y tế công lập **hạng II đạt chuẩn quốc gia** về quy mô và trang thiết bị, từng trực thuộc Sở Y tế Hải Dương — sau khi sáp nhập đơn vị hành chính được tổ chức lại thành TTYT phường, tập trung vào khám chữa bệnh ngoại trú và y tế cộng đồng. Hiện Trung tâm vẫn duy trì đầy đủ các khoa phòng chức năng theo chuẩn hạng II, song toàn bộ quy trình tiếp nhận bệnh nhân vẫn theo hình thức đến trực tiếp, **chưa có hệ thống đặt lịch hẹn trước**. Thực trạng này dẫn đến những hạn chế sau:

- **Bệnh nhân phải chờ đợi lâu**, không chủ động được thời gian khám, ảnh hưởng đến trải nghiệm và sự hài lòng;

- **Trung tâm chưa dự báo được lưu lượng** bệnh nhân theo ngày / theo ca khám, khó tối ưu hóa phân bổ nhân lực bác sĩ và lễ tân;

- **Năng lực cạnh tranh** của Trung tâm trong bối cảnh chuyển đổi số y tế còn hạn chế so với các cơ sở y tế tư nhân và bệnh viện tuyến trên đã có ứng dụng đặt lịch online;

- **Chưa khai thác được tiềm năng** từ BHYT và các dịch vụ theo yêu cầu — nhiều bệnh nhân có nhu cầu khám theo yêu cầu nhưng không biết được lịch trống của bác sĩ.

### 1.1.3. Bài toán đảm bảo chất lượng phần mềm

Bên cạnh việc xây dựng website, một bài toán đi kèm không kém phần quan trọng là **đảm bảo chất lượng phần mềm** thông qua kiểm thử. Hệ thống thông tin y tế là hệ thống nghiệp vụ phức tạp: phục vụ đồng thời nhiều vai trò người dùng (bệnh nhân, lễ tân, bác sĩ, quản trị), có nhiều luồng nghiệp vụ liên quan tới nhau (đặt lịch — duyệt lịch — check-in — chẩn đoán — kê đơn — hỏi đáp), và phải đáp ứng các yêu cầu khắt khe về bảo mật, audit, sao lưu dữ liệu y tế. Việc kiểm thử thủ công cho hệ thống quy mô như vậy tốn rất nhiều nhân lực, dễ bỏ sót các lỗi hồi quy và khó đảm bảo tính nhất quán giữa các phiên bản.

Từ thực tiễn nêu trên, đề tài *"Nghiên cứu xây dựng website y tế và triển khai kiểm thử tự động bằng Playwright"* được lựa chọn nhằm giải quyết đồng thời hai bài toán: (1) số hóa quy trình tiếp nhận bệnh nhân ngoại trú; (2) xây dựng bộ kịch bản kiểm thử tự động đảm bảo chất lượng phần mềm trong các vòng cập nhật, bảo trì sau này.

## 1.2. Phát biểu bài toán

### 1.2.1. Mục tiêu nghiên cứu

Đề tài đặt ra hai mục tiêu song song:

- **Mục tiêu 1 — Xây dựng website y tế:** Phát triển website cho Trung tâm Y tế phường Kinh Môn trên nền tảng *ASP.NET Core 8 + Entity Framework Core 8 + SQL Server*, đáp ứng các nghiệp vụ chính: đặt lịch khám trực tuyến, duyệt và xác nhận lịch, sinh mã booking, check-in tại quầy, tạo hồ sơ khám và đơn thuốc, hỏi đáp giữa bệnh nhân và bác sĩ; áp dụng đầy đủ các lớp bảo mật theo *OWASP Top 10*;

- **Mục tiêu 2 — Triển khai kiểm thử tự động:** Thiết kế và triển khai bộ kịch bản kiểm thử tự động bằng *Playwright + TypeScript* theo mô hình *Page Object — Fixture*, đồng thời kết hợp với kiểm thử đơn vị (*xUnit + EF Core InMemory*) cho tầng nghiệp vụ và kiểm thử thủ công bằng Postman cho các luồng cần đánh giá định tính.

### 1.2.2. Đầu vào — đầu ra của hệ thống

**Đầu vào của hệ thống:**

- Thông tin đặt lịch của bệnh nhân: họ tên, số điện thoại, CCCD/BHYT, chuyên khoa cần khám, ngày — ca khám mong muốn, lý do khám;
- Thao tác duyệt lịch của lễ tân: xác nhận / từ chối / phân bác sĩ / check-in;
- Kết quả chẩn đoán của bác sĩ: triệu chứng, chẩn đoán, phác đồ điều trị, đơn thuốc;
- Cấu hình hệ thống của quản trị viên: tài khoản, danh mục bác sĩ — chuyên khoa, lịch trực, quota khám.

**Đầu ra của hệ thống:**

- Mã booking duy nhất (định dạng `KMyymmddS|C......`) cho mỗi lịch hẹn xác nhận;
- Hồ sơ khám điện tử (`record_no`) lưu trữ đầy đủ thông tin chẩn đoán + đơn thuốc, có audit log đi kèm;
- Báo cáo lưu lượng bệnh nhân theo ngày / khoa / ca khám phục vụ ban quản trị;
- Bộ kịch bản kiểm thử tự động (Playwright HTML Report) đánh giá chất lượng phần mềm sau mỗi commit.

### 1.2.3. Bài toán đặc thù trong y tế

Khác với các website thương mại điện tử thông thường, website y tế có những đặc thù riêng cần giải quyết:

- **Tính nhạy cảm của dữ liệu y tế:** thông tin sức khỏe được pháp luật bảo vệ nghiêm ngặt — không được lưu mật khẩu plain-text, phải có audit log đầy đủ, kiểm soát truy cập theo vai trò (RBAC) để ngăn bác sĩ A xem hồ sơ bệnh nhân của bác sĩ B (cross-doctor IDOR);

- **Workflow nhiều bước có ràng buộc trạng thái:** một lịch hẹn đi qua chuỗi trạng thái *pending → confirmed → checked-in → completed* hoặc *rejected*; mỗi chuyển trạng thái phải được phép theo whitelist, kèm điều kiện (ví dụ: rejected phải có lý do, completed chỉ chuyển từ checked-in);

- **Cạnh tranh tài nguyên:** nhiều bệnh nhân cùng đặt lịch trong một khoa — ngày — ca, hệ thống phải kiểm soát quota và xử lý race condition khi sinh mã hồ sơ;

- **Đa kích thước màn hình:** bệnh nhân truy cập từ điện thoại (75% lượt truy cập) trong khi nhân viên y tế dùng máy tính desktop — giao diện phải responsive trên cả hai loại thiết bị.

## 1.3. Nội dung và phạm vi của đề tài

### 1.3.1. Đối tượng nghiên cứu

- **Quy trình nghiệp vụ ngoại trú** của Trung tâm Y tế cấp phường, gồm tương tác giữa bốn vai trò: Bệnh nhân — Lễ tân — Bác sĩ — Quản trị viên;

- **Công nghệ phát triển website** dựa trên ASP.NET Core 8 + Entity Framework Core 8 + SQL Server theo kiến trúc 3 tầng;

- **Công nghệ kiểm thử tự động** dựa trên Playwright + TypeScript theo mô hình Page Object Model.

### 1.3.2. Phạm vi nghiên cứu

**Phạm vi đưa vào đồ án:**

- Các nghiệp vụ ngoại trú phổ biến: đặt lịch khám, duyệt lịch, check-in, chẩn đoán, kê đơn, hỏi đáp;
- Bốn vai trò người dùng đầy đủ: Public/Member, Lễ tân, Bác sĩ, Quản trị viên;
- Kiểm thử đa thiết bị: desktop 1366×768, 1920×1080, mobile iPhone X 375×812, iPhone SE 320×568;
- Triển khai public qua Cloudflare Tunnel với tên miền `https://ttytkm.jamesnguyen28.io.vn`.

**Phạm vi không đưa vào đồ án:**

- Thanh toán BHYT phức tạp với cổng dịch vụ công (cần tích hợp BHYT API riêng);
- Dược nội trú và quản lý kho thuốc (chỉ kê đơn, không kiểm soát tồn kho);
- Chẩn đoán hình ảnh (X-quang, siêu âm) — cần thiết bị phần cứng riêng;
- Gửi tin nhắn SMS xác nhận lịch hẹn tới SĐT thật — cần đăng ký SMS Brand Name của nhà cung cấp;
- Ứng dụng mobile native (iOS/Android) — đề tài tập trung web responsive.

### 1.3.3. Phương pháp tiếp cận

Đồ án áp dụng phương pháp phát triển phần mềm **iterative-incremental** kết hợp **test-driven mindset**:

- Mỗi phiên làm việc tập trung hoàn thiện một module nghiệp vụ (Đặt lịch → Duyệt → Phân bác sĩ → Check-in → Chẩn đoán → Q&A);
- Mỗi module được kèm theo test Playwright tương ứng trước khi chuyển sang module kế tiếp;
- Bộ test xUnit cho tầng business logic được viết song song với mã nghiệp vụ, đảm bảo tỷ lệ phủ coverage cao trên các service quan trọng (`AppointmentService`, `MedicalRecordService`, `UserService`).

### 1.3.4. Cấu trúc đồ án

Ngoài phần *Lời mở đầu* và *Kết luận*, đồ án được chia thành bốn chương:

- **Chương 1.** Giới thiệu đề tài — đặt vấn đề, phát biểu bài toán, phạm vi, công nghệ sử dụng;
- **Chương 2.** Khảo sát và phân tích thiết kế — khảo sát hiện trạng, yêu cầu chức năng/phi chức năng, biểu đồ use case, biểu đồ hoạt động, đặc tả CSDL, ERD;
- **Chương 3.** Cài đặt website — chi tiết các trang chính của hệ thống với giao diện và đặc tả chức năng;
- **Chương 4.** Kiểm thử các chức năng của website bằng Playwright — phân tích ca kiểm thử, đánh giá kết quả.

## 1.4. Công nghệ sử dụng

Trên cơ sở phân tích bài toán và đặc điểm nghiệp vụ y tế, đồ án sử dụng nhóm công nghệ và công cụ sau đây — đúng theo định hướng đề cương đã được phê duyệt.

### 1.4.1. Ngôn ngữ lập trình

- **C#** — ngôn ngữ chính phát triển backend website. C# là ngôn ngữ hướng đối tượng do Microsoft phát triển, chạy trên nền tảng .NET Core 8 đa nền tảng (Windows / Linux / macOS), có hệ thống kiểu mạnh giúp phát hiện sớm lỗi tại thời điểm biên dịch;

- **TypeScript** — ngôn ngữ chính viết kịch bản kiểm thử Playwright. TypeScript là superset của JavaScript bổ sung kiểm tra kiểu tĩnh, IntelliSense, hỗ trợ refactor an toàn — phù hợp cho các bộ test có quy mô lớn cần dễ bảo trì.

### 1.4.2. Công nghệ frontend

- **Razor View Engine** — bộ render template đi kèm ASP.NET Core MVC. Cú pháp `@` của Razor cho phép trộn mã C# vào HTML một cách tự nhiên; trong đồ án, các tính năng tag helper, partial view và layout chung được sử dụng để giảm trùng lặp giữa nhiều trang;

- **HTML 5** + **CSS 3** — bộ chuẩn nền tảng cho ứng dụng web hiện nay. Đồ án sử dụng các thẻ semantic (`<section>`, `<nav>`, `<article>`) nhằm hỗ trợ công cụ trợ giúp truy cập đọc đúng nội dung; layout chính dựa trên Flexbox và CSS Grid để thuận tiện điều chỉnh theo từng kích thước màn hình;

- **Bootstrap 5** — thư viện CSS được nhiều dự án website công lập tại Việt Nam sử dụng. Bootstrap được chọn thay vì viết CSS từ đầu nhờ hệ grid 12 cột cùng các thành phần dựng sẵn (navbar, modal, form-control, card), giúp dựng giao diện nhanh và bao phủ đầy đủ các viewport cần kiểm thử (320 → 1920 px). Phần CSS riêng phục vụ nhận diện thương hiệu của Trung tâm được bổ sung trong tệp `site.css`;

- **JavaScript (ES2022)** — đảm nhận các tương tác phía client như xác thực form, gọi AJAX cho bộ đếm khách trực tuyến, đóng/mở modal xác nhận hủy lịch và lazy-load ảnh tin tức.

### 1.4.3. Công nghệ backend

- **ASP.NET Core MVC (.NET 8)** — phiên bản LTS mới nhất của Microsoft tại thời điểm bắt đầu đồ án (tháng 02/2026), hỗ trợ chạy đa nền tảng Windows / Linux. Đồ án khai thác cơ chế Dependency Injection có sẵn để inject các Service vào Controller, sử dụng Middleware pipeline cho các yêu cầu chéo (logging, exception, authorization), đồng thời bật chế độ Razor runtime compilation để chỉnh sửa tệp `.cshtml` không cần build lại;

- **Mô hình kiến trúc 3 tầng** — solution được tách thành ba project tương ứng ba lớp: `WebsiteCore.Data` chứa entity và `DbContext`; `WebsiteCore.Business` chứa các Service và view model; `WebsiteCore.Web` là lớp giao diện gồm controller và Razor view. Cách phân tách này thuận tiện cho việc viết unit test cho tầng nghiệp vụ mà không cần khởi động web server, đồng thời mở khả năng tái sử dụng tầng Data và Business cho các kênh giao tiếp khác (ứng dụng mobile, public API…) trong tương lai.

### 1.4.4. Cơ sở dữ liệu và ORM

- **SQL Server (Express Edition)** — hệ quản trị cơ sở dữ liệu quan hệ của Microsoft. Lý do chọn SQL Server thay vì MySQL hay PostgreSQL gồm ba điểm: (1) phần lớn cơ sở y tế công lập tại Việt Nam đang vận hành trên SQL Server hoặc Oracle, do đó deploy thực tế ít gặp rủi ro tương thích; (2) khả năng tích hợp với .NET tốt nhất trong các hệ quản trị phổ biến; (3) bản Express miễn phí và đi kèm SQL Server Management Studio cho công tác quản trị giao diện đồ họa. Toàn bộ schema sử dụng kiểu `NVARCHAR` Unicode để bảo đảm lưu trữ tiếng Việt có dấu chính xác;

- **Entity Framework Core (EF Core)** — thư viện ORM chính thức của Microsoft cho nền tảng .NET. Đồ án áp dụng phương pháp **code-first**: định nghĩa các lớp entity bằng C# trước, sau đó dùng lệnh `dotnet ef migrations add` để sinh tệp migration tương ứng và `dotnet ef database update` để áp dụng schema vào CSDL. EF Core hỗ trợ truy vấn LINQ với kiểm tra kiểu chặt chẽ (sai tên cột báo lỗi tại bước biên dịch thay vì lúc chạy), kèm các tính năng change tracking, transaction và lazy loading tích hợp sẵn.

### 1.4.5. Công nghệ kiểm thử — Playwright

Qua khảo sát ba công cụ kiểm thử tự động phổ biến nhất hiện nay (Selenium, Cypress, Playwright), **Playwright** được lựa chọn cho đồ án dựa trên các tiêu chí kỹ thuật sau: hỗ trợ đồng thời ba nhân trình duyệt thông dụng (Chromium, Firefox, WebKit) chỉ với một bộ kịch bản; tích hợp sẵn TypeScript với hệ kiểu tĩnh chặt chẽ; có cơ chế đợi phần tử tự động giúp giảm các lỗi không ổn định do thời gian.

Playwright là sản phẩm mã nguồn mở do nhóm phát triển trình duyệt Edge của Microsoft công bố ra cộng đồng đầu năm 2020 dưới giấy phép Apache 2.0. Điểm khác biệt quan trọng so với Selenium nằm ở giao thức truyền tin: Selenium giao tiếp gián tiếp qua chuẩn WebDriver, trong khi Playwright kết nối thẳng tới trình duyệt qua DevTools Protocol. Cách tiếp cận này giúp giảm độ trễ giữa lệnh kịch bản và thao tác thực tế trên trang. Các đặc điểm chính được khai thác trong đồ án gồm:

- **Auto-wait:** mọi action `locator.click()`, `locator.fill()` đều chờ phần tử ở trạng thái `stable`, `visible`, `enabled` trước khi thực thi, không cần khai báo `setTimeout` hay vòng lặp polling thủ công như Selenium;

- **Đa trình duyệt và mô phỏng thiết bị:** chỉ với một tệp `playwright.config.ts`, cùng một bộ test có thể chạy lần lượt trên Chromium, Firefox, WebKit; đồng thời mô phỏng các viewport iPhone X, iPhone SE, iPad ngay trong test runner;

- **Trace Viewer:** mỗi test fail tự sinh tệp `trace.zip` chứa DOM snapshot, ảnh chụp từng action và lưu lượng mạng — hỗ trợ tốt cho công tác debug;

- **HTML Reporter** tích hợp sẵn, không cần cài đặt thêm thư viện báo cáo bên ngoài.

Đồ án áp dụng mô hình **Page Object Model (POM)** kết hợp **Fixture pattern** để tổ chức bộ test gồm 269 ca, hoàn thành trong khoảng 12 phút trên máy phát triển 8 lõi. Chi tiết về ca kiểm thử và kết quả được trình bày ở Chương 4.

### 1.4.6. Công cụ hỗ trợ phát triển

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
| 8 | Cloudflare Tunnel | cloudflared 2024.x | Công cụ tạo đường hầm an toàn từ máy phát triển ra Internet, cấp tên miền và TLS 1.3 miễn phí — phục vụ deploy public website thử nghiệm |
| 9 | Microsoft Excel | 365 | Quản lý ca kiểm thử (test case management) theo chuẩn ISTQB — file `TestCases_TTYTKM.xlsx` đính kèm |

\newpage
