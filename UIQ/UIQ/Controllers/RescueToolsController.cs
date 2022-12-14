using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIQ.Attributes;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [Authorize]
    public class RescueToolsController : Controller
    {
        private readonly IUiqService _uiqService;

        public RescueToolsController(IUiqService uiqService)
        {
            _uiqService = uiqService;
        }

        [MenuPageAuthorize(Enums.MenuEnum.ArchiveRedo)]
        public IActionResult ArchiveRedo()
        {
            ViewBag.ConfigList = _uiqService.GetArchiveViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.FixFailedModel)]
        public IActionResult FixFailedModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }
    }
}