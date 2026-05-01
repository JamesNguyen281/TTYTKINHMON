using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IQnaService
{
    Task<Guid> CreateQuestionAsync(Guid patientUserId, string title, string body, string? topic, bool isPublic, Guid siteId);
    Task<List<Question>> GetByPatientAsync(Guid patientUserId);
    Task<List<Question>> GetPendingAsync(int take = 30);
    Task<List<Question>> GetPublicAnsweredAsync(Guid siteId, int take = 50);
    Task<Question?> GetByIdAsync(Guid id);
    Task<Answer?> GetAnswerForQuestionAsync(Guid questionId);
    Task AnswerAsync(Guid questionId, Guid doctorUserId, string body);
}

public class QnaService : IQnaService
{
    private readonly TtytlpDbContext _db;
    public QnaService(TtytlpDbContext db) => _db = db;

    public async Task<Guid> CreateQuestionAsync(Guid patientUserId, string title, string body, string? topic, bool isPublic, Guid siteId)
    {
        var q = new Question
        {
            Id            = Guid.NewGuid(),
            PatientUserId = patientUserId,
            Title         = title.Trim(),
            Body          = body.Trim(),
            Topic         = topic,
            IsPublic      = isPublic,
            Status        = "pending",
            SiteId        = siteId,
            CreatedDate   = DateTime.Now
        };
        _db.Questions.Add(q);
        await _db.SaveChangesAsync();
        return q.Id;
    }

    public Task<List<Question>> GetByPatientAsync(Guid patientUserId) =>
        _db.Questions
            .Where(q => q.PatientUserId == patientUserId)
            .OrderByDescending(q => q.CreatedDate)
            .ToListAsync();

    public Task<List<Question>> GetPendingAsync(int take = 30) =>
        _db.Questions
            .Where(q => q.Status == "pending")
            .OrderBy(q => q.CreatedDate)
            .Take(take)
            .ToListAsync();

    public Task<List<Question>> GetPublicAnsweredAsync(Guid siteId, int take = 50) =>
        _db.Questions
            .Where(q => q.Status == "answered"
                     && q.IsPublic == true
                     && (q.SiteId == siteId || q.SiteId == null))
            .OrderByDescending(q => q.LuUpdated ?? q.CreatedDate)
            .Take(take)
            .ToListAsync();

    public Task<Question?> GetByIdAsync(Guid id) =>
        _db.Questions.FirstOrDefaultAsync(q => q.Id == id);

    public Task<Answer?> GetAnswerForQuestionAsync(Guid questionId) =>
        _db.Answers.FirstOrDefaultAsync(a => a.QuestionId == questionId);

    public async Task AnswerAsync(Guid questionId, Guid doctorUserId, string body)
    {
        var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == questionId);
        if (q == null) return;
        // M6: Idempotent — không cho trả lời 2 lần cùng câu hỏi
        if (q.Status == "answered") return;

        var ans = new Answer
        {
            Id           = Guid.NewGuid(),
            QuestionId   = questionId,
            DoctorUserId = doctorUserId,
            // H1: Sanitize HTML chống XSS stored — body render qua Html.Raw ở /hoi-dap public
            Body         = Helpers.StringHelper.SanitizeHtml(body.Trim()),
            CreatedDate  = DateTime.Now
        };
        _db.Answers.Add(ans);
        q.Status    = "answered";
        q.LuUpdated = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
