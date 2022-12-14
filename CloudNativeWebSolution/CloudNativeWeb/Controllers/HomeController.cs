using CloudNativeWeb.Models;
using CloudNativeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNativeWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CommonService _commonService;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _commonService = new CommonService();
        }

        public IActionResult MovieList()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return Redirect("/Home/Login");
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult MovieItemRegistration([FromQuery] string title)
        {
            ViewBag.CurrentId = title;
            return View();
        }

        public IActionResult MovieItemModification([FromQuery] string title)
        {
            ViewBag.CurrentId = title;
            return View();
        }

        public IActionResult LoginProcess(string id, string password, string returnUrl)
        {
            ClaimsIdentity identity = null;
            bool returnResult = true;
            string returnMsg = string.Empty;

            if (string.IsNullOrWhiteSpace(id))
            {
                returnResult = false;
                returnMsg = "Please enter ID.";
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                returnResult = false;
                returnMsg = "Please enter Password.";
            }
            else
            {
                var adminUserList = _commonService.SelectAdminUser(id, password);
                List<object> list = (List<object>)adminUserList;
                if (list.Count == 0)
                {
                    returnResult = false;
                    returnMsg = "Unregistered user.";
                }
                else
                {
                    if (returnResult)
                    {
                        identity = new ClaimsIdentity(new[] {
                            new Claim("UserId", id),
                            new Claim("Emaile", Convert.ToString(((object[])((System.Collections.Generic.IDictionary<string, object>)list[0]).Values)[1])),
                            new Claim(ClaimTypes.Role, "ADMIN")
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity);
                        var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        Common.UserInfo.Current.SetIdentity(identity);

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            returnMsg = "Home/MovieList";
                        }
                        else
                        {
                            returnMsg = returnUrl;
                        }
                    }
                }
            }
            return Json("{\"result\": \"" + returnResult + "\", \"msg\": \"" + returnMsg + "\"}");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult RegisterProcess(UserInfo models)
        {
            var result = _commonService.InsertUserInfo(models);
            return Json(result);
        }

        public async Task<IActionResult> SelectMovieList(double rating, string genre, string title)
        {
            var result = await _commonService.SelectMovieList(rating, genre, title);
            return Json(result);
        }

        public async Task<IActionResult> SelectMovieItem(string title)
        {
            var result = await _commonService.SelectMovieItem(title);
            return Json(result);
        }

        public async Task<IActionResult> InsertMovieItem(Movie models)
        {
            var result = await _commonService.InsertMovieItem(models);
            return Json(result);
        }

        public async Task<IActionResult> UpdateMovieItem(Movie models)
        {
            var result = await _commonService.UpdateMovieItem(models);
            return Json(result);
        }

        public async Task<IActionResult> SelectCommentList(string title)
        {
            var result = await _commonService.SelectCommentList(title);
            return Json(result);
        }

        public async Task<IActionResult> InsertComment(Comment models)
        {
            models.CommentId = Convert.ToString(Guid.NewGuid());
            var result = await _commonService.InsertComment(models);
            return Json(result);
        }

        public async Task<IActionResult> UpdateComment(Comment models)
        {
            var result = await _commonService.UpdateComment(models);
            return Json(result);
        }

        public async Task<IActionResult> SeclectFileItem(string title)
        {
            var result = await _commonService.SelectFileAsync(title);
            return Json(result);
        }

        public async Task<IActionResult> FileUpload(List<IFormFile> files, string title)
        {
            var result = await _commonService.UploadFileAsync(files, title);
            return Json(result);
        }

        [HttpGet]
        public IActionResult FileDownload(string fileName, string displayName)
        {
            var result = _commonService.DownloadFileAsync(fileName).Result;
            return File(result, "application/octet-stream", displayName);
        }

    }
}
