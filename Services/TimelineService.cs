using FlexCms.Framework.Modules;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

public record TimelineItem(
    LedgerKind Kind,
    Guid Id,
    DateTime TransactionDate,
    DateTime EntryDate,
    decimal Amount,
    string Description,
    string PartnerName,
    string? CategoryName,
    LedgerApprovalStatus ApprovalStatus
);

public record LedgerSummary(
    decimal TotalCapital,
    decimal TotalLabor,
    decimal TotalExpenses,
    decimal TotalRevenue,
    int CapitalCount,
    int LaborCount,
    int ExpenseCount,
    int RevenueCount
)
{
    public decimal NetPosition => TotalRevenue - TotalExpenses;
}

[FcmsScoped]
public class TimelineService
{
    private readonly ModuleActivationOptions _opts;
    public TimelineService(ModuleActivationOptions opts) => _opts = opts;

    private InvestProDbContext OpenDb() =>
        (InvestProDbContext)new InvestProModule().CreateMigrationContext(_opts.ConnectionString, _opts.Provider)!;

    public async Task<LedgerSummary> GetSummaryAsync(Guid investmentId, CancellationToken ct = default)
    {
        await using var db = OpenDb();
        var caps = db.CapitalContributions.Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted && x.ApprovalStatus != LedgerApprovalStatus.Rejected);
        var labs = db.LaborContributions.Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted && x.ApprovalStatus != LedgerApprovalStatus.Rejected);
        var exps = db.Expenses.Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted && x.ApprovalStatus != LedgerApprovalStatus.Rejected);
        var revs = db.Revenues.Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted && x.ApprovalStatus != LedgerApprovalStatus.Rejected);

        return new LedgerSummary(
            TotalCapital:  await caps.SumAsync(x => (decimal?)x.Amount, ct) ?? 0m,
            TotalLabor:    await labs.SumAsync(x => (decimal?)x.Amount, ct) ?? 0m,
            TotalExpenses: await exps.SumAsync(x => (decimal?)x.Amount, ct) ?? 0m,
            TotalRevenue:  await revs.SumAsync(x => (decimal?)x.Amount, ct) ?? 0m,
            CapitalCount:  await caps.CountAsync(ct),
            LaborCount:    await labs.CountAsync(ct),
            ExpenseCount:  await exps.CountAsync(ct),
            RevenueCount:  await revs.CountAsync(ct)
        );
    }

    public async Task<List<TimelineItem>> GetTimelineAsync(Guid investmentId,
        LedgerKind? filterKind = null, string? search = null, CancellationToken ct = default)
    {
        await using var db = OpenDb();
        var search_l = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLower();

        var items = new List<TimelineItem>();

        if (filterKind is null or LedgerKind.Capital)
        {
            var rows = await db.CapitalContributions
                .Include(x => x.Partner)
                .Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted)
                .ToListAsync(ct);
            items.AddRange(rows.Select(r => new TimelineItem(
                LedgerKind.Capital, r.Id, r.TransactionDate, r.EntryDate, r.Amount, r.Description,
                r.Partner?.Name ?? "—", null, r.ApprovalStatus)));
        }

        if (filterKind is null or LedgerKind.Labor)
        {
            var rows = await db.LaborContributions
                .Include(x => x.Partner)
                .Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted)
                .ToListAsync(ct);
            items.AddRange(rows.Select(r => new TimelineItem(
                LedgerKind.Labor, r.Id, r.TransactionDate, r.EntryDate, r.Amount, r.Description,
                r.Partner?.Name ?? "—", r.TaskType, r.ApprovalStatus)));
        }

        if (filterKind is null or LedgerKind.Expense)
        {
            var rows = await db.Expenses
                .Include(x => x.Partner)
                .Include(x => x.Category)
                .Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted)
                .ToListAsync(ct);
            items.AddRange(rows.Select(r => new TimelineItem(
                LedgerKind.Expense, r.Id, r.TransactionDate, r.EntryDate, r.Amount, r.Description,
                r.Partner?.Name ?? "—", r.Category?.Name, r.ApprovalStatus)));
        }

        if (filterKind is null or LedgerKind.Revenue)
        {
            var rows = await db.Revenues
                .Include(x => x.Partner)
                .Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted)
                .ToListAsync(ct);
            items.AddRange(rows.Select(r => new TimelineItem(
                LedgerKind.Revenue, r.Id, r.TransactionDate, r.EntryDate, r.Amount, r.Description,
                r.Partner?.Name ?? "—", r.SourceType.ToString(), r.ApprovalStatus)));
        }

        if (search_l is not null)
            items = items.Where(i =>
                i.Description.ToLower().Contains(search_l) ||
                i.PartnerName.ToLower().Contains(search_l) ||
                (i.CategoryName?.ToLower().Contains(search_l) ?? false)
            ).ToList();

        return items.OrderByDescending(i => i.TransactionDate)
                    .ThenByDescending(i => i.EntryDate)
                    .ToList();
    }
}
