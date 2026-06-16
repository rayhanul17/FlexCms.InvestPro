using FlexCms.Framework.Modules;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class CapitalContributionService : LedgerServiceBase<CapitalContribution>
{
    public CapitalContributionService(ModuleActivationOptions opts) : base(opts) { }
    protected override DbSet<CapitalContribution> Set(InvestProDbContext db) => db.CapitalContributions;
    protected override LedgerKind Kind => LedgerKind.Capital;

    protected override void CopyForUpdate(CapitalContribution from, CapitalContribution to)
    {
        to.PartnerId        = from.PartnerId;
        to.Amount           = from.Amount;
        to.Description      = from.Description.Trim();
        to.Details          = from.Details?.Trim();
        to.ContributionType = from.ContributionType;
        to.AssetDescription = from.AssetDescription?.Trim();
        to.PaymentMethod    = from.PaymentMethod;
        to.ReferenceNo      = from.ReferenceNo?.Trim();
        to.Notes            = from.Notes?.Trim();
    }
}
