using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
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

        public IActionResult ResetModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        public IActionResult DtgAdjust()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        public IActionResult LidAdjust()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        public IActionResult SubmitModel()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }
    }
}