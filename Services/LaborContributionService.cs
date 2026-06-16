using FlexCms.Framework.Modules;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class LaborContributionService : LedgerServiceBase<LaborContribution>
{
    public LaborContributionService(ModuleActivationOptions opts) : base(opts) { }
    protected override DbSet<LaborContribution> Set(InvestProDbContext db) => db.LaborContributions;
    protected override LedgerKind Kind => LedgerKind.Labor;

    protected override void CopyForUpdate(LaborContribution from, LaborContribution to)
    {
        to.PartnerId       = from.PartnerId;
        to.UnitType        = from.UnitType;
        to.HoursOrDays     = from.HoursOrDays;
        to.RatePerUnit     = from.RatePerUnit;
        to.Amount          = from.HoursOrDays * from.RatePerUnit;
        to.TaskType        = from.TaskType?.Trim();
        to.WorkDescription = from.WorkDescription?.Trim();
        to.Description     = from.Description.Trim();
        to.Details         = from.Details?.Trim();
        to.Notes           = from.Notes?.Trim();
    }

    public override async Task<(bool ok, string? error, LaborContribution? saved)> CreateAsync(Guid investmentId, LaborContribution model, CancellationToken ct = default)
    {
        model.Amount = model.HoursOrDays * model.RatePerUnit;
        return await base.CreateAsync(investmentId, model, ct);
    }
}
