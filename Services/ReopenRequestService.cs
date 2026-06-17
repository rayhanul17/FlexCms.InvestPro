using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;

namespace FlexCms.InvestPro.Services;

/// <summary>
/// Owns every read and write against reopen_requests and reopen_approvals.
/// ReopenService composes this with InvestmentSnapshotService inside a single
/// DbContext transaction.
/// </summary>
[FcmsScoped]
public class ReopenRequestService
{
    private readonly InvestProDbContext _db;
    public ReopenRequestService(InvestProDbContext db) => _db = db;

    // ── Queries ─────────────────────────────────────────────────────────

    public Task<List<ReopenRequest>> GetByInvestmentAsync(Guid investmentId, CancellationToken ct = default)
        => _db.ReopenRequests
            .Include(r => r.Approvals)
            .Where(r => r.InvestmentId == investmentId && r.Status != EntityStatus.Deleted)
            .OrderByDescending(r => r.InitiatedAt)
            .ToListAsync(ct);

    public Task<ReopenRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ReopenRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    /// <summary>
    /// True if the investment has an Approved reopen sitting un-recloseable.
    /// This is the signal for "post-reopen Active" — contracts are editable
    /// and the Close button is shown until the reclose generates v2.
    /// </summary>
    public Task<bool> HasApprovedForInvestmentAsync(Guid investmentId, CancellationToken ct = default)
        => _db.ReopenRequests
            .AnyAsync(r => r.InvestmentId == investmentId
                           && r.RequestStatus == ReopenRequestStatus.Approved
                           && r.Status != EntityStatus.Deleted, ct);

    // ── Shared-context writers ──────────────────────────────────────────

    public Task<bool> HasPendingForInvestmentOnContextAsync(InvestProDbContext db, Guid investmentId, CancellationToken ct = default)
        => db.ReopenRequests
            .AnyAsync(r => r.InvestmentId == investmentId
                           && r.RequestStatus == ReopenRequestStatus.Pending
                           && r.Status != EntityStatus.Deleted, ct);

    public Task<bool> HasApprovedForInvestmentOnContextAsync(InvestProDbContext db, Guid investmentId, CancellationToken ct = default)
        => db.ReopenRequests
            .AnyAsync(r => r.InvestmentId == investmentId
                           && r.RequestStatus == ReopenRequestStatus.Approved
                           && r.Status != EntityStatus.Deleted, ct);

    public Task<ReopenRequest?> GetByIdOnContextAsync(InvestProDbContext db, Guid id, CancellationToken ct = default)
        => db.ReopenRequests.Include(r => r.Approvals).FirstOrDefaultAsync(r => r.Id == id, ct);

    public void StageRequestOnContext(InvestProDbContext db, ReopenRequest req) => db.ReopenRequests.Add(req);
    public void StageApprovalOnContext(InvestProDbContext db, ReopenApproval approval) => db.ReopenApprovals.Add(approval);
}
