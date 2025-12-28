namespace OpenTelemetryLib
{
    public class PlaylistSummary
    {
        /// <summary>Created album name.</summary>
        public string AlbumName { get; set; }

        /// <summary>Track names comma seperated.</summary>
        public string TracksAdded { get; set; }

        public int TotalTracksAdded { get; set; }
    }
}
