using System.Threading.Tasks;
using winamptospotifyweb.Models;

namespace winamptospotifyweb.Services
{
    public interface ISpotifyService
    {
        /// <summary>
        /// Creates access token based on code created with Authorization Button
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<string> GetAccessTokenAsync(string code);        

        /// <summary>
        /// Processes folder gets track names and creates playlist on Spotify and tracks to them.
        /// </summary>
        /// <param name="folderProcessor"></param>
        /// <returns></returns>
        public Task<PlaylistSummary> ProcessFolder(string folderPath, string accessToken);
    }
}
