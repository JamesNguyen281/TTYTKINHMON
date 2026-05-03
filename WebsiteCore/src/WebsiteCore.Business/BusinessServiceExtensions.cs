using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebsiteCore.Business.Services;
using WebsiteCore.Data;

namespace WebsiteCore.Business;

/// <summary>
/// DI extension — gọi từ Web/Program.cs:
/// <code>
/// builder.Services.AddBusinessServices(builder.Configuration.GetConnectionString("Default"));
/// </code>
/// </summary>
public static class BusinessServiceExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, string? connectionString)
    {
        // CompatibilityLevel(120) = SQL 2014 — DB ttytlp đang ở compat 120 nên EF Core 8
        // KHÔNG được dùng OPENJSON (compat ≥ 130) cho Contains/IN, nếu không sẽ sinh SQL
        // dùng JSON path '$' gây lỗi parser. Config này buộc EF dùng IN list cũ.
        services.AddDbContext<TtytlpDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(120)));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IQnaService, QnaService>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISlideService, SlideService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IQuotaService, QuotaService>();
        services.AddScoped<IScheduleChangeRequestService, ScheduleChangeRequestService>();
        services.AddScoped<IClinicRoomService, ClinicRoomService>();
        return services;
    }
}
