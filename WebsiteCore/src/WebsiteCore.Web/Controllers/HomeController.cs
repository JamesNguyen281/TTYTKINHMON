using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebsiteCore.Web.Controllers;

public class HomeController : BaseController
{
    private readonly IDepartmentService _deptService;
    private readonly INewsService _newsService;
    private readonly ICategoryService _categoryService;
    private readonly IDoctorService _doctorService;
    private readonly IVideoService _videoService;
    private readonly IDoctorScheduleService _scheduleService;
    private readonly IQuotaService _quotaService;
    private readonly TtytlpDbContext _db;

    public HomeController(
        ISiteService siteService,
        IDepartmentService deptService,
        INewsService newsService,
        ICategoryService categoryService,
        IDoctorService doctorService,
        IVideoService videoService,
        IDoctorScheduleService scheduleService,
        IQuotaService quotaService,
        TtytlpDbContext db) : base(siteService)
    {
        _deptService = deptService;
        _newsService = newsService;
        _categoryService = categoryService;
        _doctorService = doctorService;
        _videoService = videoService;
        _scheduleService = scheduleService;
        _quotaService = quotaService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Departments        = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.OutstandingServices = await _newsService.GetOutstandingServicesAsync(CurrentSiteId);
        ViewBag.FeaturedNews       = await _newsService.GetTopAsync(CurrentSiteId);
        ViewBag.SlideBox           = await _categoryService.GetSlideBoxAsync(CurrentSiteId);
        ViewBag.SlideTextBox       = await _categoryService.GetSlideTextBoxAsync(CurrentSiteId);
        ViewBag.Videos             = await _videoService.GetForHomeAsync(CurrentSiteId);
        ViewBag.DoctorIn           = await _doctorService.GetForHomeAsync(CurrentSiteId, isPartner: true);
        ViewBag.DoctorForeign      = await _doctorService.GetForHomeAsync(CurrentSiteId, isPartner: false);
        ViewBag.Title              = "Trung tâm Y tế phường Kinh Môn";
        return View();
    }

    [Route("tim-kiem")]
    [Route("Home/Search")]
    public async Task<IActionResult> Search(string? q, int page = 1, int pageSize = 10)
    {
        ViewBag.Title = string.IsNullOrEmpty(q) ? "Tìm kiếm" : $"Kết quả: {q}";
        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        var newsList = new List<News>();
        var deptList = new List<Department>();
        var doctorList = new List<Doctor>();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            var pattern = "%" + k + "%";
            newsList = await _db.News
                .Where(n => n.ActiveFlag == 1 && (n.SiteId == CurrentSiteId || n.SiteId == null)
                         && (EF.Functions.Like(n.TitleL ?? "", pattern)
                          || EF.Functions.Like(n.DescriptionL ?? "", pattern)))
                .OrderByDescending(n => n.CreatedDate)
                .Take(50).ToListAsync();
            var kLower = k.ToLower();
            deptList = (await _deptService.GetActiveBySiteAsync(CurrentSiteId))
                .Where(d => (d.NameL ?? "").ToLower().Contains(kLower)).ToList();
            doctorList = await _doctorService.SearchAsync(CurrentSiteId, k, 1, 20);
        }
        ViewBag.News = newsList;
        ViewBag.Departments = deptList;
        ViewBag.Doctors = doctorList;
        return View();
    }

    public async Task<IActionResult> DepartmentList()
    {
        var depts = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.Title = "Các chuyên khoa";
        return View(depts);
    }

    public async Task<IActionResult> DoctorList(string? q = null, string? d = null)
    {
        var dean    = await _doctorService.GetForHomeAsync(CurrentSiteId, isPartner: true,  100);
        var manager = await _doctorService.GetForHomeAsync(CurrentSiteId, isPartner: false, 100);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.ToLower();
            dean    = dean.Where(x => (x.NameL ?? "").ToLower().Contains(k)).ToList();
            manager = manager.Where(x => (x.NameL ?? "").ToLower().Contains(k)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(d))
        {
            var dept = (await _deptService.GetActiveBySiteAsync(CurrentSiteId)).FirstOrDefault(x => x.Alias == d);
            if (dept != null)
            {
                dean    = dean.Where(x => x.DepartmentId == dept.Id).ToList();
                manager = manager.Where(x => x.DepartmentId == dept.Id).ToList();
            }
        }
        ViewBag.DoctorDean    = dean;
        ViewBag.DoctorManager = manager;
        ViewBag.AllDepartment = await _deptService.GetActiveBySiteAsync(CurrentSiteId);
        ViewBag.Q = q;
        ViewBag.D = d;
        ViewBag.Title = "Đội ngũ bác sĩ";
        return View();
    }

    [Route("lich-truc")]
    [Route("Home/WorkingDoctors")]
    public async Task<IActionResult> WorkingDoctors()
    {
        ViewBag.Title = "Lịch làm việc bác sĩ";
        ViewBag.Schedules = await _scheduleService.GetAllActiveAsync();
        ViewBag.Doctors = await _doctorService.GetAllAsync(CurrentSiteId);
        return View();
    }

    [Route("tin-tuc")]
    public IActionResult AllNews() => Redirect("/chuyen-muc/tin-tuc");

    [Route("lien-he")]
    [Route("Home/Contact")]
    public async Task<IActionResult> Contact()
    {
        var site = ViewBag.Site as Site;
        ViewBag.Title = "Liên hệ";
        ViewBag.MetaDescription = "Liên hệ Trung tâm Y tế phường Kinh Môn — địa chỉ, số điện thoại, email, hộp thư góp ý.";
        ViewBag.Site = site ?? await _db.Sites.FirstOrDefaultAsync(s => s.ActiveFlag == 1);
        return View();
    }

    [HttpPost]
    [Route("lien-he/gui-gop-y")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactSubmit(string fullName, string email, string phone, string subject, string content)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(content))
        {
            TempData["ContactErr"] = "Vui lòng nhập đầy đủ họ tên và nội dung.";
            return RedirectToAction(nameof(Contact));
        }
        var phoneStr = string.IsNullOrWhiteSpace(phone) ? "" : $" • SĐT: {phone.Trim()}";
        var subjectStr = string.IsNullOrWhiteSpace(subject) ? "" : $"[{subject.Trim()}] ";
        var c = new Comment
        {
            Id          = Guid.NewGuid(),
            UserName    = fullName.Trim(),
            Email       = email?.Trim(),
            Message     = subjectStr + content.Trim() + phoneStr,
            CreatedDate = DateTime.Now,
            ActiveFlag  = 0
        };
        _db.Comments.Add(c);
        await _db.SaveChangesAsync();
        TempData["ContactOk"] = "Cảm ơn bạn đã góp ý. Chúng tôi sẽ phản hồi sớm nhất.";
        return RedirectToAction(nameof(Contact));
    }

    public async Task<IActionResult> NewList(string alias, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrEmpty(alias)) return NotFound();
        var category = await _categoryService.GetByAliasAsync(alias);
        var list = await _newsService.GetByCategoryAliasAsync(CurrentSiteId, alias, page, pageSize);

        // 1 bài duy nhất → redirect về chi tiết
        if (list.Count == 1 && page == 1)
            return Redirect("/tin-tuc/" + list[0].AliasL);

        // Sub-categories (con của category này) + related departments để bổ sung nội dung
        var subCats = category != null
            ? await _categoryService.GetChildrenAsync(category.Id)
            : new List<Category>();
        var relatedDepts = (await _deptService.GetActiveBySiteAsync(CurrentSiteId))
            .Where(d => (d.Alias ?? "").Contains(alias) || alias.Contains(d.Alias ?? "_"))
            .ToList();

        ViewBag.Alias = alias;
        ViewBag.Category = category;
        ViewBag.SubCategories = subCats;
        ViewBag.RelatedDepartments = relatedDepts;
        ViewBag.Title = category?.NameL ?? "Chuyên mục";
        ViewBag.MetaDescription = category?.DescriptionL;
        return View(list);
    }

    public async Task<IActionResult> NewDetail(string alias)
    {
        if (string.IsNullOrEmpty(alias)) return NotFound();
        var n = await _newsService.GetByAliasAsync(alias);
        if (n == null) return NotFound();
        ViewBag.Title = n.TitleL;
        return View(n);
    }

    [Route("chuyen-khoa/{alias}")]
    public async Task<IActionResult> NewForDepartment(string alias, int page = 1, int pageSize = 10)
    {
        var dept = (await _deptService.GetActiveBySiteAsync(CurrentSiteId))
            .FirstOrDefault(d => d.Alias == alias);
        if (dept == null) return NotFound();
        ViewBag.Department = dept;
        ViewBag.Title = dept.NameL;
        ViewBag.Page = page;
        var newsList = await _db.News
            .Where(n => n.DepartmentId == dept.Id && n.ActiveFlag == 1)
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.NewsList = newsList;
        var doctors = (await _doctorService.GetAllAsync(CurrentSiteId))
            .Where(d => d.DepartmentId == dept.Id).ToList();
        ViewBag.Doctors = doctors;
        return View(dept);
    }

    [HttpGet]
    [Route("Home/DoctorDetail")]
    public async Task<IActionResult> DoctorDetail(Guid id)
    {
        var d = await _doctorService.GetByIdAsync(id);
        if (d == null) return NotFound();
        var dept = d.DepartmentId.HasValue
            ? (await _deptService.GetActiveBySiteAsync(CurrentSiteId)).FirstOrDefault(x => x.Id == d.DepartmentId)
            : null;
        bool isEn = Locate_Client == "en";
        string Pick(string? l, string? e) => isEn ? (string.IsNullOrEmpty(e) ? (l ?? "") : e) : (l ?? "");

        // Lịch trực — chỉ ca active đang phủ tháng hiện tại để bệnh nhân biết khi nào bs làm việc
        var todayD = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth = new DateOnly(todayD.Year, todayD.Month, 1);
        var lastOfMonth  = firstOfMonth.AddMonths(1).AddDays(-1);
        var allSchedules = await _scheduleService.GetByDoctorAsync(d.Id);
        var schedules = allSchedules
            .Where(s => s.ValidFrom <= lastOfMonth && (s.ValidTo == null || s.ValidTo >= firstOfMonth))
            .OrderBy(s => s.Weekday).ThenBy(s => s.Session)
            .Select(s => new {
                weekday      = (int)s.Weekday,
                weekday_name = s.Weekday switch {
                    1 => "Chủ Nhật", 2 => "Thứ Hai", 3 => "Thứ Ba", 4 => "Thứ Tư",
                    5 => "Thứ Năm", 6 => "Thứ Sáu", 7 => "Thứ Bảy", _ => $"WD{s.Weekday}"
                },
                session       = s.Session,
                session_label = s.Session == "morning" ? "Sáng" : "Chiều",
                room          = s.Room,
                max_patients  = s.MaxPatients ?? 0,
                valid_from    = s.ValidFrom.ToString("yyyy-MM-dd"),
                valid_to      = s.ValidTo?.ToString("yyyy-MM-dd")
            })
            .ToList();

        return Json(new
        {
            id              = d.Id,
            name            = Pick(d.NameL, d.NameE),
            specially       = Pick(d.SpeciallyL, d.SpeciallyE),
            quantification  = Pick(d.QuantificationL, d.QuantificationE),
            experiences     = Pick(d.ExperiencesL, d.ExperiencesE),
            interests       = Pick(d.SpeciallyInterestsL, d.SpeciallyInterestsE),
            timetable       = Pick(d.TimetableL, d.TimetableE),
            position        = d.Position,
            image_path      = d.ImagePath,
            department_name = Pick(dept?.NameL, dept?.NameE),
            cycle_label     = $"{firstOfMonth:MM/yyyy}",
            schedules       = schedules
        });
    }

    [HttpGet]
    [Route("Home/GetAvailability")]
    public async Task<IActionResult> GetAvailability(Guid departmentId, DateTime date, string session)
    {
        var d = DateOnly.FromDateTime(date);
        var quota = await _quotaService.GetOrCreateAsync(departmentId, d, session);
        return Json(new
        {
            date    = date.ToString("yyyy-MM-dd"),
            session,
            max     = quota?.MaxCount ?? 0,
            booked  = quota?.BookedCount ?? 0,
            remain  = (quota?.MaxCount ?? 0) - (quota?.BookedCount ?? 0)
        });
    }

    [HttpGet]
    [Route("Home/ListFeaturedNewsPaging")]
    public async Task<IActionResult> ListFeaturedNewsPaging(int page = 1, int pageSize = 5)
    {
        var all = await _newsService.GetTopAsync(CurrentSiteId, 100);
        var totalPages = (int)Math.Ceiling((double)all.Count / pageSize);
        var data = all.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new {
                title       = n.TitleL,
                description = n.DescriptionL,
                link        = "tin-tuc/" + n.AliasL,
                image_path  = n.ImagePath
            });
        return Json(new { data, TotalPages = totalPages, CurrentPage = page });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
