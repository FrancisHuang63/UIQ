using Microsoft.AspNetCore.Mvc;

namespace UIQ.Controllers
{
    public class MaintainToolsController : Controller
    {
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
    }
}
