using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Data;

public partial class TtytlpDbContext : DbContext
{
    public TtytlpDbContext(DbContextOptions<TtytlpDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentQuotum> AppointmentQuota { get; set; }

    public virtual DbSet<AuditSystem> AuditSystems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Credential> Credentials { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<Slide> Slides { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<ScheduleChangeRequest> ScheduleChangeRequests { get; set; }

    public virtual DbSet<ClinicRoom> ClinicRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answer__3213E83FB6194140");

            entity.ToTable("Answer");

            entity.HasIndex(e => e.QuestionId, "IX_Answer_question");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DoctorUserId).HasColumnName("doctor_user_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3213E83F819C954D");

            entity.ToTable("Appointment");

            entity.HasIndex(e => e.PatientUserId, "IX_Appointment_patient");

            entity.HasIndex(e => e.Status, "IX_Appointment_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
            entity.Property(e => e.AppointmentTime).HasColumnName("appointment_time");
            entity.Property(e => e.BookingCode)
                .HasMaxLength(20)
                .HasColumnName("booking_code");
            entity.Property(e => e.CheckedIn).HasColumnName("checked_in");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(250)
                .HasColumnName("department_name");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.PatientEmail)
                .HasMaxLength(300)
                .HasColumnName("patient_email");
            entity.Property(e => e.PatientName)
                .HasMaxLength(300)
                .HasColumnName("patient_name");
            entity.Property(e => e.PatientPhone)
                .HasMaxLength(50)
                .HasColumnName("patient_phone");
            entity.Property(e => e.PatientUserId).HasColumnName("patient_user_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Session)
                .HasMaxLength(20)
                .HasColumnName("session");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.StaffNote).HasColumnName("staff_note");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            // P2.A: BN được lễ tân route vào ClinicRoom (phòng khám trong khoa "Khoa Khám bệnh")
            entity.Property(e => e.ClinicRoomId).HasColumnName("clinic_room_id");
            // P2.A: Cờ cấp cứu — bypass workflow phòng khám thường
            entity.Property(e => e.IsEmergency).HasColumnName("is_emergency");
            entity.HasIndex(e => e.ClinicRoomId, "IX_Appointment_ClinicRoom");
        });

        modelBuilder.Entity<AppointmentQuotum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3213E83F32FECFE9");

            entity.HasIndex(e => new { e.DoctorId, e.ApptDate, e.Session }, "IX_Quota_doctor").HasFilter("([doctor_id] IS NOT NULL)");

            entity.HasIndex(e => new { e.DepartmentId, e.ApptDate, e.Session }, "UX_Quota_dept_date_session")
                .IsUnique()
                .HasFilter("([department_id] IS NOT NULL AND [doctor_id] IS NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.BookedCount).HasColumnName("booked_count");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.MaxCount)
                .HasDefaultValue(30)
                .HasColumnName("max_count");
            entity.Property(e => e.Session)
                .HasMaxLength(20)
                .HasColumnName("session");
        });

        modelBuilder.Entity<AuditSystem>(entity =>
        {
            entity.ToTable("AuditSystem");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActionDate)
                .HasColumnType("datetime")
                .HasColumnName("action_date");
            entity.Property(e => e.ActionDescription)
                .HasMaxLength(500)
                .HasColumnName("action_description");
            entity.Property(e => e.ActionDetail)
                .HasColumnType("ntext")
                .HasColumnName("action_detail");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.AliasE)
                .HasMaxLength(250)
                .HasColumnName("alias_e");
            entity.Property(e => e.AliasL)
                .HasMaxLength(250)
                .HasColumnName("alias_l");
            entity.Property(e => e.CreatedByUser).HasColumnName("created_by_user");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DescriptionE)
                .HasColumnType("ntext")
                .HasColumnName("description_e");
            entity.Property(e => e.DescriptionL)
                .HasColumnType("ntext")
                .HasColumnName("description_l");
            entity.Property(e => e.HotCategory)
                .HasDefaultValue(false)
                .HasColumnName("hot_category");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasColumnName("link");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.MenuId).HasColumnName("menu_id");
            entity.Property(e => e.NameE)
                .HasMaxLength(250)
                .HasColumnName("name_e");
            entity.Property(e => e.NameL)
                .HasMaxLength(250)
                .HasColumnName("name_l");
            entity.Property(e => e.Ord)
                .HasDefaultValue(0)
                .HasColumnName("ord");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ShowOnHome)
                .HasDefaultValue(false)
                .HasColumnName("show_on_home");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.ThemeType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("theme_type");
            entity.Property(e => e.Type)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_comment");

            entity.ToTable("Comment");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag).HasColumnName("active_flag");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Message)
                .HasColumnType("ntext")
                .HasColumnName("message");
            entity.Property(e => e.NewId).HasColumnName("new_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(150)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => new { e.UserGroupId, e.RoleId }).HasName("PK_Permission");

            entity.ToTable("Credential");

            entity.Property(e => e.UserGroupId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("user_group_id");
            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_id");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag).HasColumnName("active_flag");
            entity.Property(e => e.IsClinicalDept).HasColumnName("is_clinical_dept");
            entity.Property(e => e.Alias)
                .HasMaxLength(350)
                .HasColumnName("alias");
            entity.Property(e => e.BackgroundImage)
                .HasMaxLength(500)
                .HasColumnName("background_image");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DescriptionE)
                .HasColumnType("ntext")
                .HasColumnName("description_e");
            entity.Property(e => e.DescriptionL)
                .HasColumnType("ntext")
                .HasColumnName("description_l");
            entity.Property(e => e.DetailE)
                .HasColumnType("ntext")
                .HasColumnName("detail_e");
            entity.Property(e => e.DetailL)
                .HasColumnType("ntext")
                .HasColumnName("detail_l");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasColumnName("link");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.NameE)
                .HasMaxLength(250)
                .HasColumnName("name_e");
            entity.Property(e => e.NameL)
                .HasMaxLength(250)
                .HasColumnName("name_l");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.SubLink)
                .HasMaxLength(500)
                .HasColumnName("sub_link");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctor");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag).HasColumnName("active_flag");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.ExperiencesE)
                .HasMaxLength(500)
                .HasColumnName("experiences_e");
            entity.Property(e => e.ExperiencesL)
                .HasMaxLength(500)
                .HasColumnName("experiences_l");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.IsPartner).HasColumnName("is_partner");
            entity.Property(e => e.LanguageSpoken)
                .HasMaxLength(150)
                .HasColumnName("language_spoken");
            entity.Property(e => e.NameE)
                .HasMaxLength(250)
                .HasColumnName("name_e");
            entity.Property(e => e.NameL)
                .HasMaxLength(250)
                .HasColumnName("name_l");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.Position)
                .HasMaxLength(150)
                .HasColumnName("position");
            entity.Property(e => e.QuantificationE)
                .HasColumnType("ntext")
                .HasColumnName("quantification_e");
            entity.Property(e => e.QuantificationL)
                .HasColumnType("ntext")
                .HasColumnName("quantification_l");
            entity.Property(e => e.ShowOnHome).HasColumnName("show_on_home");
            entity.Property(e => e.SpeciallyE)
                .HasMaxLength(250)
                .HasColumnName("specially_e");
            entity.Property(e => e.SpeciallyInterestsE)
                .HasMaxLength(500)
                .HasColumnName("specially_interests_e");
            entity.Property(e => e.SpeciallyInterestsL)
                .HasMaxLength(500)
                .HasColumnName("specially_interests_l");
            entity.Property(e => e.SpeciallyL)
                .HasMaxLength(250)
                .HasColumnName("specially_l");
            entity.Property(e => e.TimetableE)
                .HasColumnType("ntext")
                .HasColumnName("timetable_e");
            entity.Property(e => e.TimetableL)
                .HasColumnType("ntext")
                .HasColumnName("timetable_l");
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DoctorSc__3213E83FCFC261C1");

            entity.ToTable("DoctorSchedule");

            entity.HasIndex(e => new { e.DepartmentId, e.Weekday, e.Session, e.ActiveFlag }, "IX_DocSched_Dept");

            entity.HasIndex(e => new { e.DoctorId, e.ActiveFlag, e.Weekday, e.Session }, "IX_DocSched_Doctor");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.MaxPatients).HasColumnName("max_patients");
            entity.Property(e => e.Note)
                .HasMaxLength(300)
                .HasColumnName("note");
            entity.Property(e => e.Room)
                .HasMaxLength(100)
                .HasColumnName("room");
            entity.Property(e => e.Session)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("session");
            entity.Property(e => e.ValidFrom)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.Weekday).HasColumnName("weekday");
            // P2.B: 2 loại lịch trực (clinic / emergency / management)
            entity.Property(e => e.ScheduleType).HasMaxLength(20).HasColumnName("schedule_type");
            // P2.A: BS được luân phiên gán vào ClinicRoom — chỉ khi schedule_type='clinic'
            entity.Property(e => e.ClinicRoomId).HasColumnName("clinic_room_id");
            entity.HasIndex(e => e.ClinicRoomId, "IX_DocSched_ClinicRoom");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_legal_document");

            entity.ToTable("Document");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag).HasColumnName("active_flag");
            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(250)
                .HasColumnName("approved_by");
            entity.Property(e => e.ApprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("approved_date");
            entity.Property(e => e.AttachFilePath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("attach_file_path");
            entity.Property(e => e.BinLocation)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("bin_location");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("created_date_time");
            entity.Property(e => e.Description)
                .HasColumnType("ntext")
                .HasColumnName("description");
            entity.Property(e => e.DocumentCode)
                .HasMaxLength(100)
                .HasColumnName("document_code");
            entity.Property(e => e.DocumentDate)
                .HasColumnType("datetime")
                .HasColumnName("document_date");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(500)
                .HasColumnName("document_name");
            entity.Property(e => e.EffectiveFromDate)
                .HasColumnType("datetime")
                .HasColumnName("effective_from_date");
            entity.Property(e => e.EffectiveToDate)
                .HasColumnType("datetime")
                .HasColumnName("effective_to_date");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.Owner)
                .HasMaxLength(500)
                .HasColumnName("owner");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MedicalR__3213E83F34D75BB7");

            entity.ToTable("MedicalRecord");

            entity.HasIndex(e => new { e.PatientUserId, e.VisitDate }, "IX_MedicalRecord_Patient").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ChiefComplaint)
                .HasMaxLength(500)
                .HasColumnName("chief_complaint");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(2000)
                .HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.FollowUpDate).HasColumnName("follow_up_date");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientUserId).HasColumnName("patient_user_id");
            entity.Property(e => e.RecordNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("record_no");
            entity.Property(e => e.TreatmentPlan)
                .HasMaxLength(2000)
                .HasColumnName("treatment_plan");
            entity.Property(e => e.VisitDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("visit_date");
            // P2.C: Hồ sơ ngoại trú vs nội trú
            entity.Property(e => e.RecordType).HasMaxLength(20).HasColumnName("record_type");
            entity.Property(e => e.IsHospitalized).HasColumnName("is_hospitalized");
            entity.Property(e => e.TargetInpatientDeptId).HasColumnName("target_inpatient_dept_id");
            entity.Property(e => e.HospitalizationNote).HasMaxLength(500).HasColumnName("hospitalization_note");
            entity.HasIndex(e => e.IsHospitalized, "IX_MedicalRecord_IsHospitalized").HasFilter("[is_hospitalized] = 1");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.AliasE)
                .HasMaxLength(250)
                .HasColumnName("alias_e");
            entity.Property(e => e.AliasL)
                .HasMaxLength(250)
                .HasColumnName("alias_l");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Copyright)
                .HasMaxLength(500)
                .HasColumnName("copyright");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DescriptionE)
                .HasColumnType("ntext")
                .HasColumnName("description_e");
            entity.Property(e => e.DescriptionL)
                .HasColumnType("ntext")
                .HasColumnName("description_l");
            entity.Property(e => e.DetailE)
                .HasColumnType("ntext")
                .HasColumnName("detail_e");
            entity.Property(e => e.DetailL)
                .HasColumnType("ntext")
                .HasColumnName("detail_l");
            entity.Property(e => e.HotNew)
                .HasDefaultValue(false)
                .HasColumnName("hot_new");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasColumnName("link");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.MetaDescription)
                .HasColumnType("ntext")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeyword)
                .HasColumnType("ntext")
                .HasColumnName("meta_keyword");
            entity.Property(e => e.Ord)
                .HasDefaultValue(0)
                .HasColumnName("ord");
            entity.Property(e => e.ShowOnHome)
                .HasDefaultValue(false)
                .HasColumnName("show_on_home");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.TitleE)
                .HasMaxLength(250)
                .HasColumnName("title_e");
            entity.Property(e => e.TitleL)
                .HasMaxLength(250)
                .HasColumnName("title_l");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.Views).HasColumnName("views");

            entity.HasOne(d => d.Category).WithMany(p => p.News)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_News_Category");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("Page");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag).HasColumnName("active_flag");
            entity.Property(e => e.CreateByUserId).HasColumnName("create_by_user_id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.DetailE)
                .HasColumnType("ntext")
                .HasColumnName("detail_e");
            entity.Property(e => e.DetailL)
                .HasColumnType("ntext")
                .HasColumnName("detail_l");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.MenuId).HasColumnName("menu_id");
            entity.Property(e => e.MetaDescription)
                .HasColumnType("ntext")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeyword)
                .HasMaxLength(500)
                .HasColumnName("meta_keyword");
            entity.Property(e => e.TitleE)
                .HasMaxLength(250)
                .HasColumnName("title_e");
            entity.Property(e => e.TitleL)
                .HasMaxLength(250)
                .HasColumnName("title_l");
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("Partner");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasColumnName("link");
            entity.Property(e => e.NameE)
                .HasMaxLength(250)
                .HasColumnName("name_e");
            entity.Property(e => e.NameL)
                .HasMaxLength(250)
                .HasColumnName("name_l");
            entity.Property(e => e.Ord)
                .HasDefaultValue(0)
                .HasColumnName("ord");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3213E83F99F6E3BF");

            entity.ToTable("Prescription");

            entity.HasIndex(e => new { e.MedicalRecordId, e.Ord }, "IX_Prescription_Record");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Dosage)
                .HasMaxLength(100)
                .HasColumnName("dosage");
            entity.Property(e => e.DrugName)
                .HasMaxLength(250)
                .HasColumnName("drug_name");
            entity.Property(e => e.Duration)
                .HasMaxLength(100)
                .HasColumnName("duration");
            entity.Property(e => e.Frequency)
                .HasMaxLength(100)
                .HasColumnName("frequency");
            entity.Property(e => e.MedicalRecordId).HasColumnName("medical_record_id");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.Ord).HasColumnName("ord");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F59AFC40A");

            entity.ToTable("Question");

            entity.HasIndex(e => e.PatientUserId, "IX_Question_patient");

            entity.HasIndex(e => e.Status, "IX_Question_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.PatientUserId).HasColumnName("patient_user_id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .HasColumnName("title");
            entity.Property(e => e.Topic)
                .HasMaxLength(100)
                .HasColumnName("topic");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.CssClass).HasColumnName("cssClass");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SystemConfig");

            entity.ToTable("Site");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.AddressE)
                .HasMaxLength(500)
                .HasColumnName("address_e");
            entity.Property(e => e.AddressL)
                .HasMaxLength(500)
                .HasColumnName("address_l");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DashboardImage)
                .HasMaxLength(500)
                .HasColumnName("dashboard_image");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyNumber)
                .HasMaxLength(50)
                .HasColumnName("emergency_number");
            entity.Property(e => e.Favicon)
                .HasMaxLength(500)
                .HasColumnName("favicon");
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasColumnName("fax");
            entity.Property(e => e.Hotline)
                .HasMaxLength(50)
                .HasColumnName("hotline");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Map)
                .HasMaxLength(500)
                .HasColumnName("map");
            entity.Property(e => e.MetaDescription)
                .HasColumnType("ntext")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeyword)
                .HasColumnType("ntext")
                .HasColumnName("meta_keyword");
            entity.Property(e => e.MobilePhone)
                .HasMaxLength(50)
                .HasColumnName("mobile_phone");
            entity.Property(e => e.NameCompanyE)
                .HasMaxLength(250)
                .HasColumnName("name_company_e");
            entity.Property(e => e.NameCompanyL)
                .HasMaxLength(250)
                .HasColumnName("name_company_l");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.TimeOpen)
                .HasMaxLength(150)
                .HasColumnName("time_open");
        });

        modelBuilder.Entity<Slide>(entity =>
        {
            entity.ToTable("Slide");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CssClass)
                .HasMaxLength(50)
                .HasColumnName("css_class");
            entity.Property(e => e.DescriptionE)
                .HasMaxLength(250)
                .HasColumnName("description_e");
            entity.Property(e => e.DescriptionL)
                .HasMaxLength(250)
                .HasColumnName("description_l");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Link)
                .HasMaxLength(150)
                .HasColumnName("link");
            entity.Property(e => e.Ord)
                .HasDefaultValue(0)
                .HasColumnName("ord");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.TitleE)
                .HasMaxLength(250)
                .HasColumnName("title_e");
            entity.Property(e => e.TitleL)
                .HasMaxLength(250)
                .HasColumnName("title_l");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActiveFlag)
                .HasDefaultValue(1)
                .HasColumnName("active_flag");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.Allergies)
                .HasMaxLength(500)
                .HasColumnName("allergies");
            entity.Property(e => e.MedicalHistory)
                .HasColumnName("medical_history");
            entity.Property(e => e.BhytCard)
                .HasMaxLength(20)
                .HasColumnName("bhyt_card");
            entity.Property(e => e.BloodType)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("blood_type");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("cccd");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(200)
                .HasColumnName("emergency_contact");
            entity.Property(e => e.FailedAttempts).HasColumnName("failed_attempts");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasDefaultValue(1)
                .HasColumnName("gender");
            entity.Property(e => e.GroupId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("POSTER")
                .HasColumnName("group_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("last_login");
            entity.Property(e => e.LockoutUntil)
                .HasColumnType("datetime")
                .HasColumnName("lockout_until");
            entity.Property(e => e.LuUpdated)
                .HasColumnType("datetime")
                .HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserName)
                .HasMaxLength(250)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.ToTable("UserGroup");

            entity.Property(e => e.Id)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.ToTable("Video");

            entity.Property(e => e.VideoId)
                .ValueGeneratedNever()
                .HasColumnName("video_id");
            entity.Property(e => e.CreatedByUser).HasColumnName("created_by_user");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.VideoDescriptionE)
                .HasMaxLength(500)
                .HasColumnName("video_description_e");
            entity.Property(e => e.VideoDescriptionL)
                .HasMaxLength(500)
                .HasColumnName("video_description_l");
            entity.Property(e => e.VideoLink)
                .HasMaxLength(500)
                .HasColumnName("video_link");
            entity.Property(e => e.VideoThumbnail)
                .HasMaxLength(500)
                .HasColumnName("video_thumbnail");
            entity.Property(e => e.VideoTitleE)
                .HasMaxLength(250)
                .HasColumnName("video_title_e");
            entity.Property(e => e.VideoTitleL)
                .HasMaxLength(250)
                .HasColumnName("video_title_l");
        });

        modelBuilder.Entity<ScheduleChangeRequest>(entity =>
        {
            entity.ToTable("ScheduleChangeRequest");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.RequestedDate).HasColumnName("requested_date");
            entity.Property(e => e.RequestedSession).HasMaxLength(20).HasColumnName("requested_session");
            entity.Property(e => e.RequestType).HasMaxLength(30).HasColumnName("request_type");
            entity.Property(e => e.Reason).HasMaxLength(2000).HasColumnName("reason");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.AdminResponse).HasMaxLength(2000).HasColumnName("admin_response");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime").HasColumnName("created_date");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ProcessedDate).HasColumnType("datetime").HasColumnName("processed_date");
            entity.HasIndex(e => e.DoctorId);
            entity.HasIndex(e => e.Status);
        });

        // ClinicRoom — phòng khám trong khoa "Khoa Khám bệnh" (xem ClinicRoom.cs)
        modelBuilder.Entity<ClinicRoom>(entity =>
        {
            entity.ToTable("ClinicRoom");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.RoomCode).HasMaxLength(50).HasColumnName("room_code");
            entity.Property(e => e.RoomName).HasMaxLength(200).HasColumnName("room_name");
            entity.Property(e => e.SpecialtyL).HasMaxLength(200).HasColumnName("specialty_l");
            entity.Property(e => e.SpecialtyE).HasMaxLength(200).HasColumnName("specialty_e");
            entity.Property(e => e.Floor).HasMaxLength(50).HasColumnName("floor");
            entity.Property(e => e.CommonSymptoms).HasMaxLength(1000).HasColumnName("common_symptoms");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.ActiveFlag).HasDefaultValue(1).HasColumnName("active_flag");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime").HasColumnName("created_date");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LuUpdated).HasColumnType("datetime").HasColumnName("lu_updated");
            entity.Property(e => e.LuUserId).HasColumnName("lu_user_id");
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => new { e.DepartmentId, e.RoomCode }).IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
