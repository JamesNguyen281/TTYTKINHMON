using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebsiteCore.Business;
using WebsiteCore.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor runtime compilation (cho dev sửa view không cần build lại)
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Cho phép Razor render Vietnamese trực tiếp (không encode "Đặt lịch" thành "&#x110;&#x1EB7;t l&#x1ECB;ch")
// — vẫn an toàn vì Razor vẫn HTML-encode characters đặc biệt như < > & " '.
builder.Services.Configure<WebEncoderOptions>(opt =>
{
    opt.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

// Business + Data layer (DbContext + services)
builder.Services.AddBusinessServices(builder.Configuration.GetConnectionString("Default"));

// Session — dùng cho USER session, locate, etc.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name = ".TtytKinhMon.Session";
    // H4 + L1: cookie security
    opt.Cookie.SameSite = SameSiteMode.Lax; // Strict gây vỡ flow login (POST cross-site)
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS only nếu host HTTPS
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<VisitorCounter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// L4: Security headers — CSP + frame options + nosniff + referrer policy
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    if (!h.ContainsKey("X-Content-Type-Options")) h["X-Content-Type-Options"] = "nosniff";
    if (!h.ContainsKey("X-Frame-Options"))        h["X-Frame-Options"] = "SAMEORIGIN";
    if (!h.ContainsKey("Referrer-Policy"))        h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    if (!h.ContainsKey("Content-Security-Policy"))
    {
        // CSP cho phép 'unsafe-inline' style/script (Razor view sinh inline) + CDN cho CKEditor + FontAwesome + Google Fonts
        h["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.ckeditor.com https://code.jquery.com https://cdnjs.cloudflare.com; " +
            "style-src  'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
            "font-src   'self' data: https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
            "img-src    'self' data: https: http:; " +
            "frame-src  https://www.google.com https://www.youtube.com https://www.youtube-nocookie.com; " +
            "connect-src 'self'; " +
            "frame-ancestors 'self'; " +
            "form-action 'self';";
    }
    await next();
});

// Static files (wwwroot/) phải được serve cho CSS/JS/ảnh
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseMiddleware<VisitorCounterMiddleware>();
app.UseMiddleware<ForcePasswordChangeMiddleware>();

// SEO-friendly Vietnamese routes — phải đăng ký TRƯỚC route default
app.MapControllerRoute("DangNhap",  "dang-nhap",          new { controller = "Auth",  action = "DangNhap" });
app.MapControllerRoute("DangKy",    "dang-ky",            new { controller = "Auth",  action = "DangKy" });
app.MapControllerRoute("DangXuat",  "dang-xuat",          new { controller = "Auth",  action = "DangXuat" });
app.MapControllerRoute("HoSo",      "ho-so",              new { controller = "MyAccount", action = "Index" });
app.MapControllerRoute("DatLich",   "dat-lich-kham",      new { controller = "Appointment", action = "DatLichKham" });
app.MapControllerRoute("LichToi",   "lich-cua-toi",       new { controller = "Appointment", action = "LichCuaToi" });
app.MapControllerRoute("LichSu",    "lich-su-kham",       new { controller = "MyAccount", action = "LichSuKham" });
app.MapControllerRoute("DatCH",     "dat-cau-hoi",        new { controller = "Qna",   action = "DatCauHoi" });
app.MapControllerRoute("CHToi",     "cau-hoi-cua-toi",    new { controller = "Qna",   action = "CauHoiCuaToi" });
app.MapControllerRoute("HoiDap",    "hoi-dap",            new { controller = "Qna",   action = "HoiDap" });
app.MapControllerRoute("BacSi",     "bac-si",             new { controller = "Home",  action = "DoctorList" });
app.MapControllerRoute("VanBan",    "van-ban",            new { controller = "DocumentClient", action = "Index" });
app.MapControllerRoute("TimKiem",   "tim-kiem",           new { controller = "Home", action = "Search" });
app.MapControllerRoute("LichTruc",  "lich-truc",          new { controller = "Home", action = "WorkingDoctors" });

// === Cổng Lễ tân (RECEPTION) ===
app.MapControllerRoute("LeTanIndex",      "le-tan",                  new { controller = "LeTan", action = "Index" });
app.MapControllerRoute("LeTanAppts",      "le-tan/lich-hen",         new { controller = "LeTan", action = "Appointments" });
app.MapControllerRoute("LeTanDetail",     "le-tan/lich-hen/{id}",    new { controller = "LeTan", action = "Detail" });
app.MapControllerRoute("LeTanUpdate",     "le-tan/cap-nhat-trang-thai", new { controller = "LeTan", action = "UpdateStatus" });
app.MapControllerRoute("LeTanAssignDoc",  "le-tan/phan-bac-si",      new { controller = "LeTan", action = "AssignDoctor" });
app.MapControllerRoute("LeTanCheckIn",    "le-tan/check-in",         new { controller = "LeTan", action = "CheckIn" });
app.MapControllerRoute("LeTanMark",       "le-tan/xac-nhan-check-in",new { controller = "LeTan", action = "MarkCheckedIn" });
app.MapControllerRoute("LeTanDuty",        "le-tan/bac-si-truc",      new { controller = "LeTan", action = "DoctorsOnDuty" });
app.MapControllerRoute("LeTanQuotas",      "le-tan/suat-kham",        new { controller = "LeTan", action = "Quotas" });
app.MapControllerRoute("LeTanQuotaSet",    "le-tan/dat-suat",         new { controller = "LeTan", action = "SetQuotaMax" });
app.MapControllerRoute("LeTanCounts",      "le-tan/counts",           new { controller = "LeTan", action = "Counts" });

// === Cổng Bác sĩ (DOCTOR) ===
app.MapControllerRoute("DocPortal",       "bac-si-portal",                     new { controller = "DoctorPortal", action = "Index" });
app.MapControllerRoute("DocBNHomNay",     "bac-si-portal/benh-nhan-hom-nay",   new { controller = "DoctorPortal", action = "BenhNhanHomNay" });
app.MapControllerRoute("DocAnswer",       "bac-si-portal/duyet-cau-hoi/{id}",  new { controller = "DoctorPortal", action = "DuyetCauHoi" });
app.MapControllerRoute("DocAnswerSubmit", "bac-si-portal/answer",              new { controller = "DoctorPortal", action = "AnswerQuestion" });
app.MapControllerRoute("DocChanDoan",     "bac-si-portal/chan-doan/{apptId}",  new { controller = "DoctorPortal", action = "ChanDoan" });
app.MapControllerRoute("DocCounts",       "bac-si-portal/counts",              new { controller = "DoctorPortal", action = "TodayCounts" });
app.MapControllerRoute("DocLichTruc",     "bac-si-portal/lich-truc",           new { controller = "DoctorPortal", action = "LichTruc" });
app.MapControllerRoute("DocYeuCauDL",     "bac-si-portal/yeu-cau-doi-lich",    new { controller = "DoctorPortal", action = "YeuCauDoiLich" });
app.MapControllerRoute("ChuyenKhoa","chuyen-khoa",        new { controller = "Home",  action = "DepartmentList" });
app.MapControllerRoute("NewList",   "chuyen-muc/{alias}", new { controller = "Home",  action = "NewList" });
app.MapControllerRoute("NewDetail", "tin-tuc/{alias}",    new { controller = "Home",  action = "NewDetail" });
app.MapControllerRoute("TinTuc",    "tin-tuc",            new { controller = "Home",  action = "AllNews" });
app.MapControllerRoute("LienHe",    "lien-he",            new { controller = "Home",  action = "Contact" });

// SEO redirects — chuyển bookmark/URL `/Home/<action>` về URL có dấu thân thiện
app.MapGet("/Home/DatCauHoi", () => Results.Redirect("/dat-cau-hoi", permanent: true));
app.MapGet("/Home/HoiDap",    () => Results.Redirect("/hoi-dap",    permanent: true));
app.MapGet("/Home/CauHoiCuaToi", () => Results.Redirect("/cau-hoi-cua-toi", permanent: true));
app.MapGet("/Home/LichCuaToi",   () => Results.Redirect("/lich-cua-toi",   permanent: true));
app.MapGet("/Home/Index",        () => Results.Redirect("/", permanent: true));
app.MapGet("/Home/Contact",      () => Results.Redirect("/lien-he",       permanent: true));
app.MapGet("/Home/DoctorList",   () => Results.Redirect("/bac-si",        permanent: true));
app.MapGet("/Home/Search",  ctx => { var q = ctx.Request.Query["q"]; ctx.Response.Redirect("/tim-kiem" + (string.IsNullOrEmpty(q) ? "" : "?q=" + q), permanent: true); return Task.CompletedTask; });

// Area routes (AdminCP cho cán bộ)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
