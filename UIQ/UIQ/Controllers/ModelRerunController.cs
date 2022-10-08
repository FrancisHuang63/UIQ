using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Attributes;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [Authorize]
    public class ModelRerunController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly string _hpcCtl;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public ModelRerunController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService)
        {
            _uiqService = uiqService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        [MenuPageAuthorize(Enums.MenuEnum.ResetModel)]
        public IActionResult ResetModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.DtgAdjust)]
        public IActionResult DtgAdjust()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.LidAdjust)]
        public IActionResult LidAdjust()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.SubmitModel)]
        public IActionResult SubmitModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }
    }
}