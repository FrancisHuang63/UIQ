using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIQ.Attributes;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [Authorize]
    public class HpcEnquireController : Controller
    {
        private readonly IUiqService _uiqService;

        public HpcEnquireController(IUiqService uiqService)
        {
            _uiqService = uiqService;
        }

        [MenuPageAuthorize(Enums.MenuEnum.NwpRunningJobs)]
        public IActionResult NwpRunningJobs()
        {
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.CronStatus)]
        public IActionResult CronStatus()
        {
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.HpcStatus)]
        public IActionResult HpcStatus()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> NwpRunningNodes(string p_node)
        {
            var html = await _uiqService.GetExecuteNwpRunningNodesCommandHtmlAsync(p_node);
            return html;
        }
    }
}