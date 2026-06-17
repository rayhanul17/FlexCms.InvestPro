using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class ExpenseEntryService : LedgerServiceBase<Expense>
{
    public ExpenseEntryService(InvestProDbContext db) : base(db) { }
    protected override DbSet<Expense> Set(InvestProDbContext db) => db.Expenses;
    protected override LedgerKind Kind => LedgerKind.Expense;

    public override Task<List<Expense>> GetByInvestmentAsync(Guid investmentId, CancellationToken ct = default)
        => Db.Expenses
            .Include(x => x.Partner)
            .Include(x => x.Category)
            .Where(x => x.InvestmentId == investmentId && x.Status != EntityStatus.Deleted)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync(ct);

    protected override void CopyForUpdate(Expense from, Expense to)
    {
        to.PartnerId         = from.PartnerId;
        to.Amount            = from.Amount;
        to.Description       = from.Description.Trim();
        to.Details           = from.Details?.Trim();
        to.ExpenseCategoryId = from.ExpenseCategoryId;
        to.PaidTo            = from.PaidTo?.Trim();
        to.PaymentMethod     = from.PaymentMethod;
        to.ReceiptNo         = from.ReceiptNo?.Trim();
        to.Notes             = from.Notes?.Trim();
    }
}
