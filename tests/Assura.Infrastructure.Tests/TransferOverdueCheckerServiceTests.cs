using Assura.Application.Common.Interfaces;
using Assura.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Assura.Infrastructure.Tests;

// Covers a crash observed live: TransferOverdueCheckerService (the only
// BackgroundService in the app) would catch an exception from its periodic DB
// check and call _logger.LogError — but if the host was already shutting down,
// the default Windows EventLog logging provider could already be disposed,
// so the LogError call itself threw ObjectDisposedException. That exception
// escaped ExecuteAsync entirely, permanently killing the background service
// (host logs "BackgroundService failed") instead of the loop just ending
// gracefully like the rest of the host's shutdown.
public class TransferOverdueCheckerServiceTests
{
    private static IServiceProvider BuildServiceProvider(IApplicationDbContext context)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => context);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionDuringShutdown_DoesNotLog()
    {
        using var cts = new CancellationTokenSource();

        var mockContext = new Mock<IApplicationDbContext>();
        // Simulates the exact race that caused the crash: cancellation lands
        // (host starts shutting down, disposing logging providers) while the
        // periodic DB check is in flight, and the check then throws.
        mockContext.Setup(c => c.Transfers)
            .Callback(() => cts.Cancel())
            .Throws(new InvalidOperationException("DB unavailable during shutdown"));
        var serviceProvider = BuildServiceProvider(mockContext.Object);

        var mockLogger = new Mock<ILogger<TransferOverdueCheckerService>>();
        var service = new TransferOverdueCheckerService(serviceProvider, mockLogger.Object);

        try
        {
            // Task.Delay(_checkInterval, stoppingToken) throwing once the loop sees the
            // token cancelled is normal BackgroundService shutdown behavior — the real
            // host's StopAsync swallows it too. It's unrelated to the bug under test
            // (whether LogError got called), so it's tolerated here rather than treated
            // as a test failure.
            await service.StartAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        mockLogger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionWhileRunning_LogsError()
    {
        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Transfers).Throws(new InvalidOperationException("transient DB error"));
        var serviceProvider = BuildServiceProvider(mockContext.Object);

        var mockLogger = new Mock<ILogger<TransferOverdueCheckerService>>();
        var service = new TransferOverdueCheckerService(serviceProvider, mockLogger.Object);

        using var cts = new CancellationTokenSource();

        await service.StartAsync(cts.Token);
        await Task.Delay(50); // let the fire-and-forget ExecuteAsync loop run once
        cts.Cancel();

        mockLogger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
