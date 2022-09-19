using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [Authorize]
    public class MaintainToolsController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MaintainToolsController(IUiqService uiqService, IHttpContextAccessor httpContextAccessor)
        {
            _uiqService = uiqService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult TyphoonInitialData()
        {
            return View();
        }

        public IActionResult Command()
        {
            return View();
        }

        public IActionResult CronSetting()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult CronSetting(string cronMode)
        {
            var memberId = _httpContextAccessor.HttpContext.User.FindFirstValue("Id");
            _uiqService.SaveCronSetting(memberId, cronMode);

            return View();
        }
    }
}