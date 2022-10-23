﻿using Microsoft.AspNetCore.Authorization;
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
            var models = _uiqService.GetHomeTableDatas();
            var menus = GenerateMenuItems();
            int.TryParse(_uiqService.GetParameterItemAsync().GetAwaiter().GetResult()?.Parameter_Value, out var refreshSeconds);

            ViewBag.Menu = menus;
            ViewBag.RefreshTimeSeconds = refreshSeconds;
            ViewBag.IndexSide = _indexSide;
            return View(models);
        }

        public IActionResult DetailedStatus(string modelMemberNickname)
        {
            var models = _uiqService.GetHomeTableDatas();
            var menus = GenerateMenuItems();
            int.TryParse(_uiqService.GetParameterItemAsync().GetAwaiter().GetResult()?.Parameter_Value, out var refreshSeconds);

            ViewBag.Menu = menus;
            ViewBag.RefreshTimeSeconds = refreshSeconds;
            ViewBag.IndexSide = _indexSide;
            return View(models);
        }

        [HttpPost]
        public async Task<JsonResult> GetShellDelayData()
        {
            var userGroupName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var datas = await _uiqService.GetDelayDatasAsync(userGroupName);
            return Json(datas);
        }

        [HttpPost]
        public async Task<ContentResult> DeleteShellDelayData(int id)
        {
            var resultCnt = await _uiqService.DeleteDelayDataAsync(id);
            return new ContentResult { Content = resultCnt > 0 ? "Delete Success" : "Delete Fail", ContentType = "text/html" };
        }

        [HttpPost]
        public async Task<JsonResult> LoadRejectLog()
        {
            var result = await _uiqService.CheckRejectStatusAsync();
            return Json(result);
        }

        [HttpPost]
        public async Task<ContentResult> DeleteRejectLog()
        {
            var result = await _uiqService.DeleteRejectLogAsync();
            return new ContentResult { Content = result, ContentType = "text/html" };
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