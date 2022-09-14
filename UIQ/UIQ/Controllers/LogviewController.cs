using Microsoft.AspNetCore.Mvc;

namespace UIQ.Controllers
{
    public class LogviewController : Controller
    {
        public IActionResult Index()
        {
            return Content(string.Empty);
        }
    }
}
