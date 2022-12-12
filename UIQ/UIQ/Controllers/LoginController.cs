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
            if(account == null || password == null)
            {
                ViewData["Message"] = Common.NEED_LOGIN_FAILED_MESSAGE;
                return View(nameof(Index));
            }

            var loginResult = await _userService.Login(account, password);
            if (loginResult == false)
            {
                ViewData["Message"] = Common.NEED_LOGIN_FAILED_MESSAGE;
                return View(nameof(Index));
            }

            return LocalRedirectPermanent($"~/Home/{nameof(HomeController.Index)}");
        }
        
        public async Task<IActionResult> Logout()
        {
            await _userService.Logout();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> ChangeProfile()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> CheckPassword(string pwd)
        {
            var checkResult = await _userService.CheckPasswordCorrect(pwd);
            var response = new ApiResponse<bool>(data: checkResult);

            return Json(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> ChangeProfile(string newPwd)
        {
            var result = await _userService.ChangePassword(newPwd);
            var response = new ApiResponse<bool>(data: result);

            return Json(response);
        }
    }
}