# Cây Dữ Liệu (ERD) — TTYT Kinh Môn

Sơ đồ thực thể–quan hệ của 23 bảng trong CSDL `ttytlp`. Chia 2 cụm: (a) Nghiệp vụ y tế và (b) CMS / quản trị.

## Cụm 1 — Nghiệp vụ y tế (core)

```mermaid
erDiagram
    User ||--o{ Appointment : "đặt lịch"
    User ||--o{ Question : "đặt câu hỏi"
    User ||--o{ MedicalRecord : "khám bệnh"
    User }o--|| UserGroup : "thuộc nhóm"
    User ||--o{ Credential : "xác thực"
    Doctor ||--o{ DoctorSchedule : "có lịch trực"
    Doctor ||--o{ MedicalRecord : "lập hồ sơ"
    Doctor ||--o{ Answer : "trả lời"
    Doctor }o--|| Department : "thuộc khoa"
    Department ||--o{ Appointment : "tiếp nhận"
    Department ||--o{ AppointmentQuota : "có quota"
    Appointment ||--o| MedicalRecord : "phát sinh hồ sơ"
    MedicalRecord ||--o{ Prescription : "có đơn thuốc"
    Question ||--o| Answer : "được trả lời"

    User {
        Guid id PK
        string user_name UK
        string full_name
        string phone
        string email
        string password_hash
        Guid group_id FK
        bool must_change_password
    }
    UserGroup {
        Guid id PK
        string code "ADMIN/DOCTOR/RECEPTION/MEMBER"
        string name_l
    }
    Doctor {
        Guid id PK
        string name_l
        string specially_l
        Guid department_id FK
        string image_path
    }
    Department {
        Guid id PK
        string name_l
        string alias UK
        int ord
        bool active_flag
    }
    DoctorSchedule {
        Guid id PK
        Guid doctor_id FK
        DateTime work_date
        string session "morning/afternoon"
        string room
    }
    Appointment {
        Guid id PK
        string patient_name
        string patient_phone
        string patient_email
        Guid department_id FK
        DateTime appointment_date
        string session
        string status "pending/confirmed/rescheduled/rejected/cancelled/completed"
        string booking_code "FK->MedicalRecord"
        string staff_note "lý do từ chối/đổi lịch"
        bool checked_in
        Guid created_by_user_id FK
    }
    AppointmentQuota {
        Guid id PK
        Guid department_id FK
        DateTime work_date
        string session
        int max_count
        int booked_count
    }
    MedicalRecord {
        Guid id PK
        string record_no UK "HSyymmddNNNN"
        Guid patient_user_id FK
        Guid doctor_id FK
        DateTime visit_date
        string diagnosis
        string symptoms
        string treatment
    }
    Prescription {
        Guid id PK
        Guid medical_record_id FK
        string drug_name
        string dosage
        string usage_note
    }
    Question {
        Guid id PK
        Guid patient_user_id FK
        string title
        string body
        string topic
        bool is_public
        bool approved
    }
    Answer {
        Guid id PK
        Guid question_id FK
        Guid doctor_id FK
        string body
        DateTime answered_date
    }
    Credential {
        Guid id PK
        Guid user_id FK
        string role_code
    }
```

## Cụm 2 — CMS / quản trị

```mermaid
erDiagram
    Site ||--o{ Category : "scope"
    Site ||--o{ News : "scope"
    Site ||--o{ Slide : "scope"
    Site ||--o{ Doctor : "scope"
    Site ||--o{ Department : "scope"
    Category ||--o{ News : "phân loại"
    Category }o--o| Category : "parent (self-ref)"
    News ||--o{ Comment : "có bình luận"

    Site {
        Guid id PK
        string name_company_l
        string logo_url
        string favicon
        string address_l
        string phone
        string hotline
        string emergency_number
        string email
    }
    Category {
        Guid id PK
        string name_l
        string alias_l UK
        Guid parent_id FK
        string image_path "URL hoặc fa:fa-icon"
        int ord
        string link "redirect target tuỳ chọn"
    }
    News {
        Guid id PK
        string title_l
        string alias_l UK
        Guid category_id FK
        Guid site_id FK
        string image_path
        string description_l
        string content_l
        DateTime created_date
    }
    Slide {
        Guid id PK
        string image_path
        string link
        int ord
        Guid site_id FK
    }
    Comment {
        Guid id PK
        Guid news_id FK
        string user_name
        string body
        DateTime created_date
        bool active_flag
    }
    Page {
        Guid id PK
        string alias_l UK
        string content_l
    }
    Document {
        Guid id PK
        string title_l
        string attach_file_path
    }
    Partner {
        Guid id PK
        string name_l
        string logo
        string website
    }
    Video {
        Guid id PK
        string video_url
        string video_thumbnail
        string video_description_l
    }
    Role {
        Guid id PK
        string code UK
        string name_l
    }
    AuditSystem {
        Guid id PK
        Guid user_id FK
        string action
        string detail
        DateTime audit_date
    }
```

## Quy tắc ràng buộc nghiệp vụ (đã code-enforce)

| Quy tắc | Vị trí enforce | Test |
|---|---|---|
| `Appointment.status` chỉ chuyển theo whitelist | `AppointmentService.AllowedTransitions` | unit + functional |
| Reject phải có lý do (`staff_note`) | `AppointmentService.UpdateStatusAsync` | unit + functional |
| Bệnh nhân không đặt 2 lịch trùng buổi | `AppointmentService.CreateAsync` (duplicate check) | unit + functional |
| Quota không vượt `max_count` khi confirm | `AppointmentService.UpdateStatusAsync` | unit |
| `MedicalRecord.record_no` unique (race-safe retry 5 lần) | `MedicalRecordService.CreateAsync` | unit |
| Bác sĩ A không truy cập bệnh nhân của bác sĩ B | `DoctorPortalController.ChanDoan` cross-guard | functional |
| Check-in chỉ khi `confirmed` + đúng ngày | `LeTanController.MarkCheckedIn` | unit |
| Q&A submit yêu cầu login | `QnaController.DatCauHoi` | functional |
| Length cap 500/200/100 ký tự cho note/drug/dosage | service layer | unit |
