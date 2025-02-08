namespace WinampToSpotifyWeb.Models
{
    public class SpotifyApiDetails
    {
        public string ClientID { get; set; }
        public string SecretID { get; set; }
        public string UserID { get; set; }

        public string RedirectUrl { get; set; }

        public string PlaylistBaseUrl { get; set; }

        public string TrackSearchBaseUrl { get; set;}
        public string PlaylistAddTrackBaseUrl { get; set; }

        public string AuthorizationUrl { get; set; }

        public string ApiTokenUrl { get; set; }

        public string ApiScopes { get; set; }

    }
}
