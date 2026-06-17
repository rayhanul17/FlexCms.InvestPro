using FlexCms.Framework.Db.Ef;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class InvestmentService
{
    private readonly InvestProDbContext _db;
    private readonly EfRepository<Investment> _repo;
    public InvestmentService(InvestProDbContext db)
    {
        _db = db;
        _repo = new EfRepository<Investment>(db);
    }

    public async Task<List<Investment>> GetAllAsync(CancellationToken ct = default, bool includeDeleted = false)
        => (await _repo.FindAsync(x => true, ct, includeDeleted: includeDeleted))
            .OrderByDescending(x => x.CreatedAt).ToList();

    public Task<Investment?> GetByIdAsync(Guid id, CancellationToken ct = default, bool withPartners = false)
        => withPartners
            ? _repo.GetByIdAsync(id, ct, includes: x => x.PartnerContracts)
            : _repo.GetByIdAsync(id, ct);

    public Task<Investment?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.Investments.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<string> SuggestCodeAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";
        var maxExisting = await _db.Investments
            .Where(x => x.Code.StartsWith(prefix))
            .Select(x => x.Code)
            .ToListAsync(ct);

        int next = 1;
        foreach (var c in maxExisting)
        {
            var tail = c.Substring(prefix.Length);
            if (int.TryParse(tail, out var n) && n >= next) next = n + 1;
        }
        return $"{prefix}{next:D3}";
    }

    public async Task<(bool ok, string? error, Investment? saved)> CreateAsync(Investment model, CancellationToken ct = default)
    {
        model.Code = (model.Code ?? "").Trim();
        if (string.IsNullOrWhiteSpace(model.Code))
            return (false, "Code is required.", null);
        if (await _db.Investments.AnyAsync(x => x.Code == model.Code, ct))
            return (false, $"Code '{model.Code}' is already in use.", null);

        if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();
        model.LifecycleStatus = InvestmentLifecycle.Draft;
        if (model.StartDate == default) model.StartDate = DateTime.UtcNow;
        model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
        if (model.ExpectedEndDate.HasValue)
            model.ExpectedEndDate = DateTime.SpecifyKind(model.ExpectedEndDate.Value, DateTimeKind.Utc);
        await _repo.AddAsync(model, ct);
        await _db.SaveChangesAsync(ct);
        return (true, null, model);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(Guid id, Investment input, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(id, ct);
        if (row is null) return (false, "Not found.");

        if (row.LifecycleStatus != InvestmentLifecycle.Draft)
            return (false, "Only Draft investments can be edited.");

        var newCode = (input.Code ?? "").Trim();
        if (string.IsNullOrWhiteSpace(newCode))
            return (false, "Code is required.");
        if (!string.Equals(newCode, row.Code, StringComparison.Ordinal) &&
            await _db.Investments.AnyAsync(x => x.Code == newCode && x.Id != id, ct))
            return (false, $"Code '{newCode}' is already in use.");

        row.Code            = newCode;
        row.Name            = input.Name.Trim();
        row.Description     = input.Description?.Trim();
        row.StartDate       = DateTime.SpecifyKind(input.StartDate, DateTimeKind.Utc);
        row.ExpectedEndDate = input.ExpectedEndDate.HasValue
            ? DateTime.SpecifyKind(input.ExpectedEndDate.Value, DateTimeKind.Utc)
            : null;
        row.Notes           = input.Notes?.Trim();
        await _repo.UpdateAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var inv = await _db.Investments
            .Include(x => x.PartnerContracts)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (inv is null) return (false, "Not found.");
        if (inv.LifecycleStatus != InvestmentLifecycle.Draft)
            return (false, "Only Draft investments can be activated.");

        var validation = ShariahValidator.Validate(inv);
        if (!validation.IsValid)
            return (false, validation.Error);

        inv.LifecycleStatus = InvestmentLifecycle.Active;
        inv.ActivatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error, Investment? deleted)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(id, ct);
        if (row is null) return (false, "Not found.", null);
        if (row.LifecycleStatus != InvestmentLifecycle.Draft)
            return (false, "Only Draft investments can be deleted.", null);

        await _repo.SoftDeleteAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return (true, null, row);
    }
}
