namespace OpenTelemetryLib
{
    public interface IWinamptoSpotifyMetricsManager
    {
        /// <summary>
        /// Registers all custom metrics contained within the DasMetrics instance.
        /// </summary>
        void Start();
    }
}
