using Microsoft.AspNetCore.Mvc;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    public class RescueToolsController : Controller
    {
        private readonly IUiqService _uiqService;

        public RescueToolsController(IUiqService uiqService)
        {
            _uiqService = uiqService;
        }

        public IActionResult ArchiveRedo()
        {
            ViewBag.ConfigList = _uiqService.GetArchiveViewModels();
            return View();
        }

        public IActionResult FixFailedModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }
    }
}
