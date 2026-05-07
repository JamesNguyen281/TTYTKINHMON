using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Web.Helpers;

namespace WebsiteCore.Web.Controllers;

/// <summary>Cổng bác sĩ — chỉ DOCTOR + ADMIN truy cập.</summary>
[StaffAuthorize(Constants.DoctorGroup)]
public class DoctorPortalController : BaseController
{
    private readonly IQnaService _qnaService;
    private readonly IAppointmentService _apptService;
    private readonly IAuditService _auditService;
    private readonly IMedicalRecordService _mrService;
    private readonly IUserService _userService;
    private readonly IDoctorScheduleService _scheduleService;
    private readonly IDepartmentService _deptService;
    private readonly IScheduleChangeRequestService _scrService;

    public DoctorPortalController(
        ISiteService siteService,
        IQnaService qnaService,
        IAppointmentService apptService,
        IAuditService auditService,
        IMedicalRecordService mrService,
        IUserService userService,
        IDoctorScheduleService scheduleService,
        IDepartmentService deptService,
        IScheduleChangeRequestService scrService) : base(siteService)
    {
        _qnaService = qnaService;
        _apptService = apptService;
        _auditService = auditService;
        _mrService = mrService;
        _userService = userService;
        _scheduleService = scheduleService;
        _deptService = deptService;
        _scrService = scrService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Cổng bác sĩ";
        var pending = await _qnaService.GetPendingAsync();
        ViewBag.PendingQuestions = pending;

        var u = CurrentUser!;
        var todayList = u.DoctorId.HasValue
            ? await _apptService.GetByDoctorAsync(u.DoctorId.Value, DateTime.Today, DateTime.Today)
            : new List<Business.ViewModels.AppointmentRow>();
        ViewBag.TodayList     = todayList;
        ViewBag.TodayCount    = todayList.Count;
        ViewBag.CheckedInCount= todayList.Count(t => t.CheckedIn);
        ViewBag.CompletedCount= todayList.Count(t => t.Status == Constants.ApptCompleted);
        ViewBag.PendingDxCount= todayList.Count(t => t.CheckedIn && t.Status != Constants.ApptCompleted);
        ViewBag.LinkedDoctorId= u.DoctorId;
        return View();
    }

    public async Task<IActionResult> BenhNhanHomNay()
    {
        ViewBag.Title = "Bệnh nhân hôm nay";
        var u = CurrentUser!;
        var todayAppts = u.DoctorId.HasValue
            ? await _apptService.GetByDoctorAsync(u.DoctorId.Value, DateTime.Today, DateTime.Today)
            : new List<Business.ViewModels.AppointmentRow>();
        ViewBag.TodayAppts = todayAppts;
        ViewBag.LinkedDoctorId = u.DoctorId;
        return View();
    }

    /// <summary>JSON snapshot dùng cho polling real-time bên cổng bác sĩ.</summary>
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> TodayCounts()
    {
        var u = CurrentUser!;
        if (!u.DoctorId.HasValue) return Json(new { total = 0, checkedIn = 0 });
        var list = await _apptService.GetByDoctorAsync(u.DoctorId.Value, DateTime.Today, DateTime.Today);
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Json(new {
            total = list.Count,
            checkedIn = list.Count(t => t.CheckedIn)
        });
    }

    /// <summary>
    /// Lịch trực của riêng bác sĩ đang đăng nhập, theo chu kỳ tháng.
    /// Query string ?ym=2026-05 để xem tháng cụ thể; default = tháng hiện tại.
    /// </summary>
    public async Task<IActionResult> LichTruc(string? ym = null)
    {
        var u = CurrentUser!;
        ViewBag.Title = "Lịch trực của tôi";

        // Parse ym (yyyy-MM) — default tháng hiện tại
        DateTime cycle;
        if (!string.IsNullOrWhiteSpace(ym)
            && DateTime.TryParseExact(ym + "-01", "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsed))
        {
            cycle = parsed;
        }
        else
        {
            cycle = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
        var firstDay = new DateOnly(cycle.Year, cycle.Month, 1);
        var lastDay  = firstDay.AddMonths(1).AddDays(-1);

        ViewBag.CycleFirstDay = firstDay;
        ViewBag.CycleLastDay  = lastDay;
        ViewBag.CycleLabel    = $"{cycle:MM/yyyy}";
        ViewBag.PrevYm        = cycle.AddMonths(-1).ToString("yyyy-MM");
        ViewBag.NextYm        = cycle.AddMonths(1).ToString("yyyy-MM");
        ViewBag.LinkedDoctorId = u.DoctorId;

        if (!u.DoctorId.HasValue)
        {
            ViewBag.Schedules = new List<Data.Entities.DoctorSchedule>();
            ViewBag.DepartmentName = null;
            ViewBag.DoctorName = u.FullName;
            return View();
        }

        // Lấy schedule active của bs này, lọc theo chu kỳ tháng (overlap với firstDay..lastDay)
        var all = await _scheduleService.GetByDoctorAsync(u.DoctorId.Value);
        var schedules = all
            .Where(s => s.ValidFrom <= lastDay
                     && (s.ValidTo == null || s.ValidTo >= firstDay))
            .OrderBy(s => s.Weekday).ThenBy(s => s.Session)
            .ToList();
        ViewBag.Schedules = schedules;

        // Đếm số ca khám thực tế trong tháng (Appointment đã confirmed/completed)
        var apptList = await _apptService.GetByDoctorAsync(
            u.DoctorId.Value,
            firstDay.ToDateTime(TimeOnly.MinValue),
            lastDay.ToDateTime(TimeOnly.MaxValue));
        ViewBag.ApptList = apptList;
        ViewBag.ApptCountTotal     = apptList.Count;
        ViewBag.ApptCountCompleted = apptList.Count(a => a.Status == Constants.ApptCompleted);
        ViewBag.ApptCountPending   = apptList.Count(a => a.Status == Constants.ApptPending);

        // Lấy tên khoa để hiển thị
        var firstDept = schedules.FirstOrDefault()?.DepartmentId;
        if (firstDept.HasValue)
        {
            var dep = await _deptService.GetByIdAsync(firstDept.Value);
            ViewBag.DepartmentName = dep?.NameL;
        }
        ViewBag.DoctorName = u.FullName;
        return View();
    }

    /// <summary>Form gửi yêu cầu đổi/thêm/xoá lịch trực — bs tự gửi tới admin duyệt.</summary>
    [HttpGet]
    public async Task<IActionResult> YeuCauDoiLich()
    {
        var u = CurrentUser!;
        ViewBag.Title = "Yêu cầu đổi lịch trực";
        ViewBag.Schedules = u.DoctorId.HasValue
            ? await _scheduleService.GetByDoctorAsync(u.DoctorId.Value)
            : new List<Data.Entities.DoctorSchedule>();
        // Lịch sử request của chính bs này
        ViewBag.MyRequests = u.DoctorId.HasValue
            ? await _scrService.GetByDoctorAsync(u.DoctorId.Value)
            : new List<Data.Entities.ScheduleChangeRequest>();
        ViewBag.LinkedDoctorId = u.DoctorId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YeuCauDoiLich(string requestType, Guid? scheduleId,
        DateTime? requestedDate, string? requestedSession, string reason)
    {
        var u = CurrentUser!;
        if (!u.DoctorId.HasValue)
        {
            TempData["Error"] = "Tài khoản chưa liên kết Doctor — không thể gửi yêu cầu.";
            return RedirectToAction(nameof(YeuCauDoiLich));
        }
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10)
        {
            TempData["Error"] = "Vui lòng nhập lý do (≥ 10 ký tự).";
            return RedirectToAction(nameof(YeuCauDoiLich));
        }
        var allowedTypes = new[] { "change", "add", "remove", "swap" };
        if (string.IsNullOrWhiteSpace(requestType) || !allowedTypes.Contains(requestType))
            requestType = "change";
        if (!string.IsNullOrWhiteSpace(requestedSession)
            && requestedSession != Constants.SessionMorning
            && requestedSession != Constants.SessionAfternoon)
        {
            requestedSession = null;
        }

        var req = new Data.Entities.ScheduleChangeRequest
        {
            DoctorId         = u.DoctorId.Value,
            ScheduleId       = scheduleId,
            RequestedDate    = requestedDate.HasValue ? DateOnly.FromDateTime(requestedDate.Value) : null,
            RequestedSession = requestedSession,
            RequestType      = requestType,
            Reason           = reason.Trim(),
            CreatedBy        = u.Id
        };
        var newId = await _scrService.CreateAsync(req);
        await _auditService.LogAsync(u.Id, "Gửi yêu cầu đổi lịch",
            $"{u.UserName} gửi yêu cầu {requestType}, ngày={req.RequestedDate}, session={requestedSession ?? "-"}, reason='{(req.Reason.Length > 100 ? req.Reason.Substring(0, 100) + "…" : req.Reason)}'");
        TempData["Success"] = $"Đã gửi yêu cầu (mã #{newId.ToString().Substring(0, 8)}). Quản trị viên sẽ duyệt và phản hồi.";
        return RedirectToAction(nameof(YeuCauDoiLich));
    }

    /// <summary>Hồ sơ khám đã ký bởi chính bác sĩ này (filter DoctorId + cross-site).</summary>
    public async Task<IActionResult> HoSoDaKham(string? q)
    {
        ViewBag.Title = "Hồ sơ đã chẩn đoán";
        var u = CurrentUser!;
        if (!u.DoctorId.HasValue)
        {
            ViewBag.Records = new List<Data.Entities.MedicalRecord>();
            ViewBag.Q = q;
            return View(new List<Data.Entities.MedicalRecord>());
        }
        var list = await _mrService.GetByDoctorAsync(u.DoctorId.Value, CurrentSiteId, q);
        ViewBag.Q = q;
        return View(list);
    }

    /// <summary>Chi tiết hồ sơ — guard: chỉ xem được nếu chính bác sĩ này ký.</summary>
    public async Task<IActionResult> HoSoChiTiet(Guid id)
    {
        var u = CurrentUser!;
        var m = await _mrService.GetByIdAsync(id);
        if (m == null) return NotFound();

        // Cross-doctor guard: bs chỉ xem hồ sơ chính mình ký (admin role bypass đã handle ở MedicalRecordsController)
        if (u.GroupId != Constants.AdminGroup)
        {
            if (!u.DoctorId.HasValue || m.DoctorId != u.DoctorId.Value)
            {
                TempData["Error"] = "Hồ sơ này thuộc bác sĩ khác — bạn không có quyền xem.";
                return RedirectToAction(nameof(HoSoDaKham));
            }
        }

        // Cross-site guard qua appointment
        if (m.AppointmentId.HasValue)
        {
            var appt = await _apptService.GetByIdAsync(m.AppointmentId.Value);
            if (appt != null && appt.SiteId != CurrentSiteId) return NotFound();
        }

        ViewBag.Title = "Chi tiết hồ sơ " + m.RecordNo;
        ViewBag.Prescriptions = await _mrService.GetPrescriptionsAsync(id);
        return View(m);
    }

    public async Task<IActionResult> DuyetCauHoi(Guid id)
    {
        ViewBag.Title = "Duyệt và trả lời câu hỏi";
        var q = await _qnaService.GetByIdAsync(id);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnswerQuestion(Guid id, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Vui lòng nhập câu trả lời.";
            return RedirectToAction(nameof(DuyetCauHoi), new { id });
        }
        var u = CurrentUser!;
        await _qnaService.AnswerAsync(id, u.Id, body);
        await _auditService.LogAsync(u.Id, "Trả lời câu hỏi", $"{u.UserName} trả lời câu hỏi {id}");
        TempData["Success"] = "Đã gửi câu trả lời tới bệnh nhân.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChanDoan(Guid apptId)
    {
        var appt = await _apptService.GetByIdAsync(apptId);
        if (appt == null) return NotFound();

        var u = CurrentUser!;
        // Site scoping — chặn cross-site
        if (appt.SiteId != CurrentSiteId) return NotFound();
        // Cross-doctor guard — bs A không được xem hồ sơ y tế (CCCD/BHYT/Allergies/MedicalHistory)
        // của bệnh nhân thuộc bs B. Chỉ ADMIN role hoặc bs đã được phân lịch mới được xem.
        if (u.GroupId != Constants.AdminGroup
            && u.DoctorId.HasValue
            && appt.DoctorId.HasValue
            && appt.DoctorId.Value != u.DoctorId.Value)
        {
            TempData["Error"] = "Lịch này thuộc bác sĩ khác — bạn không có quyền xem hồ sơ.";
            return RedirectToAction(nameof(BenhNhanHomNay));
        }

        ViewBag.Title = "Chẩn đoán cho " + appt.PatientName;
        ViewBag.Appointment = appt;
        ViewBag.NextRecordNo = await _mrService.NextRecordNoAsync();
        // Load patient User để pre-fill BloodType/Allergies + hiện CCCD/BHYT/Dob để bác sĩ tham khảo
        ViewBag.Patient = appt.PatientUserId.HasValue
            ? await _userService.GetByIdAsync(appt.PatientUserId.Value)
            : null;
        // P2.C — danh sách khoa nội trú đích cho dropdown khi BS chọn "Nội trú".
        // Loại trừ Khoa Khám bệnh (đó là wrapper) để tránh chuyển BN về cùng nơi.
        var allDepts = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.InpatientDepartments = allDepts
            .Where(d => !string.Equals(d.Alias, "khoa-kham-benh", StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.Ord ?? 999).ThenBy(d => d.NameL)
            .ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChanDoan(Guid apptId, string chiefComplaint, string diagnosis,
        string? treatmentPlan, string? notes, DateTime? followUpDate,
        string? bloodType, string? allergies, string? medicalHistory,
        string[]? drugName, string[]? dosage, string[]? frequency, string[]? duration, string[]? prescriptionNote,
        string? recordType, Guid? targetInpatientDeptId, string? hospitalizationNote)
    {
        var appt = await _apptService.GetByIdAsync(apptId);
        if (appt == null) return NotFound();
        // Site scoping (đồng bộ với GET handler)
        if (appt.SiteId != CurrentSiteId) return NotFound();
        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            TempData["Error"] = "Phải nhập chẩn đoán.";
            return RedirectToAction(nameof(ChanDoan), new { apptId });
        }
        if (!appt.PatientUserId.HasValue)
        {
            TempData["Error"] = "Lịch khách vãng lai chưa có tài khoản — vui lòng tạo hồ sơ trên AdminCP.";
            return RedirectToAction(nameof(BenhNhanHomNay));
        }
        // Bác sĩ phải khám bệnh nhân của mình — chống URL tampering
        var u = CurrentUser!;
        if (u.DoctorId.HasValue && appt.DoctorId.HasValue && appt.DoctorId.Value != u.DoctorId.Value)
        {
            TempData["Error"] = "Bạn không có quyền chẩn đoán cho lịch của bác sĩ khác.";
            return RedirectToAction(nameof(BenhNhanHomNay));
        }
        // Yêu cầu bệnh nhân đã check-in trước khi tạo hồ sơ
        if (!appt.CheckedIn)
        {
            TempData["Error"] = "Bệnh nhân chưa check-in — không thể tạo hồ sơ khám.";
            return RedirectToAction(nameof(BenhNhanHomNay));
        }

        static string? Cap(string? s, int max) =>
            s == null ? null : (s.Length > max ? s.Substring(0, max) : s).Trim();

        // P2.C — phân nhánh ngoại trú vs nội trú
        // Default outpatient (cấp toa về). Inpatient yêu cầu BS chỉ định khoa nhập viện đích.
        var rt = (recordType ?? Constants.RecordTypeOutpatient).ToLowerInvariant();
        bool isInpatient = rt == Constants.RecordTypeInpatient;
        if (isInpatient && (!targetInpatientDeptId.HasValue || targetInpatientDeptId.Value == Guid.Empty))
        {
            TempData["Error"] = "Hồ sơ nội trú phải chỉ định khoa nhập viện đích.";
            return RedirectToAction(nameof(ChanDoan), new { apptId });
        }
        // Cross-site guard: khoa nhập viện phải cùng site với appointment (chống IDOR).
        if (isInpatient && targetInpatientDeptId.HasValue)
        {
            var targetDept = await _deptService.GetByIdAsync(targetInpatientDeptId.Value);
            if (targetDept == null || targetDept.SiteId != appt.SiteId)
            {
                TempData["Error"] = "Khoa nhập viện không hợp lệ hoặc không thuộc cơ sở này.";
                return RedirectToAction(nameof(ChanDoan), new { apptId });
            }
        }

        var record = new Data.Entities.MedicalRecord
        {
            PatientUserId         = appt.PatientUserId.Value,
            AppointmentId         = appt.Id,
            DoctorId              = u.DoctorId,
            DepartmentId          = appt.DepartmentId,
            VisitDate             = DateTime.Now,
            ChiefComplaint        = Cap(chiefComplaint, 1000),
            Diagnosis             = Cap(diagnosis, 1000)!,
            TreatmentPlan         = Cap(treatmentPlan, 2000),
            Notes                 = Cap(notes, 2000),
            FollowUpDate          = followUpDate.HasValue ? DateOnly.FromDateTime(followUpDate.Value) : null,
            RecordType            = isInpatient ? Constants.RecordTypeInpatient : Constants.RecordTypeOutpatient,
            IsHospitalized        = isInpatient,
            TargetInpatientDeptId = isInpatient ? targetInpatientDeptId : null,
            HospitalizationNote   = isInpatient ? Cap(hospitalizationNote, 500) : null,
        };
        var prescriptions = new List<Data.Entities.Prescription>();
        const int MaxRx = 50; // chặn input tham lam (DoS protection)
        if (drugName != null)
        {
            int n = Math.Min(drugName.Length, MaxRx);
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(drugName[i])) continue;
                prescriptions.Add(new Data.Entities.Prescription
                {
                    DrugName  = Cap(drugName[i], 200)!,
                    Dosage    = Cap(dosage != null && i < dosage.Length ? dosage[i] : null, 100),
                    Frequency = Cap(frequency != null && i < frequency.Length ? frequency[i] : null, 100),
                    Duration  = Cap(duration != null && i < duration.Length ? duration[i] : null, 100),
                    Note      = Cap(prescriptionNote != null && i < prescriptionNote.Length ? prescriptionNote[i] : null, 500)
                });
            }
        }
        try
        {
            await _mrService.CreateAsync(record, prescriptions, u.Id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(ChanDoan), new { apptId });
        }
        // Đồng bộ thông tin y tế bệnh nhân: BloodType + Allergies từ buổi khám này
        // → vào User entity, để lần sau bệnh nhân vào /ho-so thấy update của bác sĩ.
        var medChanged = await _userService.UpdateMedicalInfoAsync(
            appt.PatientUserId.Value, bloodType, allergies, medicalHistory, u.Id);

        // Audit trail: ghi rõ ai/cái gì/khi nào
        await _auditService.LogAsync(u.Id, "Tạo hồ sơ",
            $"{u.UserName} tạo HS {record.RecordNo} cho {appt.PatientName} (apptId={apptId}) | dx='{Cap(diagnosis, 200)}' | rx={prescriptions.Count}{(medChanged ? " | medical-info updated" : "")}");
        // Mark appointment completed automatically after diagnosis
        await _apptService.UpdateStatusAsync(appt.Id, Constants.ApptCompleted, "Đã khám và lập hồ sơ.", u.Id);
        TempData["Success"] = $"Đã tạo hồ sơ {record.RecordNo}.";
        return RedirectToAction(nameof(BenhNhanHomNay));
    }

    /// <summary>
    /// P3.B — BS bấm "Hẹn khám lại": tạo appointment con với status=confirmed +
    /// booking_code (BS quyết, không cần lễ tân duyệt). Increment quota tự động.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HenKhamLai(Guid currentApptId, DateTime followUpDate, string session)
    {
        var u = CurrentUser!;
        var current = await _apptService.GetByIdAsync(currentApptId);
        if (current == null) return NotFound();
        if (current.SiteId != CurrentSiteId) return NotFound();
        // Cross-doctor guard: chỉ BS phụ trách lịch (hoặc admin) được hẹn tái khám
        if (u.GroupId != Constants.AdminGroup
            && u.DoctorId.HasValue
            && current.DoctorId.HasValue
            && current.DoctorId.Value != u.DoctorId.Value)
        {
            TempData["Error"] = "Chỉ BS phụ trách lịch mới được hẹn tái khám.";
            return RedirectToAction(nameof(ChanDoan), new { apptId = currentApptId });
        }

        var result = await _apptService.ScheduleFollowUpAsync(
            currentApptId, DateOnly.FromDateTime(followUpDate.Date), session, u.Id);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage ?? "Không thể đặt lịch tái khám.";
            return RedirectToAction(nameof(ChanDoan), new { apptId = currentApptId });
        }
        await _auditService.LogAsync(u.Id, "Hẹn khám lại",
            $"{u.UserName} hẹn tái khám cho {current.PatientName}: {result.FollowUpDate:dd/MM/yyyy} {session}, code={result.BookingCode}");
        TempData["Success"] = $"Đã đặt lịch tái khám {result.FollowUpDate:dd/MM/yyyy} buổi {(session == "morning" ? "sáng" : "chiều")} — mã {result.BookingCode}.";
        return RedirectToAction(nameof(ChanDoan), new { apptId = currentApptId });
    }
}
