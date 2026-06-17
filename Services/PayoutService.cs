using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class PayoutService
{
    private readonly InvestProDbContext _db;
    public PayoutService(InvestProDbContext db) => _db = db;

    public Task<List<Payout>> GetBySnapshotAsync(Guid snapshotId, CancellationToken ct = default)
        => _db.Payouts
            .Where(p => p.SnapshotId == snapshotId && p.Status != EntityStatus.Deleted)
            .OrderBy(p => p.Amount)
            .ToListAsync(ct);

    public async Task<(bool ok, string? error)> MarkPaidAsync(Guid id, PaymentMethod method, string? referenceNo, string? notes, CancellationToken ct = default)
    {
        var p = await _db.Payouts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return (false, "Payout not found.");
        if (p.PaymentStatus == PayoutStatus.Paid) return (false, "Already marked paid.");

        p.PaymentMethod = method;
        p.PaymentStatus = PayoutStatus.Paid;
        p.PaidAt = DateTime.UtcNow;
        p.ReferenceNo = referenceNo?.Trim();
        p.Notes = notes?.Trim();
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }
}
