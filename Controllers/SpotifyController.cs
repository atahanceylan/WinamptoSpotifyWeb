using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using winamptospotifyweb.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using winamptospotifyweb.Services;
using Microsoft.AspNetCore.Http.Extensions;


namespace winamptospotifyweb.Controllers
{
    [Route("")]
    public class SpotifyController : Controller
    {

        private readonly SpotifyApiDetails spotifyAPIDetails;
        private readonly ISpotifyService spotifyService;

        public SpotifyController(ISpotifyService spotifyService, IOptions<SpotifyApiDetails> spotifyApiDetails)
        {
            this.spotifyService = spotifyService;
            spotifyAPIDetails = spotifyApiDetails.Value;
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
            return View();
        }

        [HttpPost]
        [Route("/processfolder")]
        public async Task<IActionResult> ProcessFolder(string folderpath)
        {
            ViewData["plSummary"] = await spotifyService.ProcessFolder(folderpath, Request.Cookies["access_token"]);
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
