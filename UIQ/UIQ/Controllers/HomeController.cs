using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
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
        private readonly IndexSideEnum _indexSide;

        public HomeController(IHttpContextAccessor httpContextAccessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IUiqService uiqService, IOptions<RunningJobInfoOption> runningJobInfoOption, ILogger<HomeController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _uiqService = uiqService;
            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _indexSide = runningJobInfo == null ? IndexSideEnum.Default : runningJobInfo.IndexSide;
            _logger = logger;
        }

        public IActionResult Index()
        {
            int.TryParse(_uiqService.GetParameterItemAsync().GetAwaiter().GetResult()?.Parameter_Value, out var refreshSeconds);
            ViewBag.RefreshTimeSeconds = refreshSeconds;
            ViewBag.IndexSide = _indexSide;
            ViewBag.Menu = GenerateMenuItems().Where(x => x.Menu_Id != (int)MenuEnum.HomePage);

            var permissonMenus = _uiqService.GetMenuItemsWithPermissonAsync().GetAwaiter().GetResult();
            if (permissonMenus.Any(x => x.Menu_Id == (int)MenuEnum.HomePage) == false)
            {
                ViewBag.IsHomePagePermissonNotHave = false;
                return View();
            }

            var models = _uiqService.GetHomeTableDatas();
            ViewBag.IsHomePagePermissonNotHave = true;
            return View(models);
        }

        public IActionResult DetailedStatus(string modelMemberNickname)
        {
            int.TryParse(_uiqService.GetParameterItemAsync().GetAwaiter().GetResult()?.Parameter_Value, out var refreshSeconds);
            ViewBag.RefreshTimeSeconds = refreshSeconds;
            ViewBag.IndexSide = _indexSide;
            ViewBag.Menu = GenerateMenuItems().Where(x => x.Menu_Id != (int)MenuEnum.HomePage);

            var permissonMenus = _uiqService.GetMenuItemsWithPermissonAsync().GetAwaiter().GetResult();
            if (permissonMenus.Any(x => x.Menu_Id == (int)MenuEnum.HomePage) == false)
            {
                ViewBag.IsHomePagePermissonNotHave = false;
                return View();
            }

            var models = _uiqService.GetHomeTableDatas();
            ViewBag.IsHomePagePermissonNotHave = true;
            return View(models);
        }

        [HttpPost]
        public async Task<JsonResult> GetShellDelayData()
        {
            var userGroupName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var datas = await _uiqService.GetDelayDatasAsync(userGroupName);

            var response = new ApiResponse<IEnumerable<GetShellDelayDataViewModel>>(datas);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteShellDelayData(int id)
        {
            var resultCnt = await _uiqService.DeleteDelayDataAsync(id);
            var response = new ApiResponse<string>(data: resultCnt > 0 ? "Delete Success" : "Delete Fail");
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> LoadRejectLog()
        {
            var result = await _uiqService.CheckRejectStatusAsync();

            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteRejectLog()
        {
            var result = await _uiqService.DeleteRejectLogAsync();
            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private Methods

        private IEnumerable<MenuViewModel> GenerateMenuItems()
        {
            var allMenus = _uiqService.GetMenuItemsWithPermissonAsync().GetAwaiter().GetResult().ToList();
            var menus = allMenus.Where(x => x.Parent_Id == null).ToList();
            menus.ForEach(menu => { menu.ChildItems = allMenus.Where(x => x.Parent_Id == menu.Menu_Id).ToList(); });
            return menus;
        }

        #endregion Private Methods
    }
}