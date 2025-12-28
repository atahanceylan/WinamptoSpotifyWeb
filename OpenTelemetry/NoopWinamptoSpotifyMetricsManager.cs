using Microsoft.Extensions.Logging;

namespace OpenTelemetryLib
{
    internal sealed class NoopWinamptoSpotifyMetricsManager(ILogger logger) : IWinamptoSpotifyMetricsManager
    {
        public void Start()
        {
            logger.LogInformation("NoopDmmMetricsManager.Start() called - metrics will not be registered.");
        }
    }
}
