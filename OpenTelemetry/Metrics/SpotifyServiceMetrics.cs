using System.Diagnostics.Metrics;

namespace OpenTelemetryLib.Metrics
{

    public class SpotifyServiceMetrics : IWinampToSpotifyWebMetrics
    {
        private readonly ISpotifyService _spotifyService;
        public SpotifyServiceMetrics(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService; 
        }

        public void RegisterMetrics(Meter meter)
        {
            var tracksAddedMetric = meter.CreateObservableGauge("winamptospotifyweb.spotifyservice.totaltracksadded", () => _spotifyService.GetPlaylistSummary().TotalTracksAdded,
            "unitless", "Number of tracks added");
        
        }
    }
}
