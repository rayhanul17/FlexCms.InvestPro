using FlexCms.Framework.Auth;
using FlexCms.InvestPro.Data;
using FlexCms.InvestPro.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlexCms.InvestPro.Controllers;

[Route("investpro/admin/investments/{investmentId:guid}/timeline")]
[FcmsAuthorize(InvestProPermissions.LedgerView)]
public class TimelineController : Controller
{
    private readonly TimelineService _timeline;
    private readonly InvestmentService _investments;

    public TimelineController(TimelineService timeline, InvestmentService investments)
    {
        _timeline = timeline;
        _investments = investments;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid investmentId, LedgerKind? kind, string? q, CancellationToken ct)
    {
        var inv = await _investments.GetByIdAsync(investmentId, ct);
        if (inv is null) return NotFound();
        var items = await _timeline.GetTimelineAsync(investmentId, kind, q, ct);
        var summary = await _timeline.GetSummaryAsync(investmentId, ct);

        ViewData["Investment"] = inv;
        ViewData["FilterKind"] = kind;
        ViewData["SearchQuery"] = q;
        ViewData["Summary"] = summary;
        return View(items);
    }
}
