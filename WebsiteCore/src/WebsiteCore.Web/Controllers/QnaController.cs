using Microsoft.AspNetCore.Mvc;
using WebsiteCore.Business.Services;

namespace WebsiteCore.Web.Controllers;

public class QnaController : BaseController
{
    private readonly IQnaService _qnaService;

    public QnaController(
        ISiteService siteService,
        IQnaService qnaService) : base(siteService)
    {
        _qnaService = qnaService;
    }

    [HttpGet]
    public IActionResult DatCauHoi()
    {
        ViewBag.Title = "Đặt câu hỏi cho bác sĩ";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatCauHoi(string qa_title, string qa_body, string? qa_topic, string? qa_public)
    {
        ViewBag.Title = "Đặt câu hỏi cho bác sĩ";
        var u = CurrentUser;
        if (u == null)
        {
            ViewBag.Error = "Bạn cần đăng nhập tài khoản bệnh nhân để gửi câu hỏi.";
            return View();
        }
        if (string.IsNullOrWhiteSpace(qa_title) || string.IsNullOrWhiteSpace(qa_body))
        {
            ViewBag.Error = "Vui lòng nhập tiêu đề và nội dung câu hỏi.";
            return View();
        }

        await _qnaService.CreateQuestionAsync(u.Id, qa_title, qa_body, qa_topic, qa_public == "1", CurrentSiteId);
        TempData["Success"] = "Đã gửi câu hỏi. Bác sĩ sẽ duyệt và trả lời sớm nhất có thể.";
        return Redirect("~/cau-hoi-cua-toi");
    }

    [HttpGet]
    public async Task<IActionResult> CauHoiCuaToi()
    {
        var u = CurrentUser;
        if (u == null) return Redirect("~/dang-nhap?returnUrl=/cau-hoi-cua-toi");
        ViewBag.Title = "Câu hỏi của tôi";
        var list = await _qnaService.GetByPatientAsync(u.Id);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> HoiDap()
    {
        ViewBag.Title = "Hỏi đáp y tế";
        var list = await _qnaService.GetPublicAnsweredAsync(CurrentSiteId);
        ViewBag.Answers = new Dictionary<Guid, WebsiteCore.Data.Entities.Answer?>();
        var dict = (Dictionary<Guid, WebsiteCore.Data.Entities.Answer?>)ViewBag.Answers;
        foreach (var q in list)
        {
            dict[q.Id] = await _qnaService.GetAnswerForQuestionAsync(q.Id);
        }
        return View(list);
    }
}
