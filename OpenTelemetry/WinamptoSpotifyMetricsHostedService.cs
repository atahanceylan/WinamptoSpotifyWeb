using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenTelemetryLib
{
    internal sealed class WinamptoSpotifyMetricsHostedService(IWinamptoSpotifyMetricsManager metricsManager, ILogger<WinamptoSpotifyMetricsHostedService> logger) : IHostedService
    {
        private readonly IWinamptoSpotifyMetricsManager _metricsManager = metricsManager ?? throw new ArgumentNullException(nameof(metricsManager));
        private readonly ILogger<WinamptoSpotifyMetricsHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting WinamptoSpotify metrics registration...");
                _metricsManager.Start();
                _logger.LogInformation("WinamptoSpotify metrics registered.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start WinamptoSpotify metrics manager.");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}