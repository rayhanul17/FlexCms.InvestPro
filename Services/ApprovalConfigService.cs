using FlexCms.Framework.Db.Ef;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class ApprovalConfigService
{
    private readonly InvestProDbContext _db;
    private readonly EfRepository<ApprovalConfig> _repo;
    public ApprovalConfigService(InvestProDbContext db)
    {
        _db = db;
        _repo = new EfRepository<ApprovalConfig>(db);
    }

    public async Task<List<ApprovalConfig>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.FindAsync(x => true, ct))
            .OrderBy(x => x.LedgerType).ToList();

    public async Task<bool> UpdateAsync(Guid id, decimal autoBelow, decimal reqAbove, decimal allAbove, ApproverRole role, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(id, ct);
        if (row is null) return false;
        row.AutoApproveBelow        = autoBelow;
        row.RequireApprovalAbove    = reqAbove;
        row.RequireAllPartnersAbove = allAbove;
        row.ApproverRole            = role;
        await _repo.UpdateAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
