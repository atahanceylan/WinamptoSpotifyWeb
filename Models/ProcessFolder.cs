using System.Runtime.CompilerServices;

namespace winamptospotifyweb.Models
{
    public class ProcessFolder
    {
        public ProcessFolder(string accesstoken, string filepath, string artistalbumname)
        {
            AccessToken = accesstoken;
            
            ArtistAlbumName = artistalbumname;

            FilePath = filepath;
        }

        public string AccessToken { get; set; }

        public string FilePath { get; set; }

        public string ArtistAlbumName { get; set; }

        public string PlaylistId { get; set; }

        public TrackInfo TracksInfo { get; set; }
    }
}
