using Moq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using Serilog;
using WinampToSpotifyWeb.Models;
using WinampToSpotifyWeb.Services;

namespace WinampToSpotifyWeb.Tests
{
    public class SpotifyServiceTest
    {
        private Mock<ILogger> mockLogger;
        private Mock<IOptions<SpotifyApiDetails>> mockSpotifyApiDetails;
        private SpotifyService spotifyService;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger>();
            mockSpotifyApiDetails = new Mock<IOptions<SpotifyApiDetails>>();
            mockSpotifyApiDetails.Setup(x => x.Value).Returns(new SpotifyApiDetails
            {
                ClientID = "test-client-id",
                SecretID = "test-secret-id",
                RedirectUrl = "http://localhost/callback",
                ApiTokenUrl = "http://localhost/api/token",
                PlaylistBaseUrl = "http://localhost/api/playlist",
                TrackSearchBaseUrl = "http://localhost/api/search",
                PlaylistAddTrackBaseUrl = "http://localhost/api/playlist/{playlist_id}/tracks",
                UserID = "test-user-id"
            });

            spotifyService = new SpotifyService(mockLogger.Object, mockSpotifyApiDetails.Object);
        }

        [Test]
        public void GetAccessTokenAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => spotifyService.GetAccessTokenAsync(null));
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task GetAccessTokenAsync_ReturnsAccessToken_WhenCodeIsValid()
        {
            // Arrange
            var httpClientHandler = new Mock<HttpMessageHandler>();
            //var httpClient = new HttpClient(httpClientHandler);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"test-access-token\"}")
            };
            httpClientHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(responseMessage);

            // Act
            var result = await spotifyService.GetAccessTokenAsync("test-code");

            // Assert
            Assert.Equals("test-access-token", result);
        }

        [Test]
        public void ProcessFolder_ThrowsArgumentException_WhenFolderPathIsEmpty()
        {
            Assert.ThrowsAsync<ArgumentException>(() => spotifyService.ProcessFolder("", "test-access-token"));
        }

        [Test]
        public void ProcessFolder_ThrowsArgumentException_WhenAccessTokenIsEmpty()
        {
            Assert.ThrowsAsync<ArgumentException>(() => spotifyService.ProcessFolder("test-folderpath", ""));
        }

        [Test]
        [Ignore("Ignore a test")]
        public void ProcessFolder_ThrowsArgumentException_WhenFolderDoesNotContainMp3Files()
        {
            // Arrange
            var folderPath = "test-folderpath";
            Directory.CreateDirectory(folderPath);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => spotifyService.ProcessFolder(folderPath, "test-access-token"));
            Assert.Equals($"{folderPath} don't have any mp3 file", ex.Message);

            // Cleanup
            Directory.Delete(folderPath);
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task ProcessFolder_ReturnsPlaylistSummary_WhenFolderIsValid()
        {
            // Arrange
            var folderPath = "test-folderpath";
            Directory.CreateDirectory(folderPath);
            File.Create(Path.Combine(folderPath, "test.mp3")).Dispose();

            // Mock methods
            var mockSpotifyService = new Mock<SpotifyService>(mockLogger.Object, mockSpotifyApiDetails.Object);
            mockSpotifyService.Setup(x => x.CreatePlayList(It.IsAny<ProcessFolder>())).ReturnsAsync("test-playlist-id");
            mockSpotifyService.Setup(x => x.GetTrackUriAndNames(It.IsAny<ProcessFolder>())).ReturnsAsync(new TrackInfo("test-uri", "test-track"));
            mockSpotifyService.Setup(x => x.AddTracksToPlaylistOnSpotify(It.IsAny<ProcessFolder>())).Returns(Task.CompletedTask);

            // Act
            var result = await mockSpotifyService.Object.ProcessFolder(folderPath, "test-access-token");

            // Assert
            Assert.That(result is not null);
            Assert.Equals("test-folderpath", result.AlbumName);
            Assert.Equals("test-track", result.TracksAdded);

            // Cleanup
            Directory.Delete(folderPath, true);
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task CreatePlayList_ReturnsPlaylistId_WhenCalled()
        {
            // Arrange
            var processFolder = new ProcessFolder("test-access-token", "test-folderpath", "test-artist-album");
            var httpClientHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"test-playlist-id\"}")
            };
            httpClientHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(responseMessage);

            // Act
            var result = await spotifyService.CreatePlayList(processFolder);

            // Assert
            Assert.Equals("test-playlist-id", result);
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task GetTrackUriAndNames_ReturnsTrackInfo_WhenCalled()
        {
            // Arrange
            var processFolder = new ProcessFolder("test-access-token", "test-folderpath", "test-artist-album");
            var mockSpotifyService = new Mock<SpotifyService>(mockLogger.Object, mockSpotifyApiDetails.Object);
            mockSpotifyService.Setup(x => x.GetTrackUri(It.IsAny<ProcessFolder>())).ReturnsAsync(new Dictionary<string, string> { { "test-uri", "test-track" } });

            // Act
            var result = await mockSpotifyService.Object.GetTrackUriAndNames(processFolder);

            // Assert
            Assert.That(result is not null);
            Assert.Equals("test-uri", result.TrackUri);
            Assert.Equals("test-track", result.TrackName);
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task AddTracksToPlaylistOnSpotify_AddsTracks_WhenCalled()
        {
            // Arrange
            var processFolder = new ProcessFolder("test-access-token", "test-folderpath", "test-artist-album")
            {
                PlaylistId = "test-playlist-id",
                TracksInfo = new TrackInfo("test-uri", "test-track")
            };
            var httpClientHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpClientHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(responseMessage);

            // Act
            await spotifyService.AddTracksToPlaylistOnSpotify(processFolder);

            // Assert
            // No exception means success
        }
    }
}