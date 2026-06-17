using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;

namespace FlexCms.InvestPro.Services;

/// <summary>
/// Owns every read and write against close_requests and close_approvals.
/// CloseService composes this with InvestmentSnapshotService inside a single
/// DbContext transaction.
/// </summary>
[FcmsScoped]
public class CloseRequestService
{
    private readonly InvestProDbContext _db;
    public CloseRequestService(InvestProDbContext db) => _db = db;

    // ── Queries ─────────────────────────────────────────────────────────

    public Task<List<CloseRequest>> GetByInvestmentAsync(Guid investmentId, CancellationToken ct = default)
        => _db.CloseRequests
            .Include(r => r.Approvals)
            .Where(r => r.InvestmentId == investmentId && r.Status != EntityStatus.Deleted)
            .OrderByDescending(r => r.InitiatedAt)
            .ToListAsync(ct);

    public Task<CloseRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.CloseRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> HasPendingForInvestmentAsync(Guid investmentId, CancellationToken ct = default)
        => _db.CloseRequests
            .AnyAsync(r => r.InvestmentId == investmentId
                           && r.RequestStatus == CloseRequestStatus.Pending
                           && r.Status != EntityStatus.Deleted, ct);

    // ── Shared-context writers ──────────────────────────────────────────
    // Kept so orchestrators can express "operate on my transaction" intent.
    // The `db` arg here is the same scoped DbContext as our `_db`.

    public Task<bool> HasPendingForInvestmentOnContextAsync(InvestProDbContext db, Guid investmentId, CancellationToken ct = default)
        => db.CloseRequests
            .AnyAsync(r => r.InvestmentId == investmentId
                           && r.RequestStatus == CloseRequestStatus.Pending
                           && r.Status != EntityStatus.Deleted, ct);

    public Task<CloseRequest?> GetByIdOnContextAsync(InvestProDbContext db, Guid id, CancellationToken ct = default)
        => db.CloseRequests.Include(r => r.Approvals).FirstOrDefaultAsync(r => r.Id == id, ct);

    public void StageRequestOnContext(InvestProDbContext db, CloseRequest req) => db.CloseRequests.Add(req);
    public void StageApprovalOnContext(InvestProDbContext db, CloseApproval approval) => db.CloseApprovals.Add(approval);
}
