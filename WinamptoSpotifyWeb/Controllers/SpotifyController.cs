using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetryLib;
using System;
using System.IO;
using System.Threading.Tasks;
using WinampToSpotifyWeb.Models;


namespace WinampToSpotifyWeb.Controllers
{
    [Route("")]
    public class SpotifyController : Controller
    {

        private readonly SpotifyApiDetails spotifyAPIDetails;
        private readonly ISpotifyService spotifyService;
        private readonly IWebHostEnvironment _env;

        public SpotifyController(ISpotifyService spotifyService, IOptions<SpotifyApiDetails> spotifyApiDetails, IWebHostEnvironment env)
        {
            this.spotifyService = spotifyService;
            spotifyAPIDetails = spotifyApiDetails.Value;
            _env = env;
        }

        [HttpGet]
        public IActionResult Authenticate()
        {
            var qb = new QueryBuilder
            {
                { "response_type", "code" },
                { "client_id", spotifyAPIDetails.ClientID },
                { "scope", spotifyAPIDetails.ApiScopes },
                { "redirect_uri", spotifyAPIDetails.RedirectUrl }
            };
            ViewData["AuthorizationUrl"] = spotifyAPIDetails.AuthorizationUrl;
            ViewData["qb"] = qb;

            return View();
        }

        [Route("/callback")]
        public async Task<ActionResult> Get(string code)
        {
            CookieSet("access_token", await spotifyService.GetAccessTokenAsync(code), 9000);
            return await Task.Run<ActionResult>(() => RedirectToAction("SelectFolder"));
        }

        [HttpGet]
        [Route("/selectfolder")]
        public IActionResult SelectFolder()
        {
            var folderPath = Path.Combine(_env.ContentRootPath, "SampleMp3Archieve", "Ozbi -Rakılı Seri 1");
            ViewBag.Mp3FolderPath = folderPath;
            return View();
        }

        [HttpPost]
        [Route("/processfolder")]
        public async Task<IActionResult> ProcessFolder(string folderpath)
        {
            var playlistSummary = await spotifyService.ProcessFolder(folderpath, Request.Cookies["access_token"]);
            ViewData["plSummary"] = spotifyService.GetPlaylistSummary();
            return View();
        }

        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  

        public void CookieSet(string key, string value, int? expireTime)
        {

            CookieOptions option = new CookieOptions(new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
            });

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            Response.Cookies.Append(key, value, option);
        }
    }
}
