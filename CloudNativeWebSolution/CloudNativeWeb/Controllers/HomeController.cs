using CloudNativeWeb.Models;
using CloudNativeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public IActionResult Index()
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
                var adminUserList = _commonService.SelectAdminUser(id);
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
                            new Claim("UserName", Convert.ToString(((object[])((System.Collections.Generic.IDictionary<string, object>)list[0]).Values)[1])),
                            new Claim(ClaimTypes.Role, "ADMIN")
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity);
                        var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        Common.UserInfo.Current.SetIdentity(identity);

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            returnMsg = "/Index";
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
    }
}
