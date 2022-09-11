using Microsoft.AspNetCore.Mvc;

namespace UIQ.Controllers
{
    public class RescueToolsController : Controller
    {
        public IActionResult ArchiveRedo()
        {
            return View();
        }

        public IActionResult FixFailedModel()
        {
            return View();
        }
    }
}
