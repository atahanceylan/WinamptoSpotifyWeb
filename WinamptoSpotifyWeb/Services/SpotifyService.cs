using Serilog;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using winamptospotifyweb.Models;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.Json;

namespace winamptospotifyweb.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly SpotifyApiDetails SpotifyAPIDetails;
        private readonly ILogger logger;

        public SpotifyService(ILogger logger, IOptions<SpotifyApiDetails> spotifyAPIDetails)
        {
            this.logger = logger;
            SpotifyAPIDetails = spotifyAPIDetails.Value;
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(SpotifyAPIDetails.ClientID + ":" + SpotifyAPIDetails.SecretID)));

                    FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", SpotifyAPIDetails.RedirectUrl),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    });

                    var result = await client.PostAsync(SpotifyAPIDetails.ApiTokenUrl, formContent);
                    var content = await result.Content.ReadAsStringAsync();
                    var spotifyAuth = JsonSerializer.Deserialize<SpotifyJsonResponseWrapper.AccessToken>(content);
                    return spotifyAuth.access_token;
                }
            }
        }


        public async Task<PlaylistSummary> ProcessFolder(string folderPath, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentException($"{nameof(folderPath)} is empty");
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException($"{nameof(accessToken)} is empty");

            string artistAndOrAlbum = folderPath.Split('\\')[folderPath.Split('\\').Length -1];
            ProcessFolder processFolder = new ProcessFolder(accessToken, folderPath, artistAndOrAlbum);
            processFolder.PlaylistId = await CreatePlayList(processFolder);
            processFolder.TracksInfo = await GetTrackUriAndNames(processFolder);
            await AddTracksToPlaylistOnSpotify(processFolder);
            string addedTracks = String.Join(",", processFolder.TracksInfo.TrackName.Split(',', StringSplitOptions.RemoveEmptyEntries));

            logger.Information($"{processFolder.FilePath} is processed.");
            logger.Information($"{artistAndOrAlbum} album created successfully.Tracks added: {addedTracks}");

            return new PlaylistSummary
            {
                AlbumName = artistAndOrAlbum,
                TracksAdded = addedTracks,
            };
        }

        private async Task<TrackInfo> GetTrackUriAndNames(ProcessFolder processFolder)
        {
            Dictionary<string, string> trackInfoDict = await GetTrackUri(processFolder);
            return new TrackInfo(string.Join(",", trackInfoDict.Keys), string.Join(",", trackInfoDict.Values));
        }

        private async Task<string> CreatePlayList(ProcessFolder folderOperation)
        {
            string playlistId = String.Empty;
            var stringPayload = new
            {
                name = folderOperation.ArtistAlbumName,
                description = folderOperation.ArtistAlbumName
            };
            var bodyPayload = new StringContent(JsonSerializer.Serialize(stringPayload));
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + folderOperation.AccessToken);
                var response = await client.PostAsync(SpotifyAPIDetails.PlaylistBaseUrl.Replace("{UserId}", SpotifyAPIDetails.UserID), bodyPayload);
                var responseContent = await response.Content.ReadAsStringAsync();
                var playlist = JsonSerializer.Deserialize< SpotifyJsonResponseWrapper.PlayList>(responseContent);
                playlistId = playlist.id;
            }
            logger.Information($"{folderOperation.ArtistAlbumName} created successfully");
            return playlistId;
        }

        private async Task<Dictionary<string, string>> GetTrackUri(ProcessFolder folderOperation)
        {
            Dictionary<string, string> trackInfoDict = new Dictionary<string, string>();
            string artistInfo = folderOperation.FilePath.Split('\\')[folderOperation.FilePath.Split('\\').Length - 1];
            string artist = artistInfo.Split(' ').FirstOrDefault();

            List<string> fileNamesList = GetMp3FileNames(folderOperation.FilePath, folderOperation.ArtistAlbumName);
            if (fileNamesList.Count > 0)
            {
                foreach (var qb in from item in fileNamesList
                                   let qb = BuildQueryForTrackAddition(artist, item)
                                   select qb)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + folderOperation.AccessToken);
                        var trackUrl = SpotifyAPIDetails.TrackSearchBaseUrl + qb.ToQueryString().ToString();
                        var response = await client.GetAsync(trackUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            var results = JsonSerializer.Deserialize< SpotifyJsonResponseWrapper.RootObject>(responseString);
                            var tracks = results.tracks;
                            if (tracks.items.Count > 0)
                            {
                                trackInfoDict.TryAdd(tracks.items[0].uri, tracks.items[0].name);
                                logger.Information($"Track {tracks.items[0].name} found.");
                            }
                        }
                    }
                }
                return trackInfoDict;
            }
            return trackInfoDict;
        }

        private static QueryBuilder BuildQueryForTrackAddition(string artist, string item)
        {
            return new QueryBuilder
                    {
                        { "q", item + $" artist:{artist}" },
                        { "type", "track" },
                        { "limit", "1" }
                    };
        }

        private List<string> GetMp3FileNames(string path, string artistName)
        {
            FileInfo[] filesInfoArray = new DirectoryInfo(path).GetFiles("*.mp3");
            List<string> fileNames = new List<string>();
            if (filesInfoArray.Length > 0)
            {
                foreach (var file in filesInfoArray)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    fileName = NormalizeFileName(artistName, fileName);
                    fileNames.Add(fileName);
                }
            }
            else
            {
                logger.Error($"Cannot find any file in {path}");
                throw new FileNotFoundException($"Cannot find any file in {path}");
            }
            return fileNames;
        }

        private static string NormalizeFileName(string artistName, string fileName)
        {
            Regex reg = new Regex(@"[^\p{L}\p{N} ]");
            fileName = reg.Replace(fileName, String.Empty);
            fileName = Regex.Replace(fileName, @"[0-9]+", "");
            fileName = fileName.Replace(artistName, "");
            fileName = fileName.TrimStart();
            fileName = fileName.TrimEnd();
            return fileName;
        }

        private async Task AddTracksToPlaylistOnSpotify(ProcessFolder folderOperation)
        {
            var qb = new QueryBuilder
            {
                { "uris", folderOperation.TracksInfo.TrackUri }
            };
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + folderOperation.AccessToken);
                await client.PostAsync(SpotifyAPIDetails.PlaylistAddTrackBaseUrl.Replace("{playlist_id}", folderOperation.PlaylistId) + qb.ToQueryString(), null);
            }

            await Task.Yield();
        }
    }
}
