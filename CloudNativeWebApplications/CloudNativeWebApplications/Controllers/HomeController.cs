using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNativeWebApplications.Controllers
{
    public class HomeController : Controller
    {
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
                returnMsg = "아이디를 입력하세요.";
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                returnResult = false;
                returnMsg = "패스워드를 입력하세요.";
            }
            else
            {
                var adminUserList = _commonDac.SelectAdminUser(id);
                List<object> list = (List<object>)adminUserList;
                if (list.Count == 0)
                {
                    returnResult = false;
                    returnMsg = "관리자로 등록되지 않은 사용자 입니다.";
                }
                else
                {
                    string upn = null;
#if !DEBUG
                    try
                    {
                        upn = ActiveDirectoryManager.GetUserPrincipalName(id, password);
                        if (string.IsNullOrEmpty(upn))
                        {
                            returnResult = false;
                            returnMsg = "Active Directory에서 사용자를 찾을 수 없습니다.";
                        }
                    }
                    catch (Exception ex)
                    {
                        MethodBase method = MethodInfo.GetCurrentMethod();
                        FileLogWriter.WriteErrorLog(string.Format("{0}.{1}", method.DeclaringType.ToString(), method.Name), string.Empty, ex);
                        returnResult = false;
                        returnMsg = "패스워드를 확인 해주세요";
                    }
#endif

                    if (returnResult)
                    {
                        identity = new ClaimsIdentity(new[] {
                            new Claim("UserId", id),
                            new Claim("UserName", Convert.ToString(((object[])((System.Collections.Generic.IDictionary<string, object>)list[0]).Values)[1])),
                            new Claim(ClaimTypes.Role, "ADMIN")
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity);
                        var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        UserInfo.Current.SetIdentity(identity);

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            returnMsg = "/Board/AnnouncementsManagement";
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
    }
}
