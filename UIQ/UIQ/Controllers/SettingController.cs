using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIQ.Enums;

namespace UIQ.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Edit(int? memberId)
        {
            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()))
            {
                ViewBag.ErrorMessage = "Your Group Can Not Use This Page！";
            }

            //TODO

            return View();
        }
    }
}
