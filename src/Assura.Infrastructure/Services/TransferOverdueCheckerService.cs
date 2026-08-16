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
                     && t.ReturnDate.HasValue 
                     && t.ReturnDate.Value.Date < DateTime.UtcNow.Date)
            .ToListAsync(cancellationToken);

        if (!overdueTransfers.Any())
            return;

        foreach (var transfer in overdueTransfers)
        {
            _logger.LogInformation($"Marking Transfer {transfer.Id} as Overdue.");
            transfer.Status = TransferStatus.Overdue;
            transfer.UpdatedAt = DateTime.UtcNow;
            
            // Optional: You could also log an automated TransferApproval here to track the status change
            context.TransferApprovals.Add(new Domain.Entities.TransferApproval
            {
                TransferId = transfer.Id,
                ApprovedByUserId = 1, // System User ID or 0 depending on your DB
                FromStatus = TransferStatus.Active,
                ToStatus = TransferStatus.Overdue,
                Comments = "Automatically marked as overdue by system.",
                ApprovedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Successfully marked {overdueTransfers.Count} transfers as overdue.");
    }
}
