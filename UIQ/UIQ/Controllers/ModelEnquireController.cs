using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    [Authorize]
    public class ModelEnquireController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly string _hpcCtl;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public ModelEnquireController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService)
        {
            _uiqService = uiqService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        public IActionResult ModelLogFiles()
        {
            return View();
        }

        public IActionResult ModelLogFilesSub()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        public IActionResult RunningStatus()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        public IActionResult DailyReport()
        {
            var command = $"rsh -l ${_rshAccount} ${_loginIp} ls /ncs/{_hpcCtl}/daily_log/ | grep -v old_log";
            ViewBag.Data = _uiqService.RunCommandAsync(command).GetAwaiter().GetResult();
            return View();
        }

        public IActionResult TyphoonInitialData()
        {
            var command = $"rsh -l {_rshAccount} {_loginIp} cat /ncs/ncsatyp/TYP/M00/tmp/typhoon.ini";
            ViewBag.Data = _uiqService.RunCommandAsync(command).GetAwaiter().GetResult();
            return View();
        }

        public IActionResult BatchHistory()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetModelItems()
        {
            var datas = _uiqService.GetModelItemsAsync().GetAwaiter().GetResult();
            var models = datas.OrderBy(x => x.Model_Position).Select(x => x.Model_Name).Distinct();
            return new JsonResult(new ApiResponse<IEnumerable<string>>(models) { Success = true });
        }

        [HttpPost]
        public JsonResult GetMemberItems(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName)) return new JsonResult(new ApiResponse<IEnumerable<string>>(new string[] { }));

            var datas = _uiqService.GetModelLogFileViewModels();
            var members = datas.Where(x => x.Model_Name == modelName).Select(x => x.Member_Name).Distinct();
            return new JsonResult(new ApiResponse<IEnumerable<string>>(members) { Success = true });
        }

        [HttpPost]
        public JsonResult GetNicknameItems(string modelName, string memberName)
        {
            if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(memberName)) return new JsonResult(new ApiResponse<IEnumerable<string>>(new string[] { }));

            var datas = _uiqService.GetModelLogFileViewModels();
            var nickNames = datas.Where(x => x.Model_Name == modelName && x.Member_Name == memberName).Select(x => x.Member_Name).Distinct();
            return new JsonResult(new ApiResponse<IEnumerable<string>>(nickNames) { Success = true });
        }

        [HttpPost]
        public JsonResult GetModelTimeTableData(string modelName, string memberName, string nickname, int jtStartIndex = 0, int jtPageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(memberName) || string.IsNullOrWhiteSpace(nickname))
                return new JsonResult(new PageDataResponse<object>());

            var datas = _uiqService.GetModelTimeDatas(modelName, memberName, nickname, jtStartIndex, jtPageSize, out var totalCnt);

            return new JsonResult(new PageDataResponse<IEnumerable<ModelTimeViewModel>>(datas, totalCnt));
        }

        [HttpPost]
        public JsonResult GetBatchDetailTableData(string modelName, string memberName, string nickname, string runType, int typhoonMode, string cronMode, string round)
        {
            var param = new BatchDetailViewModelSearchParameter
            {
                ModelName = modelName,
                MemberName = memberName,
                Nickname = nickname,
                RunType = runType,
                TyphoonMode = typhoonMode,
                CronMode = cronMode,
                Round = round,
            };
            var datas = _uiqService.GetBatchDetailDatas(param);

            return new JsonResult(new PageDataResponse<IEnumerable<BatchDetailViewModel>>(datas, datas.Count()));
        }

        [HttpPost]
        public JsonResult GetShellDetailTableData(string modelName, string memberName, string nickname, string runType, int typhoonMode, string cronMode, string round)
        {
            var param = new ShellDetailViewModelSearchParameter
            {
                ModelName = modelName,
                MemberName = memberName,
                Nickname = nickname,
                RunType = runType,
                TyphoonMode = typhoonMode,
                CronMode = cronMode,
                Round = round,
            };
            var datas = _uiqService.GetShellDetailDatas(param);

            return new JsonResult(new PageDataResponse<IEnumerable<ShellDetailViewModel>>(datas, datas.Count()));
        }
    }
}