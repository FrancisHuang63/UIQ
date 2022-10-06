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

        public HomeController(IHttpContextAccessor httpContextAccessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IUiqService uiqService, ILogger<HomeController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _uiqService = uiqService;
            _logger = logger;
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
            var models = _uiqService.GetHomeTableDatas();
            var menus = GenerateMenuItems();

            ViewBag.Menu = menus;
            return View(models);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private Methods

        private IEnumerable<MenuViewModel> GenerateMenuItems()
        {
            var menus = new List<MenuViewModel>(new MenuViewModel[]
            {
                //System enquire
                new MenuViewModel("System enquire", 1, new List<MenuViewModel>
                {
                    { new MenuViewModel("NWP running jobs", Url.Action(nameof(HpcEnquireController.NwpRunningJobs), "HpcEnquire"), 1) },
                    { new MenuViewModel("CRON table status", Url.Action(nameof(HpcEnquireController.CronStatus), "HpcEnquire"), 2) },
                    { new MenuViewModel("HPC system status", Url.Action(nameof(HpcEnquireController.HpcStatus), "HpcEnquire"), 3)},
                }),
                //Model enquire
                new MenuViewModel("Model enquire", 2, new List<MenuViewModel>
                {
                    { new MenuViewModel("Job log check", Url.Action(nameof(ModelEnquireController.ModelLogFiles), "ModelEnquire"), 1) },
                    { new MenuViewModel("Running status", Url.Action(nameof(ModelEnquireController.RunningStatus), "ModelEnquire"), 2) },
                    { new MenuViewModel("Daily report", Url.Action(nameof(ModelEnquireController.DailyReport), "ModelEnquire"), 3)},
                    { new MenuViewModel("Typhoon data check", Url.Action(nameof(ModelEnquireController.TyphoonInitialData), "ModelEnquire"), 4) },
                    { new MenuViewModel("Batch history", Url.Action(nameof(ModelEnquireController.BatchHistory), "ModelEnquire"), 5) },
                }),
                //Model rerun
                new MenuViewModel("Model rerun", 3, new List<MenuViewModel>
                {
                    { new MenuViewModel("Cancel running job", Url.Action(nameof(ModelRerunController.ResetModel), "ModelRerun"), 1) },
                    { new MenuViewModel("Dtg adjust", Url.Action(nameof(ModelRerunController.DtgAdjust), "ModelRerun"), 2) },
                    { new MenuViewModel("Lid adjust", Url.Action(nameof(ModelRerunController.LidAdjust), "ModelRerun"), 3) },
                    { new MenuViewModel("Submit model", Url.Action(nameof(ModelRerunController.SubmitModel), "ModelRerun"), 4) },
                }),
                //Rescue tools
                new MenuViewModel("Rescue tools", 4, new List<MenuViewModel>
                {
                    { new MenuViewModel("Archive redo", Url.Action(nameof(RescueToolsController.ArchiveRedo), "RescueTools"), 1) },
                    { new MenuViewModel("Model output generate", Url.Action(nameof(RescueToolsController.FixFailedModel), "RescueTools"), 2) },
                }),
                //Maintain tools
                new MenuViewModel("Maintain tools", 5, new List<MenuViewModel>
                {
                    { new MenuViewModel("Set typhoon data", Url.Action(nameof(MaintainToolsController.TyphoonInitialData), "MaintainTools"), 1) },
                    { new MenuViewModel("Special Use command", Url.Action(nameof(MaintainToolsController.Command), "MaintainTools"), 2) },
                    { new MenuViewModel("Cron_Mode set", Url.Action(nameof(MaintainToolsController.CronSetting), "MaintainTools"), 3) },
                    { new MenuViewModel("Update File", Url.Action(nameof(MaintainToolsController.UploadFile), "MaintainTools"), 5) },
                    { new MenuViewModel("Permission Setting", Url.Action(nameof(MaintainToolsController.PermissionSetting), "MaintainTools"), 6) },
                    { new MenuViewModel("Parameter Setting", Url.Action(nameof(MaintainToolsController.ParameterSetting), "MaintainTools"), 7) },
                }),
                //Reference info
                new MenuViewModel("Reference info", 6, new List<MenuViewModel>
                {
                    { new MenuViewModel("UI user guide", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/reference/user_guide.pdf", 1) },
                    { new MenuViewModel("Monitor reference", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/reference/Monitor_reference.pdf", 2) },
                    { new MenuViewModel("Contact list", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/reference/Contact_list.pdf", 3) },
                }),
            });

            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()))
            {
                menus[4].ChildItems.Add(new MenuViewModel("Model_Member set", Url.Action(nameof(MaintainToolsController.ModelMemberSet), "MaintainTools"), 4));
                menus[5].ChildItems.AddRange(new List<MenuViewModel>
                {
                    { new MenuViewModel("FX100 Model and node", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/reference/FX100_Model_and_node.pdf", 4) },
                    { new MenuViewModel("FX10 Model and node", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/reference/FX10_Model_and_node.pdf", 5) },
                });
            }

            return menus;
        }

        #endregion Private Methods
    }
}