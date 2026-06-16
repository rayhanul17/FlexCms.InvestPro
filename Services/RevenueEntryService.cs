using FlexCms.Framework.Modules;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class RevenueEntryService : LedgerServiceBase<Revenue>
{
    public RevenueEntryService(ModuleActivationOptions opts) : base(opts) { }
    protected override DbSet<Revenue> Set(InvestProDbContext db) => db.Revenues;
    protected override LedgerKind Kind => LedgerKind.Revenue;

    protected override void CopyForUpdate(Revenue from, Revenue to)
    {
        to.PartnerId    = from.PartnerId;
        to.Amount       = from.Amount;
        to.Description  = from.Description.Trim();
        to.Details      = from.Details?.Trim();
        to.SourceType   = from.SourceType;
        to.Customer     = from.Customer?.Trim();
        to.SalesChannel = from.SalesChannel?.Trim();
        to.InvoiceNo    = from.InvoiceNo?.Trim();
        to.Notes        = from.Notes?.Trim();
    }
}
