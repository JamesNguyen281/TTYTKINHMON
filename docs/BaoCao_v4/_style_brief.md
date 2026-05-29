# Brief văn phong học thuật — đồ án K63 ĐH GTVT

Brief này được rút từ 3 đồ án mẫu (Nguyễn Thu Hiền, Nguyễn Thị Thùy Dương, Trần Lam Liên). Dùng làm tham chiếu khi viết lại các chương báo cáo TTYTKM v5.

## CẤM (AI-tells phải gỡ sạch)

1. **Câu mở đoạn meta**: "Trong chương này...", "Trên cơ sở...", "Đây chính là cơ sở để...", "Như đã đề cập...", "Tiếp theo, chúng ta sẽ...", "Phần này sẽ trình bày..."
2. **Bold giữa câu** kiểu `**Mục tiêu 1 — ...**` hay `**Lưu trữ mật khẩu an toàn:**`. Bold CHỈ ở heading markdown (`##`, `###`) và ở dòng "**Bảng X.Y. ...**" / "**Hình X.Y. ...**".
3. **Em-dash `—`** rải khắp câu. Mỗi đoạn tối đa 1 em-dash; đa số câu KHÔNG có. Thay bằng dấu phẩy, dấu hai chấm, hoặc tách câu.
4. **Bullet+bold sub-header** kiểu `**a. Yêu cầu về sản phẩm:**` → đổi thành tiêu đề `**a. Yêu cầu về sản phẩm**` rồi xuống dòng viết đoạn văn (không bullet ngay sau).
5. **Self-praise**: "là một quyết định kiến trúc quan trọng", "giải pháp tối ưu", "cách tiếp cận hiện đại", "mạnh mẽ", "toàn diện", "tinh gọn", "hoàn thiện", "vượt trội", "tốt nhất hiện nay".
6. **Câu tổng kết cuối đoạn**: "Như vậy có thể thấy...", "Tóm lại,...", "Điều này chứng minh rằng..."
7. **Parenthetical kỹ thuật chêm vào** quá nhiều: "(thể hiện bằng tam giác UML)", "(super-user)", "(bird's-eye view)", "(defense-in-depth)". Chỉ giữ chú thích khi thực sự cần.
8. **Emoji, dấu ✅ ❌ 📌**: không bao giờ.
9. **Bảng so sánh nhị phân Trước/Sau, Vấn đề/Giải pháp** in đậm in italics.
10. **Liệt kê 1. 2. 3. rồi giải thích từng item dài**: chỉ dùng số đếm cho luồng sự kiện use case. Liệt kê thông thường dùng gạch đầu dòng `-`.

## YÊU CẦU (theo mẫu 3 đồ án tham khảo)

1. **Mở đầu chương/mục**: vào thẳng nội dung. Hoặc bằng định nghĩa ("X là..."), hoặc bằng câu khẳng định bối cảnh ngắn ("Trong thời đại công nghệ thông tin phát triển..."). Không có câu dẫn meta.
2. **Chủ ngữ chính**: "Hệ thống", "Website", "Đề tài", "Đồ án", tên công nghệ ("ASP.NET Core...", "EF Core..."), "Người dùng/Bệnh nhân/Lễ tân/Bác sĩ/Quản trị viên". KHÔNG dùng "em" trong các chương chính (chỉ được dùng ở Lời mở đầu/Lời cảm ơn). KHÔNG dùng "Tác giả", "Chúng tôi", "Tôi".
3. **Bị động + không chủ ngữ** dùng nhiều: "được thiết kế để...", "được sử dụng...", "Việc xây dựng... giúp..."
4. **Đoạn văn xuôi 3-5 câu** ở các phần giải thích công nghệ/khái niệm. Không bullet quá dài.
5. **Gạch đầu dòng `-`** khi liệt kê yêu cầu, chức năng. KHÔNG dùng `•`. Sub-bullet dùng `+ ` hoặc indent.
6. **Thuật ngữ tiếng Anh** giữ nguyên + chú thích tiếng Việt trong ngoặc LẦN ĐẦU: "API (Application Programming Interface — Giao diện lập trình ứng dụng)", "RDBMS (Relational Database Management System)". Sau lần đầu dùng thẳng tiếng Anh.
7. **Trích nguồn `[n]`** đặt cuối câu/đoạn lý thuyết (vd: "OWASP khuyến nghị PBKDF2 với ≥ 600.000 vòng lặp [13]").
8. **Câu chuyển đoạn** ngắn gọn: "Tuy nhiên,", "Ngoài ra,", "Đồng thời,", "Bên cạnh đó,", "Đặc biệt,", "Với...".
9. **Đếm thứ tự**: chương "Chương 1.", mục "1.1.", "1.1.1.", "a)", "b)", "c)" cho subsub. Sau tiêu đề a) b) c) là đoạn văn, KHÔNG mở bullet ngay.

## GIỮ NGUYÊN

- Cấu trúc heading markdown (`#`, `##`, `###`, `####`)
- Mọi đường dẫn ảnh `![Hình X.Y. ...](images/...){...}`
- Mọi bảng markdown (dòng `**Bảng X.Y...**` + bảng `| ... | ... |`)
- Mọi block code (```bash, ```csharp, v.v.)
- Mọi block trích dẫn (`>`)
- Mọi attribute pandoc (`{.unnumbered}`, `\newpage`)
- Số chương, số mục, số bảng, số hình
- Các số liệu kỹ thuật: 84 unit test, 94 functional, 279 E2E, 18 lỗi, 600.000 vòng PBKDF2, v.v.

## VÍ DỤ TRƯỚC/SAU

**TRƯỚC (AI-gen):**
> Trên cơ sở khảo sát yêu cầu và phạm vi đề tài, toàn bộ chức năng của website Trung tâm Y tế phường Kinh Môn được phân rã thành **năm nhóm chức năng** ứng với năm vai trò người dùng — Public Site (khách vãng lai), Bệnh nhân (member), Lễ tân, Bác sĩ và Quản trị viên. Cách phân rã này phản ánh trực tiếp cấu trúc thư mục mã nguồn...

**SAU (học thuật):**
> Toàn bộ chức năng của website được phân rã thành năm nhóm ứng với năm vai trò người dùng: Public Site, Bệnh nhân, Lễ tân, Bác sĩ và Quản trị viên. Cách phân rã bám theo mô hình phân quyền dựa trên bảng `system_user_group` cũng như cấu trúc thư mục mã nguồn.

**TRƯỚC:**
> **Bảo mật:** Vì website lưu trữ thông tin sức khỏe của người dân — thuộc dạng dữ liệu nhạy cảm được pháp luật bảo vệ — đồ án đặt ra các yêu cầu cụ thể như sau:
> - **Lưu trữ mật khẩu an toàn:** không lưu mật khẩu dạng plain text;...

**SAU:**
> **c. Yêu cầu bảo mật**
>
> Website lưu trữ thông tin sức khỏe của người dân, là dạng dữ liệu nhạy cảm được pháp luật bảo vệ theo Luật Khám bệnh, chữa bệnh năm 2023 [1]. Đồ án đặt ra các yêu cầu bảo mật cụ thể như sau:
>
> - Mật khẩu không lưu dạng plain text mà được băm bằng thuật toán PBKDF2 kết hợp SHA-256 với salt riêng cho từng tài khoản và 600.000 vòng lặp theo khuyến nghị của OWASP năm 2023 [13];
> - ...
