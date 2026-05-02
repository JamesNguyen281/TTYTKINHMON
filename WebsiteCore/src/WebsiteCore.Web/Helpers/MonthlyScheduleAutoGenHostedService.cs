using WebsiteCore.Business.Services;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Background service tự sinh lịch trực BS cho tháng kế tiếp khi đến ngày 28 hàng tháng.
/// Chạy mỗi 1 giờ kiểm tra một lần. Idempotent — service.GenerateMonthlyScheduleAsync()
/// tự skip BS đã có lịch tháng đó nên dù nhỡ chạy nhiều lần cũng không tạo trùng.
///
/// Tại sao ngày 28? — phòng tháng có 28/29/30/31 ngày. 28 là chung cho mọi tháng,
/// thực thi sau ngày 28 thì luôn có thời gian ≥ 0 trước khi tháng mới bắt đầu.
/// </summary>
public class MonthlyScheduleAutoGenHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyScheduleAutoGenHostedService> _logger;
    private const int TriggerDayOfMonth = 28;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public MonthlyScheduleAutoGenHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonthlyScheduleAutoGenHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonthlyScheduleAutoGen started — checking every {Hours}h, trigger on day {Day}", CheckInterval.TotalHours, TriggerDayOfMonth);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Service tự catch — không cho exception giết hosted service. Log + tiếp tục.
                _logger.LogError(ex, "MonthlyScheduleAutoGen tick failed");
            }

            try { await Task.Delay(CheckInterval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        var today = DateTime.Today;
        if (today.Day < TriggerDayOfMonth) return;

        var nextMonth = today.AddMonths(1);
        int targetYear = nextMonth.Year, targetMonth = nextMonth.Month;

        using var scope = _scopeFactory.CreateScope();
        var schedSvc = scope.ServiceProvider.GetRequiredService<IDoctorScheduleService>();
        var siteSvc  = scope.ServiceProvider.GetRequiredService<ISiteService>();
        var auditSvc = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var sites = await siteSvc.GetAllAsync();
        foreach (var site in sites)
        {
            ct.ThrowIfCancellationRequested();
            // Per-site try-catch — 1 site fail không block các site khác trong cùng tick.
            try
            {
                var rs = await schedSvc.GenerateMonthlyScheduleAsync(targetYear, targetMonth, site.Id, createdBy: null);
                if (rs.Created == 0)
                {
                    _logger.LogInformation("Auto-gen site {SiteId} {Month:00}/{Year}: nothing new (skipped {Skipped} BS)", site.Id, targetMonth, targetYear, rs.SkippedExisting);
                    continue;
                }
                _logger.LogInformation("Auto-gen site {SiteId} {Month:00}/{Year}: created {Created} schedules for {Doctors} BS", site.Id, targetMonth, targetYear, rs.Created, rs.DoctorsProcessed - rs.SkippedExisting);
                // userId=null vì là background, không có CurrentUser
                await auditSvc.LogAsync(null, "Auto-gen lịch trực tháng (cron)",
                    $"system day={today:yyyy-MM-dd} site={site.Id} target={targetMonth:00}/{targetYear} created={rs.Created} skipped={rs.SkippedExisting} processed={rs.DoctorsProcessed}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-gen failed for site {SiteId} target {Month:00}/{Year} — continuing with next site", site.Id, targetMonth, targetYear);
            }
        }
    }
}
