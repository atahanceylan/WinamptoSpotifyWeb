using OpenTelemetryLib.Metrics;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;

namespace OpenTelemetryLib
{
    internal sealed class WinamptoSpotifyMetricsManager : IWinamptoSpotifyMetricsManager
    {
        /// <summary>
        ///     Specifies the name of the Meter used for collecting and registering custom metrics
        ///     in the DMM application.
        /// </summary>
        public const string MeterName = "winamptospotify_metrics";

        private readonly Meter _meter;
        private readonly IReadOnlyCollection<IWinampToSpotifyWebMetrics> _metrics;

        public WinamptoSpotifyMetricsManager(IEnumerable<IWinampToSpotifyWebMetrics> metrics, IMeterFactory meterFactory)
        {
            _metrics = metrics.ToImmutableList();
            _meter = meterFactory.Create(new MeterOptions(MeterName));
        }

        /// <summary>
        ///     Registers all custom metrics contained within the WinampToSpotify instance.
        /// </summary>
        public void Start()
        {
            foreach (var metric in _metrics)
            {
                metric.RegisterMetrics(_meter);
            }
        }
    }
}
