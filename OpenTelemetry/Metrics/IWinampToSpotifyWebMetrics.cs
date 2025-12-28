using System.Diagnostics.Metrics;

namespace OpenTelemetryLib.Metrics
{

    /// <summary>
    ///     Interface for implementing custom metrics registration and management.
    /// </summary>
    public interface IWinampToSpotifyWebMetrics
    {
        /// <summary>
        ///     Registers custom metrics with the provided <see cref="Meter" /> instance.
        /// </summary>
        /// <param name="meter">The <see cref="Meter" /> used to create and register metrics.</param>
        void RegisterMetrics(Meter meter);
    }
}
