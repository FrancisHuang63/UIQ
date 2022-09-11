using Microsoft.AspNetCore.Mvc;

namespace UIQ.Controllers
{
    public class HpcEnquireController : Controller
    {
        public IActionResult NwpRunningJobs()
        {
            return View();
        }

        public IActionResult CronStatus()
        {
            return View();
        }

        public IActionResult HpcStatus()
        {
            return View();
        }
    }
}
