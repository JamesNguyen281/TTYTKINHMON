# CHƯƠNG 2. PHÂN TÍCH THIẾT KẾ HỆ THỐNG

## 2.1. Khảo sát hiện trạng

### 2.1.1. Khảo sát thực tế tại Trung tâm Y tế phường Kinh Môn

Đồ án tiến hành khảo sát trực tiếp tại Trung tâm Y tế phường Kinh Môn (Số 294 đường Trần Hưng Đạo, phường Kinh Môn, TP Hải Phòng) thông qua phỏng vấn cán bộ lễ tân, bác sĩ và quan sát quy trình tiếp đón bệnh nhân. Kết quả khảo sát được tổng hợp trong bảng sau.

Bảng 2.1. Hiện trạng quy trình ngoại trú tại TTYT phường Kinh Môn

| STT | Khía cạnh | Hiện trạng (trước cải tiến) |
|:--:|:--|:--|
| 1 | Hình thức tiếp nhận | 100% bệnh nhân đến trực tiếp, không có đặt lịch trước |
| 2 | Lượt khám trung bình | Dưới 100 lượt / ngày, phân bố không đều |
| 3 | Thời gian chờ trung bình | 30 – 60 phút trong giờ cao điểm (8h–10h sáng) |
| 4 | Cơ chế đăng ký | Sổ giấy tại quầy lễ tân, gọi tên theo thứ tự đến trước |
| 5 | Hồ sơ khám | Bệnh án giấy, lưu trong tủ hồ sơ theo mã bệnh nhân |
| 6 | Kê đơn thuốc | Đơn thuốc viết tay, photo lưu lại 1 bản cho cơ sở |
| 7 | Kênh hỏi đáp | Gọi điện hotline 0220.3.822.205 trong giờ hành chính |
| 8 | Phân lịch trực bác sĩ | Trưởng khoa lập tay theo từng tháng, dán bảng tin |
| 9 | Báo cáo lưu lượng | Thống kê thủ công cuối tháng dựa trên sổ sách |

Kết quả khảo sát cho thấy quy trình hiện tại còn nhiều hạn chế: bệnh nhân không chủ động được thời gian, dữ liệu phục vụ dự báo lưu lượng chưa được số hóa, hồ sơ khám lưu giấy gây khó khăn cho tra cứu và kênh hỏi đáp bị giới hạn theo giờ hành chính. Các điểm hạn chế này được xác định làm cơ sở để xây dựng các nhóm chức năng của website.

### 2.1.2. Quan sát các website đặt lịch khám trực tuyến tham khảo

Để tham khảo mô hình tổ chức giao diện và nghiệp vụ, đồ án khảo sát ba cơ sở y tế đại diện cho ba phân khúc khác nhau trong cùng địa bàn Hải Phòng – Hải Dương. Từ năm 2025, phường Kinh Môn thuộc TP Hải Phòng sau khi sáp nhập với tỉnh Hải Dương cũ:

- Bệnh viện Đa khoa Quốc tế Hải Phòng – HIH (`hih.vn`) là bệnh viện tư nhân quốc tế lớn nhất TP Hải Phòng (124 Nguyễn Đức Cảnh, Lê Chân). Đại diện cho mô hình tư nhân hiện đại, đơn vị có hệ thống đặt lịch online riêng tại `register.hih.vn` và chức năng tra cứu kết quả xét nghiệm trực tuyến;

- Bệnh viện Đa khoa Hải Dương (`benhviendakhoahaiduong.vn`) là bệnh viện công lập hạng I tuyến tỉnh trên địa bàn Hải Dương cũ, có hệ thống "Đăng ký khám bệnh trực tuyến" tại `dkkham.benhviendakhoahaiduong.vn`. Đây là cơ sở y tế tuyến trên thường tiếp nhận chuyển tuyến từ TTYT phường Kinh Môn;

- Bệnh viện Hữu nghị Việt Tiệp Hải Phòng (`viettiephospital.vn`) là bệnh viện đa khoa hạng I tuyến TP, đồng thời là cơ sở y tế công lập lớn nhất TP Hải Phòng và vùng Duyên hải Bắc Bộ. Hệ thống đặt lịch khám online (`appointment-booking`, `personal-booking`) phục vụ cả khám BHYT và khám theo yêu cầu, hotline 1900 23 23 61.

Kết quả quan sát được tổng hợp trong bảng sau.

Bảng 2.2. So sánh các website đặt lịch khám tham khảo cùng địa bàn HP — HD

| Tiêu chí | HIH (Đa khoa Quốc tế HP) | BVĐK Hải Dương | BV Việt Tiệp HP |
|:--|:--|:--|:--|
| Mô hình | Tư nhân quốc tế | Công lập tuyến tỉnh hạng I | Công lập tuyến TP hạng I |
| Đặt lịch online | Có (sub-domain `register.hih.vn`) | Có (sub-domain `dkkham…`) | Có (`appointment-booking`) |
| Đặt theo bác sĩ | Có | Theo chuyên khoa | Theo chuyên khoa |
| Đặt theo dịch vụ | Có (đầy đủ gói khám) | Có (gói khám) | Có (khám BHYT + theo yêu cầu) |
| Mã booking | Có (kèm SMS xác nhận) | Có | Có |
| Yêu cầu đăng ký | Khuyến khích | Không bắt buộc | Không bắt buộc |
| Hồ sơ điện tử | Tra cứu kết quả XN online | Đang triển khai | Đang triển khai |
| Q&A bác sĩ | Form contact + hotline | Hotline | Hotline 1900 23 23 61 |
| Mobile responsive | Có (chưa có App) | Có (chưa có App) | Có (chưa có App) |
| Đặc điểm tham chiếu | BV tư nhân quốc tế lớn nhất HP, hệ đặt lịch riêng | Đơn giản, dành cho người dân địa phương HD cũ | Quy mô tuyến TP, đầy đủ BHYT + tự nguyện |

**Nhận xét chung:**

Cả ba cơ sở đều áp dụng cơ chế đặt lịch trước, xác nhận và sinh mã booking. Đây là chuẩn chung của các hệ thống đặt lịch khám hiện nay và tương đồng với mô hình mà đồ án đề xuất cho TTYT phường Kinh Môn. Trong nhóm tham chiếu, HIH có giao diện chuyên nghiệp với hệ thống đặt lịch riêng trên sub-domain `register.hih.vn`, song form đặt lịch yêu cầu nhập nhiều trường nên đồ án rút ra bài học đơn giản hóa luồng đặt lịch xuống còn bốn bước. BVĐK Hải Dương có giao diện đơn giản, ngôn ngữ thuần Việt, phù hợp với đối tượng người dân địa phương; cách trình bày bảng giá dịch vụ và bảng lịch trực bác sĩ theo tuần được tham khảo cho website. BV Việt Tiệp Hải Phòng là cơ sở tuyến TP, quản lý cả khám BHYT và khám theo yêu cầu, cung cấp tham chiếu về cách phân nhóm dịch vụ và sắp xếp danh mục chuyên khoa.

TTYT phường Kinh Môn có đặc thù riêng so với ba cơ sở tham chiếu. Đơn vị tiền thân là Bệnh viện Đa khoa Kinh Môn, một cơ sở y tế công lập hạng II đạt chuẩn quốc gia từng trực thuộc Sở Y tế Hải Dương, có đầy đủ đội ngũ bác sĩ và các khoa phòng chức năng tương đương ba cơ sở nêu trên. Sau khi sáp nhập đơn vị hành chính, đơn vị được tổ chức lại thành TTYT phường với trọng tâm là khám chữa bệnh ngoại trú và y tế cộng đồng. Vì vậy website trong đồ án vẫn đáp ứng các nghiệp vụ chuẩn của bệnh viện hạng II như đặt lịch theo bác sĩ và theo dịch vụ, hồ sơ bệnh án điện tử, quản lý lịch trực, audit log, hỗ trợ BHYT; phạm vi được tinh gọn ở khám ngoại trú và lược bỏ các module nội trú dài ngày, phẫu thuật và hậu phẫu vốn chỉ cần ở tuyến tỉnh hoặc tuyến TP.

Q&A có bác sĩ trả lời được lựa chọn làm tính năng tạo điểm khác biệt cho TTYT cấp phường, tận dụng đội ngũ bác sĩ đa nhiệm của Trung tâm để tăng kết nối với người dân. Bên cạnh đó, hỗ trợ thiết bị di động là yêu cầu bắt buộc; quan sát thực tế cho thấy đa số người dân địa bàn Hải Phòng – Hải Dương truy cập website y tế qua điện thoại Android tầm trung, do đó website được thiết kế responsive đồng thời cho iPhone X (375 px) và iPhone SE (320 px) ngay từ đầu.

Trên cơ sở khảo sát thực tế và quan sát các nền tảng tham khảo, đồ án xác định bảy nhóm nghiệp vụ chính cần triển khai: (1) Đặt lịch khám, (2) Duyệt và xác nhận lịch, (3) Check-in tại cơ sở, (4) Chẩn đoán và kê đơn, (5) Quản lý hồ sơ y tế, (6) Hỏi đáp, (7) Quản trị hệ thống.

## 2.2. Khảo sát yêu cầu

Các yêu cầu của hệ thống được phân thành hai nhóm: yêu cầu chức năng mô tả những gì hệ thống phải làm, và yêu cầu phi chức năng mô tả hệ thống phải hoạt động như thế nào.

### 2.2.1. Yêu cầu chức năng

Yêu cầu chức năng được phân theo bốn vai trò người dùng, phù hợp với mô hình RBAC (Role-Based Access Control) được thiết kế ở mục 2.3.

a) Chức năng cho khách vãng lai

Khách vãng lai là người dùng chưa đăng nhập, có thể truy cập các tính năng cơ bản phục vụ tra cứu và đặt lịch khám:

- Xem trang chủ, danh sách bác sĩ, danh sách chuyên khoa, tin tức, hỏi đáp, liên hệ;
- Đặt lịch khám không cần đăng ký tài khoản (nhập thông tin cá nhân tại form);
- Xem mã đặt lịch và trạng thái sau khi đặt thành công nhờ Session bảo mật;
- Đăng ký tài khoản mới hoặc đăng nhập để truy cập các tính năng nâng cao.

b) Chức năng cho bệnh nhân đã đăng nhập (Member)

Bệnh nhân đã đăng nhập có quyền truy cập đầy đủ các tính năng tự phục vụ trên kênh thành viên:

- Đặt lịch khám trực tuyến với thông tin được lưu sẵn từ hồ sơ;
- Xem danh sách lịch hẹn của mình theo trạng thái: Chờ duyệt, Đã xác nhận, Đã khám, Đã hủy;
- Xem lại lịch sử khám bệnh, hồ sơ khám và đơn thuốc;
- Đặt câu hỏi cho bác sĩ và xem trả lời;
- Cập nhật thông tin cá nhân, đổi mật khẩu.

c) Chức năng cho lễ tân (Reception)

Lễ tân vận hành cổng nội bộ phục vụ tiếp nhận và xác nhận lịch hẹn:

- Xem danh sách lịch hẹn theo trạng thái, sắp xếp theo ngày, ca và bác sĩ;
- Xác nhận hoặc từ chối lịch hẹn (có yêu cầu lý do từ chối);
- Tìm kiếm lịch hẹn theo số điện thoại bệnh nhân;
- Tra cứu lịch theo ngày bất kỳ trong vòng ± 30 ngày;
- Check-in bệnh nhân tại quầy.

d) Chức năng cho bác sĩ (Doctor)

Bác sĩ thao tác trên cổng riêng để khám bệnh và trả lời câu hỏi của người dân:

- Xem danh sách bệnh nhân được phân công khám;
- Lập hồ sơ chẩn đoán và kê đơn thuốc với kiểm soát độ dài chuỗi để chống tấn công DoS;
- Xem hồ sơ khám đã chẩn đoán bởi chính mình;
- Xem lịch trực của mình;
- Trả lời các câu hỏi mà bệnh nhân gửi tới.

e) Chức năng cho quản trị viên (Admin)

Quản trị viên có quyền cao nhất, phụ trách cấu hình toàn bộ hệ thống và phân quyền nhân sự:

- Quản lý toàn bộ tài khoản, phân quyền theo nhóm (Admin / Reception / Doctor / Member);
- Quản lý danh mục: bác sĩ, chuyên khoa, dịch vụ, slot khám, tin tức, hỏi đáp;
- Tự động phân lịch trực hằng tháng cho bác sĩ (manual trigger + cron tự động ngày 28);
- Xem nhật ký kiểm toán (audit log) toàn hệ thống;
- Sao lưu và phục hồi cơ sở dữ liệu;
- Cấu hình thông tin chung: tên đơn vị, logo, địa chỉ, hotline, email, banner.

### 2.2.2. Yêu cầu phi chức năng

**a. Yêu cầu về sản phẩm**

Website cần bảo đảm các đặc trưng chất lượng vận hành trên hạ tầng dự kiến của Trung tâm:

- Tốc độ truy cập trang nhanh, thời gian phản hồi của các thao tác chính dưới 1 giây trong điều kiện mạng nội bộ và dưới 3 giây qua kết nối Internet thông thường;
- Độ tin cậy cao, hệ thống chạy ổn định 24/7, không có lỗi 500 trong các nghiệp vụ chuẩn;
- Bộ nhớ và tài nguyên server được tối ưu, có thể chạy được trên cấu hình thấp (4 GB RAM, 2 vCPU, SSD 40 GB);
- Giao diện thân thiện, sử dụng tiếng Việt làm ngôn ngữ chính, có thể mở rộng song ngữ Việt – Anh nhờ thiết kế cột song song `name_l` và `name_e`.

**b. Yêu cầu về quá trình phát triển**

Quá trình xây dựng hệ thống tuân thủ các chuẩn lập trình và công cụ quản lý phổ biến:

- Tuân thủ chuẩn lập trình ASP.NET Core MVC và mô hình kiến trúc 3 tầng (Web – Business – Data);
- Sử dụng Entity Framework Core với mô hình code-first, có quản lý migrations;
- Mã nguồn được quản lý phiên bản bằng Git, đẩy lên GitHub;
- Sử dụng các công cụ phát triển và đóng gói tiêu chuẩn: Visual Studio 2022, Visual Studio Code, Postman (kiểm thử HTTP endpoint), Docker (đóng gói triển khai).

**c. Yêu cầu bảo mật**

Website lưu trữ thông tin sức khỏe của người dân, là dạng dữ liệu nhạy cảm được pháp luật bảo vệ theo Luật Khám bệnh, chữa bệnh năm 2023. Đồ án đặt ra các yêu cầu bảo mật cụ thể như sau:

- Mật khẩu không lưu dạng plain text mà được băm bằng thuật toán PBKDF2 kết hợp SHA-256 với salt riêng cho từng tài khoản và 600.000 vòng lặp theo khuyến nghị mới nhất của OWASP năm 2023. Cookie phiên có chữ ký số chống giả mạo;
- Cơ sở dữ liệu được sao lưu định kỳ hằng tuần ra thiết bị lưu trữ rời, cho phép khôi phục lại trong vòng vài giờ khi xảy ra sự cố phần cứng;
- Tệp backup hằng tuần được kết hợp với cơ chế EF Core Migration để tái tạo schema từ đầu khi cần, giữ thời gian gián đoạn dịch vụ ở mức tối thiểu;
- Nhật ký kiểm toán (audit log) ghi nhận mỗi thay đổi trạng thái lịch hẹn, hồ sơ khám hay đơn thuốc vào bảng `audit_system` gồm giá trị cũ, giá trị mới, người thực hiện và lý do nếu có. Cơ chế này hỗ trợ truy vết khi xảy ra tranh chấp giữa bệnh nhân và Trung tâm;
- Hệ thống tuân thủ OWASP Top 10, áp dụng đầy đủ các biện pháp phòng vệ chống các lỗ hổng phổ biến: chống tấn công CSRF qua AntiForgeryToken, sanitize input chống XSS, tham số hóa truy vấn chống SQL Injection, kiểm soát truy cập đối tượng chống IDOR, khóa tài khoản sau 5 lần đăng nhập sai liên tiếp.

## 2.3. Thiết kế biểu đồ Use Case

### 2.3.1. Biểu đồ use case tổng quát

Hệ thống có các tác nhân là Bệnh nhân, Lễ tân, Bác sĩ và Quản trị viên. Biểu đồ use case tổng quát được trình bày ở Hình 2.1.

![Hình 2.1. Biểu đồ use case tổng quát của hệ thống](images/hinh-2-1.png){width=25cm}

Chức năng của từng tác nhân được liệt kê dưới đây.

a. Bệnh nhân:

- Đăng ký tài khoản
- Đăng nhập
- Đặt lịch khám
- Xem lịch của tôi
- Đặt câu hỏi Q&A
- Xem lịch sử khám
- Cập nhật hồ sơ cá nhân

b. Lễ tân:

- Xem danh sách lịch hẹn
- Duyệt và xác nhận lịch
- Từ chối lịch
- Check-in bệnh nhân
- Phân phòng khám và bác sĩ
- Quản lý quota khám
- Xem lịch trực bác sĩ

c. Bác sĩ:

- Xem bệnh nhân hôm nay
- Tạo hồ sơ và chẩn đoán
- Kê đơn thuốc
- Hoàn tất khám
- Trả lời câu hỏi Q&A

d. Quản trị viên:

- Cấu hình site
- Quản lý tin tức và chuyên khoa
- Quản lý bác sĩ và tài khoản người dùng
- Quản lý tài liệu
- Xem nhật ký kiểm toán
- Buộc đổi mật khẩu

**a) Tác nhân Bệnh nhân**

![Hình 2.3. Biểu đồ use case của tác nhân Bệnh nhân](images/hinh-2-3.png){width=15cm}

**b) Tác nhân Lễ tân**

![Hình 2.4. Biểu đồ use case của tác nhân Lễ tân](images/hinh-2-4.png){width=15cm}

**c) Tác nhân Bác sĩ**

![Hình 2.5. Biểu đồ use case của tác nhân Bác sĩ](images/hinh-2-5.png){width=15cm}

**d) Tác nhân Quản trị viên**

![Hình 2.6. Biểu đồ use case của tác nhân Quản trị viên](images/hinh-2-6.png){width=15cm}

### 2.3.2. Use case Đặt lịch khám

Bảng 2.3. Mô tả use case Đặt lịch khám

| Trường | Nội dung |
|:--|:--|
| Tên use case | Đặt lịch khám |
| Tác nhân | Bệnh nhân (Member hoặc Khách vãng lai) |
| Mục đích | Cho phép bệnh nhân đặt lịch khám trực tuyến tại Khoa Khám bệnh mà không cần đến trực tiếp cơ sở |
| Mô tả | Cho phép bệnh nhân đặt lịch khám tại Khoa Khám bệnh, đầu mối tiếp nhận bệnh nhân ngoại trú. Bệnh nhân không chọn khoa, phòng hoặc bác sĩ; lễ tân tiếp nhận triệu chứng và phân vào một trong tám phòng khám chuyên môn (Nội, Ngoại, Tiểu đường, Sản, Truyền nhiễm, Nhi, Đông y, Răng Hàm Mặt) |
| Tiền điều kiện | Khoa Khám bệnh đang hoạt động trong site; có lịch trực bác sĩ tại các phòng khám chuyên môn |
| Luồng sự kiện chính | 1. Bệnh nhân chọn menu [Đặt lịch khám]<br>2. Hệ thống hiển thị form: họ tên, SĐT, email, ngày khám, ca khám (sáng/chiều), triệu chứng/lý do khám<br>3. Bệnh nhân điền thông tin và submit<br>4. Hệ thống xác thực CSRF token, kiểm tra dữ liệu hợp lệ (ngày trong [hôm nay, hôm nay + 14], không trùng buổi)<br>5. Hệ thống tự động gán `DepartmentId = Khoa Khám bệnh`, để trống `ClinicRoomId` (lễ tân phân sau)<br>6. Hệ thống lưu lịch ở trạng thái *Pending*<br>7. Hệ thống chuyển bệnh nhân tới trang xác nhận, hiển thị mã đặt lịch tạm |
| Luồng sự kiện thay thế | – Trùng buổi (cùng tài khoản, cùng ngày, cùng ca): hệ thống chặn, hiển thị link tới *Lịch của tôi*<br>– Khoa Khám bệnh chưa khởi tạo: trả lỗi yêu cầu liên hệ quản trị viên |
| Hậu điều kiện | Lịch hẹn ở trạng thái *Pending*, chờ lễ tân duyệt và phân phòng |

### 2.3.3. Use case Duyệt và xác nhận lịch hẹn

Bảng 2.4. Mô tả use case Duyệt lịch hẹn

| Trường | Nội dung |
|:--|:--|
| Tên use case | Duyệt và xác nhận lịch hẹn |
| Tác nhân | Lễ tân (Reception) |
| Mục đích | Cho phép lễ tân kiểm tra lịch hẹn do bệnh nhân gửi, phân phòng khám và bác sĩ, xác nhận lịch và cấp mã booking |
| Mô tả | Lễ tân đọc triệu chứng của bệnh nhân, lựa chọn phòng khám chuyên môn và bác sĩ đang trực để gán cho lịch hẹn, sau đó xác nhận và sinh mã booking |
| Tiền điều kiện | Đã đăng nhập, có lịch ở trạng thái *Pending* |
| Luồng sự kiện chính | 1. Lễ tân vào [Cổng Lễ tân → Lịch hẹn → Chờ duyệt]<br>2. Hệ thống liệt kê các lịch sắp xếp theo thời gian gửi<br>3. Lễ tân mở chi tiết một lịch, đọc triệu chứng do bệnh nhân khai báo<br>4. Lễ tân chọn phòng khám chuyên môn phù hợp (Nội/Ngoại/Tiểu đường/Sản/Truyền nhiễm/Nhi/Đông y/RHM) → gán `ClinicRoomId`<br>5. Lễ tân chọn bác sĩ đang trực phòng đó trong ngày-ca tương ứng → gán `DoctorId`<br>6. Lễ tân nhấn [Xác nhận]; hệ thống áp dụng state machine: Pending → Confirmed (whitelist transition)<br>7. Sinh mã booking dạng `KMyymmddS\|C` + 6 ký tự hex (S = sáng, C = chiều)<br>8. Ghi audit log: action = APPOINTMENT_CONFIRMED, before = Pending, after = Confirmed<br>9. Cập nhật quota 2 tầng (khoa + bác sĩ) |
| Luồng sự kiện thay thế | – Từ chối: yêu cầu nhập lý do (`staff_note` >= 5 ký tự)<br>– Trùng lịch quota: hệ thống chặn không cho confirm |
| Hậu điều kiện | Lịch hẹn có mã booking, sẵn sàng cho check-in |

### 2.3.4. Use case Check-in bệnh nhân

Bảng 2.5. Mô tả use case Check-in bệnh nhân

| Trường | Nội dung |
|:--|:--|
| Tên use case | Check-in bệnh nhân |
| Tác nhân | Lễ tân |
| Mục đích | Đánh dấu bệnh nhân đã đến tại cơ sở, đẩy lịch sang trạng thái sẵn sàng khám |
| Mô tả | Lễ tân tra cứu lịch hẹn theo số điện thoại hoặc mã booking, xác nhận bệnh nhân có mặt tại cơ sở, sau đó hệ thống chuyển lịch sang trạng thái CheckedIn để bác sĩ tiếp nhận khám |
| Tiền điều kiện | Lịch hẹn ở trạng thái Confirmed, bệnh nhân có mặt tại cơ sở |
| Luồng sự kiện chính | 1. Lễ tân nhập SĐT bệnh nhân hoặc quét mã booking<br>2. Hệ thống hiển thị lịch hẹn của ngày<br>3. Lễ tân nhấn [Check-in]<br>4. Hệ thống chuyển trạng thái Confirmed → CheckedIn<br>5. Bác sĩ thấy bệnh nhân trong danh sách [Bệnh nhân hôm nay] |
| Luồng sự kiện thay thế | – Không tìm thấy lịch hẹn: hệ thống thông báo và đề nghị lễ tân tạo lịch tại quầy<br>– Lịch không thuộc trạng thái Confirmed: hệ thống chặn thao tác check-in |
| Hậu điều kiện | Lịch hẹn ở trạng thái CheckedIn, hiển thị bên Cổng Bác sĩ |

### 2.3.5. Use case Chẩn đoán và kê đơn

Bảng 2.6. Mô tả use case Chẩn đoán và kê đơn thuốc

| Trường | Nội dung |
|:--|:--|
| Tên use case | Chẩn đoán và kê đơn thuốc |
| Tác nhân | Bác sĩ (Doctor) |
| Mục đích | Cho phép bác sĩ ghi nhận triệu chứng, chẩn đoán bệnh và kê đơn thuốc cho bệnh nhân vào hồ sơ điện tử |
| Mô tả | Bác sĩ tiếp nhận bệnh nhân đã check-in, lập hồ sơ khám bao gồm triệu chứng, chẩn đoán và đơn thuốc; hệ thống cấp số hồ sơ tự động và chuyển lịch hẹn sang trạng thái Done |
| Tiền điều kiện | Có bệnh nhân ở trạng thái CheckedIn được phân công cho bác sĩ |
| Luồng sự kiện chính | 1. Bác sĩ vào [Cổng Bác sĩ → Bệnh nhân hôm nay]<br>2. Chọn bệnh nhân, nhấn [Khám]<br>3. Hệ thống kiểm tra cross-doctor guard (BS A không được khám bệnh nhân của BS B)<br>4. Bác sĩ nhập triệu chứng, chẩn đoán, đơn thuốc<br>5. Hệ thống kiểm soát độ dài: ghi chú ≤ 500 ký tự, tên thuốc ≤ 100, liều dùng ≤ 200<br>6. Hệ thống sinh số hồ sơ tự động (`NextRecordNoAsync` retry 5 lần khi đụng race)<br>7. Lưu hồ sơ, chuyển trạng thái lịch hẹn sang *Done*<br>8. Ghi audit log với userId của bác sĩ |
| Luồng sự kiện thay thế | – Bác sĩ truy cập bệnh nhân không thuộc phân công: hệ thống từ chối và ghi audit<br>– Vượt giới hạn độ dài chuỗi: hệ thống cảnh báo và yêu cầu rút gọn nội dung |
| Hậu điều kiện | Bệnh án điện tử được lưu, sẵn sàng cho bệnh nhân tra cứu |

### 2.3.6. Use case Hỏi đáp Q&A

Bảng 2.7. Mô tả use case Hỏi đáp giữa bệnh nhân và bác sĩ

| Trường | Nội dung |
|:--|:--|
| Tên use case | Hỏi đáp giữa bệnh nhân và bác sĩ |
| Tác nhân | Bệnh nhân (đặt câu hỏi), Bác sĩ (trả lời), Quản trị viên (kiểm duyệt) |
| Mục đích | Cho phép bệnh nhân đặt câu hỏi cho bác sĩ chuyên khoa của Trung tâm và nhận trả lời công khai sau khi quản trị viên duyệt nội dung |
| Mô tả | Bệnh nhân gửi câu hỏi kèm chuyên khoa hoặc bác sĩ cần hỏi; quản trị viên duyệt nội dung tránh spam; bác sĩ chuyên khoa thấy câu hỏi đã duyệt và viết trả lời; toàn bộ Q&A được hiển thị công khai |
| Tiền điều kiện | Bệnh nhân đã đăng nhập tài khoản |
| Luồng sự kiện chính | 1. Bệnh nhân chọn [Hỏi đáp → Đặt câu hỏi mới]<br>2. Nhập tiêu đề, nội dung; chọn chuyên khoa hoặc bác sĩ cụ thể<br>3. Hệ thống lưu câu hỏi ở trạng thái *Pending* (chờ duyệt)<br>4. Quản trị viên duyệt câu hỏi, chuyển sang *Visible*<br>5. Bác sĩ xem các câu hỏi thuộc chuyên khoa của mình, nhập trả lời<br>6. Hệ thống hiển thị Q&A công khai trên trang *Hỏi đáp* |
| Luồng sự kiện thay thế | – Quản trị từ chối câu hỏi (vi phạm nội quy): xóa mềm, ghi audit |
| Hậu điều kiện | Câu hỏi được hiển thị công khai và có câu trả lời |

### 2.3.7. Use case Quản lý người dùng

Bảng 2.8. Mô tả use case Quản lý người dùng

| Trường | Nội dung |
|:--|:--|
| Tên use case | Quản lý người dùng |
| Tác nhân | Quản trị viên |
| Mục đích | Cho phép quản trị viên tạo mới, phân quyền, khóa hoặc reset mật khẩu cho các tài khoản nhân sự và bệnh nhân của hệ thống |
| Mô tả | Quản trị viên truy cập trang quản lý tài khoản trong AdminCP để thực hiện các thao tác CRUD và phân nhóm quyền; mọi thao tác đều được ghi nhận vào nhật ký kiểm toán |
| Tiền điều kiện | Quản trị viên đã đăng nhập với quyền Admin |
| Luồng sự kiện chính | 1. Admin vào [AdminCP → Quản lý tài khoản]<br>2. Hệ thống liệt kê tài khoản theo nhóm quyền<br>3. Admin có thể: tạo mới, gán nhóm quyền, khóa/mở khóa, reset mật khẩu<br>4. Mọi thao tác đều ghi audit log với mã hành vi tương ứng |
| Luồng sự kiện thay thế | – Trùng tên đăng nhập khi tạo mới: hệ thống chặn và yêu cầu nhập lại<br>– Reset mật khẩu: hệ thống bật cờ `must_change_password = 1` để buộc người dùng đổi mật khẩu ở lần đăng nhập kế tiếp |
| Hậu điều kiện | Tài khoản được cập nhật, người dùng có thể đăng nhập lại với quyền mới |

## 2.4. Biểu đồ hoạt động

### 2.4.1. Biểu đồ hoạt động Đăng ký tài khoản

![Hình 2.7. Biểu đồ hoạt động Đăng ký tài khoản](images/hinh-2-7.png){width=16cm}

**Mô tả luồng:**

- Khách vãng lai vào trang */dang-ky*, điền họ tên, số điện thoại (10 chữ số bắt đầu bằng `0` hoặc `+84`), email, mật khẩu (≥ 8 ký tự, có chữ và số);
- Hệ thống kiểm tra trùng SĐT hoặc email trong bảng `customer`;
- Nếu hợp lệ, mật khẩu được mã hóa bằng PBKDF2-SHA256 600.000 vòng và lưu vào `customer.password_hash` cùng salt riêng;
- Nếu không hợp lệ, hệ thống hiển thị thông báo lỗi tương ứng (SĐT đã tồn tại, email sai định dạng, mật khẩu yếu).

### 2.4.2. Biểu đồ hoạt động Đăng nhập

![Hình 2.8. Biểu đồ hoạt động Đăng nhập](images/hinh-2-8.png){width=16cm}

**Mô tả luồng:**

- Người dùng nhập SĐT và mật khẩu;
- Hệ thống kiểm tra `failed_login_count`. Nếu ≥ 5 trong 15 phút thì khóa tạm thời 15 phút;
- Lấy `password_hash` và salt từ DB, tính PBKDF2 từ mật khẩu nhập, so sánh hằng thời gian (timing-safe);
- Nếu khớp, hệ thống tạo cookie session ký số, redirect theo `GroupId` (Member → /ho-so, Reception → /le-tan, Doctor → /bac-si-portal, Admin → /AdminCP);
- Nếu sai, tăng `failed_login_count`, hiển thị thông báo, không tiết lộ tài khoản tồn tại hay không.

### 2.4.3. Biểu đồ hoạt động Đặt lịch khám

![Hình 2.9. Biểu đồ hoạt động Đặt lịch khám](images/hinh-2-9.png){width=16cm}

**Mô tả luồng (3 swimlane: Bệnh nhân – Hệ thống – Lễ tân):**

1. Bệnh nhân chọn chuyên khoa, bác sĩ, ngày và ca;
2. Hệ thống truy vấn `doctor_schedule` với điều kiện `valid_from <= ngày chọn <= valid_to`, `is_active = 1`;
3. Hệ thống đếm số lịch đã có cho slot và so sánh với `quota` của lịch trực;
4. Bệnh nhân điền thông tin liên hệ và lý do khám;
5. Hệ thống validate CSRF, kiểm tra anti-spam (số lần đặt trong 1 giờ ≤ 5), lưu `appointment` ở trạng thái Pending;
6. Hệ thống gửi thông báo realtime cho lễ tân qua banner trên Cổng Lễ tân;
7. Lễ tân duyệt: Confirmed (sinh mã) hoặc Rejected (yêu cầu lý do).

### 2.4.4. Biểu đồ hoạt động Check-in và Khám bệnh

![Hình 2.10. Biểu đồ hoạt động Check-in và Khám bệnh](images/hinh-2-10.png){width=16cm}

**Mô tả luồng:**

- Lễ tân tra cứu lịch hẹn, thực hiện check-in, lịch chuyển sang trạng thái CheckedIn;
- Bác sĩ thấy bệnh nhân trong [Bệnh nhân hôm nay] và mở chẩn đoán;
- Bác sĩ điền hồ sơ, hệ thống áp `SafeTrim` cho các trường text;
- Hồ sơ `medical_record` được lưu với số hồ sơ tự sinh, lịch hẹn chuyển sang Done;
- Bệnh nhân tra cứu được hồ sơ trong [Lịch sử khám] khi đăng nhập tài khoản.

### 2.4.5. Biểu đồ hoạt động Hỏi đáp với bác sĩ

![Hình 2.11. Biểu đồ hoạt động Hỏi đáp](images/hinh-2-11.png){width=16cm}

**Mô tả luồng:**

- Bệnh nhân đăng nhập, chọn [Hỏi đáp → Đặt câu hỏi];
- Câu hỏi được lưu ở trạng thái Pending, ẩn với người dùng khác;
- Quản trị viên duyệt nội dung (chống spam, nội dung không phù hợp) và chuyển sang Visible;
- Bác sĩ chuyên khoa thấy câu hỏi và viết trả lời;
- Sau khi có trả lời, hệ thống gửi email thông báo cho người hỏi nếu có email.

## 2.5. Biểu đồ tuần tự

Trong khi biểu đồ hoạt động ở mục 2.4 mô tả thứ tự các bước nghiệp vụ, biểu đồ tuần tự thể hiện tương tác giữa các đối tượng trong hệ thống thông qua các thông điệp được trao đổi. Năm biểu đồ tuần tự dưới đây minh hoạ luồng dữ liệu giữa controller, service và cơ sở dữ liệu cho năm nghiệp vụ chính của hệ thống.

### 2.5.1. Biểu đồ tuần tự Đăng nhập

![Hình 2.12. Biểu đồ tuần tự nghiệp vụ Đăng nhập](images/hinh-2-12.png){width=16cm}

Luồng đăng nhập đi qua tầng controller xác thực anti-CSRF, tầng service xác minh mật khẩu bằng PBKDF2-SHA256 và tầng cơ sở dữ liệu cập nhật cờ phiên. Sau khi xác thực thành công, hệ thống điều hướng người dùng tới giao diện phù hợp với vai trò.

### 2.5.2. Biểu đồ tuần tự Đặt lịch khám

![Hình 2.13. Biểu đồ tuần tự nghiệp vụ Đặt lịch khám](images/hinh-2-13.png){width=16cm}

Yêu cầu đặt lịch của bệnh nhân được kiểm tra trùng buổi và quota hai tầng (khoa, bác sĩ) ở tầng service trước khi lưu vào cơ sở dữ liệu ở trạng thái Pending. Mọi thao tác đều được ghi vào nhật ký kiểm toán.

### 2.5.3. Biểu đồ tuần tự Duyệt và xác nhận lịch hẹn

![Hình 2.14. Biểu đồ tuần tự nghiệp vụ Duyệt và xác nhận lịch hẹn](images/hinh-2-14.png){width=16cm}

Lễ tân chọn phòng khám chuyên môn, hệ thống lọc danh sách bác sĩ đang trực phòng đó và áp dụng máy trạng thái whitelist khi chuyển lịch từ Pending sang Confirmed. Mã booking dạng `KMyymmdd-XXXXXX` được sinh và quota được cập nhật ngay sau khi xác nhận.

### 2.5.4. Biểu đồ tuần tự Chẩn đoán và kê đơn thuốc

![Hình 2.15. Biểu đồ tuần tự nghiệp vụ Chẩn đoán và kê đơn thuốc](images/hinh-2-15.png){width=16cm}

Bác sĩ chỉ truy cập được hồ sơ của bệnh nhân được phân công nhờ cơ chế cross-doctor guard. Số hồ sơ được cấp tự động với cơ chế retry chống xung đột khi nhiều bác sĩ ghi đồng thời, sau đó lịch hẹn chuyển sang trạng thái Done.

### 2.5.5. Biểu đồ tuần tự Hỏi đáp Q&A

![Hình 2.16. Biểu đồ tuần tự nghiệp vụ Hỏi đáp giữa bệnh nhân và bác sĩ](images/hinh-2-16.png){width=16cm}

Nghiệp vụ Q&A có ba tác nhân tham gia: bệnh nhân đặt câu hỏi, quản trị viên duyệt nội dung, bác sĩ trả lời. Câu hỏi đi qua ba trạng thái Pending, Visible và Answered, mỗi bước đều được ghi nhật ký kiểm toán.

## 2.6. Đặc tả cơ sở dữ liệu

Hệ thống sử dụng SQL Server, schema `ttytlp` được sinh tự động từ Entity Framework Core 8 với mô hình code-first. Toàn bộ cơ sở dữ liệu gồm **25 bảng** chia thành 6 nhóm chức năng. Bảng 2.9 mô tả cấu trúc các bảng quan trọng nhất.

### 2.6.1. Bảng `customer` — Hồ sơ bệnh nhân

Bảng 2.9. Cấu trúc bảng `customer`

| STT | Tên trường | Kiểu dữ liệu | Mô tả |
|:--:|:--|:--|:--|
| 1 | Id | UNIQUEIDENTIFIER | Khóa chính, GUID |
| 2 | SiteId | UNIQUEIDENTIFIER | FK – cơ sở y tế (multi-tenant scoping) |
| 3 | Phone | NVARCHAR(20) | SĐT, unique trong site |
| 4 | Email | NVARCHAR(100) | Email |
| 5 | FullName | NVARCHAR(150) | Họ và tên đầy đủ |
| 6 | DateOfBirth | DATE | Ngày sinh |
| 7 | Gender | TINYINT | 0 = Nam, 1 = Nữ, 2 = Khác |
| 8 | PasswordHash | NVARCHAR(500) | PBKDF2-SHA256 hash |
| 9 | PasswordSalt | NVARCHAR(200) | Salt riêng cho từng tài khoản |
| 10 | FailedLoginCount | INT | Số lần đăng nhập sai liên tiếp |
| 11 | LockedUntil | DATETIME2 | Thời điểm hết khóa nếu bị lockout |
| 12 | CreatedAt | DATETIME2 | Thời điểm tạo |

### 2.6.2. Bảng `appointment` — Lịch hẹn khám

Bảng 2.10. Cấu trúc bảng `appointment`

| STT | Tên trường | Kiểu dữ liệu | Mô tả |
|:--:|:--|:--|:--|
| 1 | Id | UNIQUEIDENTIFIER | Khóa chính |
| 2 | SiteId | UNIQUEIDENTIFIER | FK – cơ sở y tế |
| 3 | CustomerId | UNIQUEIDENTIFIER | FK – bệnh nhân (NULL nếu khách vãng lai) |
| 4 | DoctorId | UNIQUEIDENTIFIER | FK – bác sĩ |
| 5 | DepartmentId | UNIQUEIDENTIFIER | FK – chuyên khoa |
| 6 | ApptDate | DATE | Ngày khám |
| 7 | Session | TINYINT | 0 = Sáng, 1 = Chiều |
| 8 | BookingCode | NVARCHAR(20) | Mã booking dạng `KMyymmddS001` |
| 9 | Status | TINYINT | 0=Pending, 1=Confirmed, 2=CheckedIn, 3=Done, 4=Cancelled, 5=Rejected |
| 10 | StaffNote | NVARCHAR(500) | Ghi chú của lễ tân (lý do từ chối...) |
| 11 | ContactPhone | NVARCHAR(20) | SĐT liên hệ tại lúc đặt |
| 12 | ContactName | NVARCHAR(150) | Họ tên tại lúc đặt |
| 13 | Reason | NVARCHAR(500) | Lý do khám |
| 14 | CreatedAt | DATETIME2 | Thời điểm đặt |
| 15 | ConfirmedAt | DATETIME2 | Thời điểm xác nhận |
| 16 | DoneAt | DATETIME2 | Thời điểm khám xong |

### 2.6.3. Bảng `medical_record` — Hồ sơ khám

Bảng 2.11. Cấu trúc bảng `medical_record`

| STT | Tên trường | Kiểu dữ liệu | Mô tả |
|:--:|:--|:--|:--|
| 1 | Id | UNIQUEIDENTIFIER | Khóa chính |
| 2 | SiteId | UNIQUEIDENTIFIER | FK – cơ sở y tế |
| 3 | RecordNo | NVARCHAR(20) | Số hồ sơ tự sinh, unique trong site |
| 4 | AppointmentId | UNIQUEIDENTIFIER | FK – lịch hẹn |
| 5 | DoctorId | UNIQUEIDENTIFIER | FK – bác sĩ khám |
| 6 | Symptoms | NVARCHAR(MAX) | Triệu chứng |
| 7 | Diagnosis | NVARCHAR(MAX) | Chẩn đoán |
| 8 | Prescription | NVARCHAR(MAX) | Đơn thuốc (text) |
| 9 | Note | NVARCHAR(500) | Ghi chú thêm |
| 10 | CreatedAt | DATETIME2 | Thời điểm tạo hồ sơ |

### 2.6.4. Bảng `doctor_schedule` — Lịch trực bác sĩ

Bảng 2.12. Cấu trúc bảng `doctor_schedule`

| STT | Tên trường | Kiểu dữ liệu | Mô tả |
|:--:|:--|:--|:--|
| 1 | Id | UNIQUEIDENTIFIER | Khóa chính |
| 2 | DoctorId | UNIQUEIDENTIFIER | FK – bác sĩ |
| 3 | DayOfWeek | TINYINT | 1 = Thứ 2 ... 7 = Chủ nhật |
| 4 | Session | TINYINT | 0 = Sáng, 1 = Chiều |
| 5 | Quota | INT | Số bệnh nhân tối đa cho slot |
| 6 | ValidFrom | DATE | Ngày bắt đầu hiệu lực |
| 7 | ValidTo | DATE | Ngày kết thúc hiệu lực |
| 8 | IsActive | BIT | 0 = không hoạt động, 1 = hoạt động |

### 2.6.5. Bảng `audit_system` — Nhật ký kiểm toán

Bảng 2.13. Cấu trúc bảng `audit_system`

| STT | Tên trường | Kiểu dữ liệu | Mô tả |
|:--:|:--|:--|:--|
| 1 | Id | BIGINT | Khóa chính, tự tăng |
| 2 | SiteId | UNIQUEIDENTIFIER | FK – cơ sở y tế |
| 3 | UserId | UNIQUEIDENTIFIER | Tài khoản gây ra hành vi (NULL nếu hệ thống) |
| 4 | Action | NVARCHAR(50) | Mã hành vi (vd `APPOINTMENT_CONFIRMED`) |
| 5 | EntityId | UNIQUEIDENTIFIER | Đối tượng bị tác động |
| 6 | Note | NVARCHAR(1000) | Mô tả thay đổi old → new |
| 7 | IpAddress | NVARCHAR(45) | IP người dùng |
| 8 | CreatedAt | DATETIME2 | Thời điểm sự kiện |

Ngoài năm bảng nêu trên, hệ thống còn 20 bảng phụ trợ khác phục vụ các nghiệp vụ: `site` (cấu hình cơ sở), `system_user` và `system_user_group` (quản lý nhân viên và phân quyền), `doctor`, `department`, `clinic_room` (phòng khám trong Khoa Khám bệnh), `schedule_change_request` (yêu cầu đổi lịch trực của bác sĩ), `service`, `news`, `qna_topic` và `qna_post`, `prescription_drug`, `bank_holiday`, `ip_blacklist`, `customer_address`, `notification`, `attachment`, `setting`, `migration_history`. Chi tiết đầy đủ được mô tả trong sơ đồ ERD ở mục 2.7.

## 2.7. Sơ đồ quan hệ thực thể (ERD)

![Hình 2.17. Sơ đồ quan hệ thực thể (ERD) của hệ thống TTYT phường Kinh Môn](images/hinh-2-17.png){width=16cm}

*(xem ảnh đính kèm — file `docs/diagrams/erd_full.png`)*

Sơ đồ ERD thể hiện 25 thực thể cùng các quan hệ một-nhiều và nhiều-nhiều giữa chúng. Một số quan hệ chính được liệt kê dưới đây:

- `customer` 1 — N `appointment`: một bệnh nhân có thể có nhiều lịch hẹn (lịch sử khám);
- `appointment` 1 — 1 `medical_record`: mỗi lịch hẹn đã khám có một hồ sơ khám tương ứng;
- `doctor` 1 — N `doctor_schedule`: một bác sĩ có nhiều slot lịch trực theo từng tháng;
- `doctor` 1 — N `appointment`: một bác sĩ phụ trách nhiều lịch hẹn;
- `department` 1 — N `doctor`: một chuyên khoa có nhiều bác sĩ;
- `site` 1 — N tất cả các bảng nghiệp vụ: mọi dữ liệu đều scope theo `site_id` để hỗ trợ multi-tenant khi mở rộng cho hệ thống TTYT toàn TP Hải Phòng.

Mọi bảng nghiệp vụ đều có trường `SiteId` để dữ liệu được scope theo cơ sở. Khi mở rộng, một hệ thống duy nhất có thể phục vụ nhiều TTYT (Kinh Môn, An Lưu, Phú Thái) mà dữ liệu giữa các đơn vị không bị rò rỉ, đồng thời hạn chế nguy cơ lỗ hổng IDOR ở tầng query.

\newpage
