using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string account, string password)
        {
            var loginResult = await _userService.Login(account, password);
            if (loginResult == false)
            {
                ViewData["Message"] = Common.NEED_LOGIN_FAILED_MESSAGE;
                return View(nameof(Index));
            }

            return LocalRedirectPermanent($"~/Home/{nameof(HomeController.Index)}");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userService.Logout();
            return RedirectToAction(nameof(Index));
        }
    }
}