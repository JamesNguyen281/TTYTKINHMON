# Website Trung tâm Y tế phường Kinh Môn (TTYT Kinh Môn)

> Đồ án tốt nghiệp K63 — *Nghiên cứu, xây dựng website y tế và triển khai kiểm thử tự động*
>
> **Sinh viên**: Nguyễn Trường Anh · MSV 221220741 · CNTT1 K63 · GVHD ThS. Nguyễn Thu Hường — Trường Đại học Giao thông Vận tải

## Stack

- **Backend**: ASP.NET Core 8 MVC + EF Core 8
- **Frontend**: Razor View + Bootstrap 3 + jQuery
- **Database**: SQL Server Express (`.\SQLEXPRESS`, schema `ttytlp`, 25 bảng)

## Cấu trúc

```
SourceCodeTTYTKM/
├── README.md                    ← bạn đang ở đây
├── .gitignore
│
├── WebsiteCore/                 ← SOURCE CODE (.NET 8)
│   ├── WebsiteCore.sln
│   └── src/
│       ├── WebsiteCore.Data/        — Entities + DbContext (EF Core)
│       ├── WebsiteCore.Business/    — Services + ViewModels (DI)
│       └── WebsiteCore.Web/         — MVC + Razor + AdminCP area
│
└── docs/                        ← Tài liệu kiến trúc
    ├── README.md
    ├── 01_ERD.md                    — Sơ đồ ERD
    ├── 02_UseCase.md                — Use case chi tiết theo actor
    └── 03_ActivityDiagram.md        — Activity diagram cho các flow chính
```

## Tính năng chính

### 3 surface chính

- **Public** (`/`): trang chủ, tin tức, đặt lịch khám (Khoa Khám bệnh + 7 phòng khám), đặt câu hỏi Q&A, xem bác sĩ + lịch trực
- **AdminCP** (`/AdminCP`): quản trị Site / Department / ClinicRoom / Doctor / News / Category / Slide / Video / Partner / Document / Quota / DoctorSchedule / ScheduleRequest — có pagination 10/20/50/100/1000/All
- **Portals**: Lễ tân (`/le-tan`) — duyệt lịch (auto-purge khách vãng lai > 3 ngày), check-in, phân BS theo slot · Bác sĩ (`/bac-si-portal`) — bệnh nhân hôm nay, chẩn đoán 2 nhánh, hẹn khám lại auto, lịch trực, gửi yêu cầu đổi lịch

### Workflow HIS chuẩn

1. Bệnh nhân đặt lịch (`/dat-lich-kham`) → status=`pending`
2. Lễ tân duyệt (`/le-tan/lich-hen`) → status=`confirmed` + sinh `booking_code`
3. Bệnh nhân tới phòng khám → Lễ tân check-in bằng booking_code
4. Bác sĩ vào `/bac-si-portal` → "Bệnh nhân hôm nay" → bấm "Chẩn đoán"
5. Bác sĩ nhập chẩn đoán + đơn thuốc → tự lưu `MedicalRecord` + status=`completed`
6. Bệnh nhân xem lịch sử khám tại `/lich-su-kham`

### Bảo mật

- PBKDF2-SHA256 600 000 iterations + auto-rehash MD5 legacy
- Lockout 15 phút sau 5 lần sai pass
- CSRF token + `[ValidateAntiForgeryToken]` trên mọi POST
- XSS sanitize `SanitizeHtml()` + CSP header
- IDOR guard: site scoping qua `siteId`, cross-doctor guard
- Audit log mọi thay đổi state (duyệt lịch, chẩn đoán, đổi pass)

## Bắt đầu phát triển

### 1. Yêu cầu môi trường

- .NET SDK 8.0 hoặc 9.0
- SQL Server Express (instance `.\SQLEXPRESS`)
- Windows / macOS / Linux

### 2. Cấu hình DB

Mở `WebsiteCore/src/WebsiteCore.Web/appsettings.json` và sửa `ConnectionStrings:Default` nếu cần (mặc định trỏ `.\SQLEXPRESS`, DB `ttytlp`):

```json
"ConnectionStrings": {
  "Default": "Server=.\\SQLEXPRESS;Database=ttytlp;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Tạo schema (lần đầu):

```bash
cd WebsiteCore/src/WebsiteCore.Web
dotnet ef database update
```

### 3. Build & Run

```bash
# Build
dotnet build WebsiteCore/src/WebsiteCore.Web/WebsiteCore.Web.csproj

# Run dev server
cd WebsiteCore/src/WebsiteCore.Web
dotnet run --urls "http://localhost:5050"
```

Truy cập http://localhost:5050.

### 4. Tài khoản default

| Role | Username | Password mặc định | Note |
|---|---|---|---|
| Admin | `admin` | `123456` | Force đổi pass lần đầu login |
| Lễ tân | `letan` | `123456` | Force đổi pass lần đầu login |
| Bác sĩ | `Bacsy` | `123456` | Force đổi pass lần đầu login |

> ⚠️ Đây chỉ là **default cho local dev**. Khi deploy production phải đổi pass ngay qua `/AdminCP/Users` hoặc force-change-password flow.

Bệnh nhân đăng ký tài khoản qua `/dang-ky`.

## Quy ước code

- **Bilingual fields**: `name_l` / `name_e` cho tiếng Việt / Anh
- **Site scoping**: query luôn filter theo `(Guid siteId, string locate)`
- **State machine**: `AppointmentService.AllowedTransitions` whitelist
- **Race-safe**: `MedicalRecordService.NextRecordNoAsync()` retry 5x trên `DbUpdateException`
- **Length cap**: `SafeTrim` 500 / 200 / 100 ký tự cho note / drug / dosage chống DoS
- **Vietnamese encoder**: `Program.cs` config `WebEncoderOptions.UnicodeRanges.All`
- **EF Core 8 compat**: `UseCompatibilityLevel(120)` để tương thích DB compat 120 (chống lỗi OPENJSON)

## Database schema

25 bảng entity:

| Nhóm | Bảng |
|---|---|
| Authn / Authz | `User`, `UserGroup`, `Role`, `Credential` |
| Hồ sơ y tế | `Doctor`, `Department`, `ClinicRoom`, `MedicalRecord`, `Prescription`, `Appointment`, `AppointmentQuota`, `DoctorSchedule`, `ScheduleChangeRequest` |
| Q&A | `Question`, `Answer` |
| CMS | `News`, `Category`, `Page`, `Document`, `Comment` |
| Marketing | `Slide`, `Video`, `Partner` |
| Hệ thống | `Site`, `AuditSystem` |

Xem [docs/01_ERD.md](docs/01_ERD.md) để hiểu quan hệ FK đầy đủ.

## License

Đồ án sinh viên — không có giấy phép công khai. Nếu sử dụng tham khảo, vui lòng trích dẫn:
> Nguyễn Trường Anh, *Nghiên cứu, xây dựng website y tế và triển khai kiểm thử tự động*, Khóa luận tốt nghiệp K63 CNTT1, Trường Đại học Giao thông Vận tải, 2026.
