using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UIQ.Enums;
using UIQ.Models;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUiqService _uiqService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHttpContextAccessor httpContextAccessor, IUiqService uiqService, ILogger<HomeController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _uiqService = uiqService;
            _logger = logger;

            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()))
            {
                //TODO
            }
        }

        public IActionResult Index()
        {
            var models = _uiqService.GetHomeTableDatas();
            var menus = GenerateMenuItems();
            
            ViewBag.Menu = menus;
            return View(models);
        }

        public IActionResult DetailedStatus(string modelMemberNickname)
        {
            //TODO
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private Methods

        private IEnumerable<MenuViewModel> GenerateMenuItems()
        {
            return new List<MenuViewModel>(new MenuViewModel[]
            {
                //System enquire
                new MenuViewModel("System enquire", 1, new List<MenuViewModel>
                {
                    { new MenuViewModel("NWP running jobs", Url.Action(nameof(HpcEnquireController.NwpRunningJobs)), 1) },
                    { new MenuViewModel("CRON table status", Url.Action(nameof(HpcEnquireController.CronStatus)), 2) },
                    { new MenuViewModel("HPC system status", Url.Action(nameof(HpcEnquireController.HpcStatus)), 3)},
                }),
                //Model enquire
                new MenuViewModel("Model enquire", 2, new List<MenuViewModel>
                {
                    { new MenuViewModel("Job log check", Url.Action(nameof(ModelEnquireController.ModelLogFiles)), 1) },
                    { new MenuViewModel("Running status", Url.Action(nameof(ModelEnquireController.RunningStatus)), 2) },
                    { new MenuViewModel("Daily report", Url.Action(nameof(ModelEnquireController.DailyReport)), 3)},
                    { new MenuViewModel("Typhoon data check", Url.Action(nameof(ModelEnquireController.TyphoonInitialData)), 4) },
                    { new MenuViewModel("Batch history", Url.Action(nameof(ModelEnquireController.BatchHistory)), 5) },
                }),
                //Model rerun
                new MenuViewModel("Model rerun", 3, new List<MenuViewModel>
                {
                    { new MenuViewModel("Cancel running job", Url.Action(nameof(ModelRerunController.ResetModel)), 1) },
                    { new MenuViewModel("Dtg adjust", Url.Action(nameof(ModelRerunController.DtgAdjust)), 2) },
                    { new MenuViewModel("Lid adjust", Url.Action(nameof(ModelRerunController.LidAdjust)), 3) },
                    { new MenuViewModel("Submit model", Url.Action(nameof(ModelRerunController.SubmitModel)), 4) },
                }),
                //Rescue tools
                new MenuViewModel("Rescue tools", 4, new List<MenuViewModel>
                {
                    { new MenuViewModel("Archive redo", Url.Action(nameof(RescueToolsController.ArchiveRedo)), 1) },
                    { new MenuViewModel("Model output generate", Url.Action(nameof(RescueToolsController.FixFailedModel)), 2) },
                }),
                //Maintain tools
                new MenuViewModel("Maintain tools", 5, new List<MenuViewModel>
                {
                    { new MenuViewModel("Set typhoon data", Url.Action(nameof(MaintainToolsController.TyphoonInitialData)), 1) },
                    { new MenuViewModel("Special Use command", Url.Action(nameof(MaintainToolsController.Command)), 2) },
                    { new MenuViewModel("Cron_Mode set", Url.Action(nameof(MaintainToolsController.CronSetting)), 3) },
                }),
                //Reference info
                new MenuViewModel("Reference info", 6, new List<MenuViewModel>
                {
                    { new MenuViewModel("Set typhoon data", "~/reference/user_guide.pdf", 1) },
                    { new MenuViewModel("Special Use command", "~/reference/Monitor_reference.pdf", 2) },
                    { new MenuViewModel("Cron_Mode set", "~/reference/Contact_list.pdf", 3) },
                }),
            }); ;
        }

        #endregion Private Methods
    }
}