using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Assura.Infrastructure.Services;

public class TransferOverdueCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransferOverdueCheckerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public TransferOverdueCheckerService(IServiceProvider serviceProvider, ILogger<TransferOverdueCheckerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Transfer Overdue Checker Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueTransfersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Don't log once shutdown has started: by this point the host may have
                // already disposed the logging providers (e.g. the default Windows
                // EventLog provider), and logging through a disposed provider throws
                // ObjectDisposedException, which escapes ExecuteAsync and permanently
                // kills this background service instead of just ending the loop.
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error occurred executing Overdue Transfers check.");
                }
            }

            // Wait for next check interval. A cancelled delay here just means the host
            // is shutting down while we're asleep — that's the normal, expected way this
            // loop ends, not a failure, so it's swallowed instead of left to escape
            // ExecuteAsync (which the host logs as "BackgroundService failed").
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
    }

    private async Task CheckOverdueTransfersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var overdueTransfers = await context.Transfers
            .Where(t => t.Status == TransferStatus.Active
                     && t.ExpectedReturnDate.HasValue
                     && t.ExpectedReturnDate.Value.Date < DateTime.UtcNow.Date)
            .ToListAsync(cancellationToken);

        if (!overdueTransfers.Any())
            return;

        // TransferApprovals.ApprovedByUserId is a required FK to Users, and there's no dedicated
        // "system" account — hardcoding an id (this used to hardcode 1) breaks the instant that id
        // doesn't exist in a given database, and since every transfer in this run shares one
        // SaveChangesAsync call, one bad id fails the *entire* batch: no transfer gets marked
        // Overdue at all, not just the audit row. Resolve a real user (the earliest-created Admin)
        // instead, and if none exists, still update every transfer's status — the automated audit
        // trail is a nice-to-have, not something that should block the actual status change.
        var systemUserId = await context.Users
            .Where(u => u.Role == UserRole.Admin)
            .OrderBy(u => u.Id)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        foreach (var transfer in overdueTransfers)
        {
            _logger.LogInformation($"Marking Transfer {transfer.Id} as Overdue.");
            transfer.Status = TransferStatus.Overdue;
            transfer.UpdatedAt = DateTime.UtcNow;

            if (systemUserId.HasValue)
            {
                context.TransferApprovals.Add(new Domain.Entities.TransferApproval
                {
                    TransferId = transfer.Id,
                    ApprovedByUserId = systemUserId.Value,
                    FromStatus = TransferStatus.Active,
                    ToStatus = TransferStatus.Overdue,
                    Comments = "Automatically marked as overdue by system.",
                    ApprovedAt = DateTime.UtcNow
                });
            }
        }

        if (!systemUserId.HasValue)
        {
            _logger.LogWarning("No Admin user found — marking transfers Overdue without an automated TransferApproval audit row.");
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Successfully marked {overdueTransfers.Count} transfers as overdue.");
    }
}
