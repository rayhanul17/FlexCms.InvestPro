using FlexCms.InvestPro.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlexCms.InvestPro.Services;

/// <summary>
/// Sample module-side <see cref="BackgroundService"/>. Logs a one-line
/// summary of investment + partner counts on startup and every 24h
/// thereafter. Proves that:
///
/// <list type="bullet">
/// <item>A module can register an <c>IHostedService</c> from its
///   <see cref="InvestProModule.RegisterServices"/> — ASP.NET Core's
///   hosted-service runner picks it up because module
///   <c>RegisterServices</c> runs before <c>builder.Build()</c>.</item>
/// <item>The hosted service is a singleton, so it must resolve its
///   scoped <see cref="InvestProDbContext"/> through
///   <see cref="IServiceScopeFactory"/> per work iteration — same
///   pattern the host's <c>TrashCleanupService</c> uses.</item>
/// </list>
/// </summary>
public sealed class InvestProDailySummaryService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<InvestProDailySummaryService> _logger;

    public InvestProDailySummaryService(
        IServiceScopeFactory scopes,
        ILogger<InvestProDailySummaryService> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InvestProDailySummary: started.");

        // Initial tick on startup so operators see at least one summary
        // without waiting 24h.
        await EmitSummaryAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await EmitSummaryAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        finally
        {
            _logger.LogInformation("InvestProDailySummary: stopped.");
        }
    }

    private async Task EmitSummaryAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetService<InvestProDbContext>();
            if (db is null)
            {
                // Module DbContext not registered — e.g. the host booted
                // with the module's services skipped (deactivated marker).
                // Quiet skip — nothing to report.
                return;
            }

            var partnerCount = await db.Partners.CountAsync(ct);
            var activeInvestments = await db.Investments
                .CountAsync(i => i.LifecycleStatus == InvestmentLifecycle.Active, ct);
            var closedInvestments = await db.Investments
                .CountAsync(i => i.LifecycleStatus == InvestmentLifecycle.Closed, ct);

            _logger.LogInformation(
                "InvestProDailySummary: {Partners} partner(s), {Active} active investment(s), {Closed} closed.",
                partnerCount, activeInvestments, closedInvestments);
        }
        catch (Exception ex)
        {
            // First boot before EF migrations have run, transient DB
            // outage, etc. Log + continue the timer loop.
            _logger.LogWarning(ex, "InvestProDailySummary: skipped this tick — DB read failed.");
        }
    }
}
