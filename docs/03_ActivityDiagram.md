# Sơ Đồ Hoạt Động (Activity & Sequence Diagram) — TTYT Kinh Môn

Tài liệu mô tả luồng nghiệp vụ chi tiết cho 5 flow chính của hệ thống. Mọi sơ đồ render bằng Mermaid (xem trên GitHub / VS Code preview).

Quy ước:
- Activity diagram: thể hiện rẽ nhánh, quyết định, trạng thái nghiệp vụ.
- Sequence diagram: thể hiện thông điệp giữa Browser → Controller → Service → DB và side-effect (booking code, audit log, quota delta).

---

## 1. UC07 — Đặt lịch khám trực tuyến

### 1.1 Activity diagram

```mermaid
flowchart TD
    Start([Bắt đầu]) --> A1[Truy cập /dat-lich-kham]
    A1 --> Q1{CurrentUser != null?}
    Q1 -- Có --> A2[Auto-fill họ tên, SĐT, email từ session]
    Q1 -- Không --> A3[Hiển thị banner gợi ý đăng nhập + form rỗng]
    A2 --> A4[Chọn chuyên khoa, ngày, buổi sáng/chiều, lý do]
    A3 --> A4
    A4 --> A5[Submit form - POST /dat-lich-kham]
    A5 --> Q2{Anti-forgery token hợp lệ?}
    Q2 -- Không --> E1[400 Bad Request]
    Q2 -- Có --> A6[Validate input ở Service]
    A6 --> Q3{Họ tên + SĐT đầy đủ?}
    Q3 -- Không --> E2[Flash error: thiếu thông tin bắt buộc]
    Q3 -- Có --> Q4{Ngày khám hợp lệ?<br/>today ≤ date ≤ today+30?}
    Q4 -- Không --> E3[Flash error: ngày không hợp lệ]
    Q4 -- Có --> Q5{Department active?}
    Q5 -- Không --> E4[Flash error: chuyên khoa không khả dụng]
    Q5 -- Có --> Q6{Member đã có lịch trùng buổi?}
    Q6 -- Có --> E5[Flash error: bạn đã có lịch buổi này]
    Q6 -- Không --> A7[Tạo Appointment status=pending, CheckedIn=false]
    A7 --> A8[Lưu DB]
    A8 --> A9[Audit log: APPOINTMENT_CREATED]
    A9 --> A10[Redirect /lich-cua-toi với flash success]
    A10 --> End([Kết thúc])
    E1 --> End
    E2 --> A4
    E3 --> A4
    E4 --> A4
    E5 --> A4
```

### 1.2 Sequence diagram

```mermaid
sequenceDiagram
    actor P as Bệnh nhân
    participant Br as Browser
    participant Ctrl as AppointmentController
    participant Svc as AppointmentService
    participant DB as SQL Server (ttytlp)
    participant Au as AuditService

    P->>Br: GET /dat-lich-kham
    Br->>Ctrl: Index()
    Ctrl->>Svc: ListActiveDepartments(siteId)
    Svc->>DB: SELECT Department WHERE active_flag=1
    DB-->>Svc: List<Department>
    Svc-->>Ctrl: VM
    Ctrl-->>Br: View với form + dropdown chuyên khoa

    P->>Br: Submit form
    Br->>Ctrl: POST /dat-lich-kham (CSRF token, BookingInputModel)
    Ctrl->>Ctrl: ValidateAntiForgeryToken
    Ctrl->>Svc: CreateAsync(input, currentUserId, siteId)
    Svc->>Svc: Validate phone/name/date/department
    Svc->>DB: SELECT Department WHERE Id=...
    DB-->>Svc: Department (ActiveFlag=1)
    Svc->>DB: SELECT 1 FROM Appointment WHERE patient_user_id, dept, date, session, status IN (pending, confirmed)
    DB-->>Svc: 0 rows (chưa trùng)
    Svc->>DB: INSERT Appointment (status=pending)
    DB-->>Svc: OK
    Svc-->>Ctrl: BookingResult{Success=true, AppointmentId}
    Ctrl->>Au: LogAsync("APPOINTMENT_CREATED", apptId, "Tạo lịch chờ duyệt")
    Au->>DB: INSERT AuditSystem
    Ctrl-->>Br: 302 → /lich-cua-toi (flash success)
    Br-->>P: Hiển thị "Đã đặt lịch — chờ lễ tân xác nhận"
```

---

## 2. UC21 — Lễ tân duyệt lịch + sinh booking code

### 2.1 Activity diagram

```mermaid
flowchart TD
    Start([Bắt đầu]) --> A1[Lễ tân login → /le-tan]
    A1 --> Q1{Group=RECEPTION?}
    Q1 -- Không --> E0[403 Forbidden cross-portal guard]
    Q1 -- Có --> A2[Truy cập /le-tan/lich-hen?status=pending]
    A2 --> A3[Service.GetByStatusAsync filter siteId]
    A3 --> A4[Click 1 lịch → /le-tan/chi-tiet/&#123;id&#125;]
    A4 --> A5[Chọn newStatus: confirmed/rejected/rescheduled]
    A5 --> A6[Submit form UpdateStatus]
    A6 --> Q2{Anti-forgery + StaffAuthorize OK?}
    Q2 -- Không --> E1[401/403]
    Q2 -- Có --> A7[Service.UpdateStatusAsync]
    A7 --> Q3{newStatus thuộc whitelist?}
    Q3 -- Không --> E2[Lỗi: trạng thái không hợp lệ]
    Q3 -- Có --> Q4{Transition oldStatus → newStatus<br/>nằm trong AllowedTransitions?}
    Q4 -- Không --> E3[Lỗi: không thể chuyển trạng thái]
    Q4 -- Có --> Q5{newStatus = rejected?}
    Q5 -- Có --> Q6{StaffNote không trống?}
    Q6 -- Không --> E4[Lỗi: vui lòng nhập lý do từ chối]
    Q6 -- Có --> A8[Đặt status=rejected]
    Q5 -- Không --> Q7{newStatus = confirmed?}
    Q7 -- Có --> A9[Đọc/Tạo AppointmentQuota row]
    A9 --> Q8{BookedCount + 1 ≤ MaxCount?}
    Q8 -- Không --> E5[Lỗi: buổi này đã hết suất]
    Q8 -- Có --> A10[BookedCount += 1]
    A10 --> A11[Sinh booking_code KM yyMMdd S/C 6hex]
    A11 --> A12[Đặt status=confirmed]
    Q7 -- Không --> A13[confirmed → cancelled/rescheduled<br/>BookedCount -= 1]
    A8 --> A14[SaveChanges]
    A12 --> A14
    A13 --> A14
    A14 --> Q9{DbUpdateConcurrencyException?}
    Q9 -- Có & retry < 3 --> A7
    Q9 -- Có & retry = 3 --> E6[Lỗi: có người khác đã update — thử lại]
    Q9 -- Không --> A15[Audit log: APPOINTMENT_STATUS oldStatus → newStatus]
    A15 --> A16[Redirect về danh sách + flash success]
    A16 --> End([Kết thúc])
    E0 --> End
    E1 --> End
    E2 --> A5
    E3 --> A5
    E4 --> A5
    E5 --> A5
    E6 --> End
```

### 2.2 Sequence diagram

```mermaid
sequenceDiagram
    actor R as Lễ tân
    participant Br as Browser
    participant Ctrl as LeTanController
    participant Svc as AppointmentService
    participant DB as SQL Server
    participant Au as AuditService

    R->>Br: GET /le-tan/lich-hen?status=pending
    Br->>Ctrl: Appointments("pending")
    Ctrl->>Svc: GetByStatusAsync("pending", siteId)
    Svc->>DB: SELECT Appointment WHERE status=pending, site_id=...
    DB-->>Svc: List<Appointment>
    Svc-->>Ctrl: List<AppointmentRow>
    Ctrl-->>Br: View danh sách

    R->>Br: Submit form duyệt (newStatus=confirmed)
    Br->>Ctrl: POST /le-tan/cap-nhat-trang-thai (id, newStatus, staffNote)
    Ctrl->>Svc: UpdateStatusAsync(id, "confirmed", note, staffId)
    Svc->>Svc: Check whitelist transition pending→confirmed (OK)
    Svc->>DB: SELECT Appointment WHERE Id=...
    DB-->>Svc: Appointment(pending)
    Svc->>DB: SELECT AppointmentQuota WHERE dept, date, session
    DB-->>Svc: Quota or null
    alt Quota null
        Svc->>DB: INSERT AppointmentQuota(MaxCount=DefaultQuotaPerSession, BookedCount=1)
    else Quota.BookedCount + 1 > MaxCount
        Svc-->>Ctrl: Fail "Buổi này đã hết suất"
        Ctrl-->>Br: Flash error
    else
        Svc->>Svc: Quota.BookedCount += 1
        Svc->>Svc: BookingCode = "KM" + yyMMdd + S/C + 6hex
        Svc->>DB: UPDATE Appointment SET status=confirmed, booking_code=...
        Svc->>DB: UPDATE AppointmentQuota SET booked_count
        DB-->>Svc: OK
        Svc-->>Ctrl: UpdateStatusResult{Success=true, BookingCode}
        Ctrl->>Au: LogAsync("APPOINTMENT_STATUS", id, "pending → confirmed")
        Au->>DB: INSERT AuditSystem
        Ctrl-->>Br: 302 → /le-tan/lich-hen + flash "Đã duyệt KM26..."
    end
```

---

## 3. UC24 — Check-in bệnh nhân ngày khám

```mermaid
flowchart TD
    Start([Bắt đầu]) --> A1[Bệnh nhân tới quầy lễ tân]
    A1 --> A2[Đọc/quét booking_code KM26...]
    A2 --> A3[Lễ tân nhập code → /le-tan/check-in]
    A3 --> A4[GetByBookingCodeAsync code]
    A4 --> Q1{Tìm thấy?}
    Q1 -- Không --> E1[Flash: Mã không tồn tại]
    Q1 -- Có --> Q2{status = confirmed?}
    Q2 -- Không --> E2[Flash: Lịch chưa được duyệt]
    Q2 -- Có --> Q3{appointment_date = hôm nay?}
    Q3 -- Không --> E3[Flash: Không đúng ngày khám]
    Q3 -- Có --> Q4{Đã checked_in?}
    Q4 -- Có --> A5[Idempotent: trả OK]
    Q4 -- Không --> A6[CheckedIn=true, LuUpdated, LuUserId]
    A6 --> A7[SaveChanges]
    A7 --> A8[Audit log: CHECK_IN]
    A5 --> A9[Hiển thị thông tin BN + chuyển khoa]
    A8 --> A9
    A9 --> End([Bệnh nhân chờ gọi tên])
    E1 --> End
    E2 --> End
    E3 --> End
```

---

## 4. UC31 — Bác sĩ tạo hồ sơ khám

### 4.1 Activity diagram

```mermaid
flowchart TD
    Start([Bắt đầu]) --> A1[Bác sĩ login → /bac-si-portal]
    A1 --> A2[Vào /bac-si-portal/benh-nhan-hom-nay]
    A2 --> A3[GetTodayConfirmedAsync filter checked_in=true, doctor_id=current]
    A3 --> A4[Chọn 1 BN → /bac-si-portal/chan-doan/&#123;apptId&#125;]
    A4 --> Q1{Cross-doctor guard:<br/>appt.doctor_id = current doctor?}
    Q1 -- Không --> E0[403: bệnh nhân không thuộc bác sĩ này]
    Q1 -- Có --> A5[Form: triệu chứng, chẩn đoán, điều trị, đơn thuốc]
    A5 --> A6[Submit POST /bac-si-portal/luu-ho-so]
    A6 --> Q2{Diagnosis không trống?}
    Q2 -- Không --> E1[Flash: chẩn đoán bắt buộc]
    Q2 -- Có --> A7[NextRecordNoAsync sinh HSyymmddNNNN]
    A7 --> A8[Tạo MedicalRecord]
    A8 --> A9[Insert Prescription nếu có]
    A9 --> A10[SaveChanges]
    A10 --> Q3{DbUpdateException - record_no collision?}
    Q3 -- Có & retry < 5 --> A7
    Q3 -- Có & retry = 5 --> E2[500: lỗi sinh mã hồ sơ]
    Q3 -- Không --> A11[UpdateStatus appt → completed]
    A11 --> A12[Audit log: MEDICAL_RECORD_CREATED]
    A12 --> A13[Redirect /bac-si-portal/benh-nhan-hom-nay + flash]
    A13 --> End([Kết thúc])
    E0 --> End
    E1 --> A5
    E2 --> End
```

### 4.2 Sequence diagram

```mermaid
sequenceDiagram
    actor D as Bác sĩ
    participant Br as Browser
    participant Ctrl as DoctorPortalController
    participant ApptSvc as AppointmentService
    participant MrSvc as MedicalRecordService
    participant DB as SQL Server
    participant Au as AuditService

    D->>Br: GET /bac-si-portal/chan-doan/&#123;apptId&#125;
    Br->>Ctrl: ChanDoan(apptId)
    Ctrl->>ApptSvc: GetByIdAsync(apptId)
    ApptSvc-->>Ctrl: Appointment
    Ctrl->>Ctrl: assert appt.doctor_id == currentDoctorId
    Ctrl-->>Br: View form chẩn đoán

    D->>Br: Submit (diagnosis, prescriptions[])
    Br->>Ctrl: POST /bac-si-portal/luu-ho-so
    Ctrl->>MrSvc: CreateAsync(MedicalRecordInput, doctorId, patientId)
    loop Retry up to 5x on collision
        MrSvc->>MrSvc: NextRecordNoAsync — count today, format HSyymmddNNNN
        MrSvc->>DB: INSERT MedicalRecord, Prescription[]
        DB-->>MrSvc: OK or DbUpdateException
    end
    MrSvc-->>Ctrl: MedicalRecord
    Ctrl->>ApptSvc: UpdateStatusAsync(apptId, "completed", null, doctorId)
    ApptSvc->>DB: UPDATE Appointment SET status=completed
    Ctrl->>Au: LogAsync("MEDICAL_RECORD_CREATED", recordId, record_no)
    Au->>DB: INSERT AuditSystem
    Ctrl-->>Br: 302 → /bac-si-portal/benh-nhan-hom-nay
```

---

## 5. UC11 + UC34 — Q&A: Đặt câu hỏi → Bác sĩ trả lời

```mermaid
flowchart TD
    Start([Bắt đầu]) --> A1[BN login → /dat-cau-hoi]
    A1 --> Q1{CurrentUser != null?}
    Q1 -- Không --> E0[Redirect /dang-nhap]
    Q1 -- Có --> A2[Form: title, body, topic, is_public]
    A2 --> A3[Submit POST /dat-cau-hoi]
    A3 --> A4[QnaService.CreateAsync — status=pending, approved=false]
    A4 --> A5[Redirect /cau-hoi-cua-toi flash success]
    A5 --> A6[Bác sĩ /bac-si-portal/duyet-cau-hoi]
    A6 --> A7[GetPendingAsync]
    A7 --> A8[Trả lời body + approve]
    A8 --> A9[QnaService.AnswerAsync]
    A9 --> A10[INSERT Answer + UPDATE Question.approved=true]
    A10 --> A11[Audit log: QUESTION_ANSWERED]
    A11 --> A12[BN xem /cau-hoi-cua-toi → thấy câu trả lời<br/>Public xem /hoi-dap nếu is_public=true]
    A12 --> End([Kết thúc])
```

---

## 6. State diagram — Vòng đời Appointment

```mermaid
stateDiagram-v2
    [*] --> pending: BN đặt lịch
    pending --> confirmed: Lễ tân duyệt + đủ quota
    pending --> rejected: Lễ tân từ chối + lý do
    pending --> cancelled: BN huỷ
    pending --> rescheduled: Lễ tân đề nghị đổi
    confirmed --> completed: Bác sĩ tạo hồ sơ khám
    confirmed --> cancelled: BN huỷ trước ngày khám
    confirmed --> rescheduled: Lễ tân đổi lịch
    rescheduled --> confirmed: BN đồng ý lịch mới + duyệt lại
    rescheduled --> rejected: BN từ chối
    rescheduled --> cancelled: BN huỷ
    rejected --> [*]
    cancelled --> [*]
    completed --> [*]
```

**Whitelist** đã code-enforce trong `AppointmentService.AllowedTransitions`:

| Old status | Allowed new status |
|---|---|
| `pending` | `confirmed`, `rejected`, `cancelled`, `rescheduled` |
| `confirmed` | `completed`, `cancelled`, `rescheduled` |
| `rescheduled` | `confirmed`, `rejected`, `cancelled` |
| `rejected` / `cancelled` / `completed` | (terminal — không cho đổi) |

Mọi transition vi phạm → reject với message `"Không thể chuyển trạng thái '{old}' → '{new}'"`.

---

## 7. Tham chiếu chéo

| Sơ đồ | Use case | Controller | Service | Test cases |
|---|---|---|---|---|
| §1 Đặt lịch | UC07 | `AppointmentController.Index` | `AppointmentService.CreateAsync` | TC-007 → TC-011 |
| §2 Duyệt lịch | UC21 | `LeTanController.UpdateStatus` | `AppointmentService.UpdateStatusAsync` | TC-020 → TC-024 |
| §3 Check-in | UC24 | `LeTanController.MarkCheckedIn` | `AppointmentService.MarkCheckedInAsync` | TC-025, TC-026 |
| §4 Hồ sơ khám | UC31 | `DoctorPortalController.ChanDoan` | `MedicalRecordService.CreateAsync` | TC-030 → TC-033 |
| §5 Q&A | UC11+UC34 | `QnaController` + `DoctorPortalController.AnswerQuestion` | `QnaService` | TC-040 → TC-043 |
| §6 State | UC07+UC21+UC31 | All | `AppointmentService.AllowedTransitions` | TC-050, TC-051 |
