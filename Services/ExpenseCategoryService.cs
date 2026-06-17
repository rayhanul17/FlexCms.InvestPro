using FlexCms.Framework.Db.Ef;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class ExpenseCategoryService
{
    private readonly InvestProDbContext _db;
    private readonly EfRepository<ExpenseCategory> _repo;
    public ExpenseCategoryService(InvestProDbContext db)
    {
        _db = db;
        _repo = new EfRepository<ExpenseCategory>(db);
    }

    public async Task<List<ExpenseCategory>> GetAllAsync(CancellationToken ct = default, bool includeDeleted = false)
        => (await _repo.FindAsync(x => true, ct, includeDeleted: includeDeleted))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();

    public Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<ExpenseCategory> CreateAsync(ExpenseCategory model, CancellationToken ct = default)
    {
        if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();
        model.IsSystem = false; // user-created can never be system
        await _repo.AddAsync(model, ct);
        await _db.SaveChangesAsync(ct);
        return model;
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string? description, bool isActive, int sortOrder, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(id, ct);
        if (row is null) return false;
        row.Name        = name.Trim();
        row.Description = description?.Trim();
        row.IsActive    = isActive;
        row.SortOrder   = sortOrder;
        await _repo.UpdateAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool ok, string? error, ExpenseCategory? deleted)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(id, ct);
        if (row is null) return (false, "Not found.", null);
        if (row.IsSystem) return (false, "System categories cannot be deleted. Deactivate instead.", null);
        await _repo.SoftDeleteAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return (true, null, row);
    }
}
