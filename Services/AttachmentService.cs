using FlexCms.Framework.Modules;
using FlexCms.Framework.Modules.Attributes;
using FlexCms.InvestPro.Data;
using EntityStatus = FlexCms.Framework.Db.EntityStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlexCms.InvestPro.Services;

[FcmsScoped]
public class AttachmentService
{
    private readonly ModuleActivationOptions _opts;
    private readonly IWebHostEnvironment _env;

    public AttachmentService(ModuleActivationOptions opts, IWebHostEnvironment env)
    {
        _opts = opts;
        _env = env;
    }

    private InvestProDbContext OpenDb() =>
        (InvestProDbContext)new InvestProModule().CreateMigrationContext(_opts.ConnectionString, _opts.Provider)!;

    public const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    public const int MaxFilesPerEntry = 20;

    public string GetUploadRoot(Guid investmentId, LedgerKind kind) =>
        Path.Combine(_env.WebRootPath, "uploads", "investpro",
                     investmentId.ToString("N"), kind.ToString().ToLower());

    public async Task<List<LedgerAttachment>> GetForEntryAsync(LedgerKind kind, Guid entryId, CancellationToken ct = default)
    {
        await using var db = OpenDb();
        return await db.LedgerAttachments
            .Where(x => x.LedgerType == kind && x.LedgerEntryId == entryId && x.Status != EntityStatus.Deleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<LedgerAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = OpenDb();
        return await db.LedgerAttachments.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(bool ok, string? error, LedgerAttachment? saved)> UploadAsync(
        Guid investmentId, LedgerKind kind, Guid entryId,
        IFormFile file, AttachmentLabel label, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return (false, "No file uploaded.", null);
        if (file.Length > MaxFileSize)
            return (false, $"File too large. Max {MaxFileSize / 1024 / 1024} MB.", null);

        await using var db = OpenDb();
        var existing = await db.LedgerAttachments.CountAsync(
            x => x.LedgerType == kind && x.LedgerEntryId == entryId && x.Status != EntityStatus.Deleted, ct);
        if (existing >= MaxFilesPerEntry)
            return (false, $"Maximum {MaxFilesPerEntry} files per entry reached.", null);

        var folder = GetUploadRoot(investmentId, kind);
        Directory.CreateDirectory(folder);

        var safeName = Path.GetFileNameWithoutExtension(file.FileName);
        var ext = Path.GetExtension(file.FileName);
        var uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, uniqueName);
        var relativeUrl = $"/uploads/investpro/{investmentId:N}/{kind.ToString().ToLower()}/{uniqueName}";

        await using (var fs = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(fs, ct);

        var row = new LedgerAttachment
        {
            Id = Guid.NewGuid(),
            LedgerType = kind,
            LedgerEntryId = entryId,
            FilePath = relativeUrl,
            FileName = Path.GetFileName(file.FileName),
            FileType = file.ContentType,
            FileSize = file.Length,
            AttachmentLabel = label,
        };
        db.LedgerAttachments.Add(row);
        await db.SaveChangesAsync(ct);
        return (true, null, row);
    }

    public async Task<(bool ok, string? error, LedgerAttachment? deleted)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = OpenDb();
        var row = await db.LedgerAttachments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row is null) return (false, "Not found.", null);

        row.Status = EntityStatus.Deleted;
        row.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        try
        {
            var physical = Path.Combine(_env.WebRootPath, row.FilePath.TrimStart('/'));
            if (File.Exists(physical)) File.Delete(physical);
        }
        catch { /* ignore filesystem cleanup errors — soft-delete still wins */ }

        return (true, null, row);
    }
}
