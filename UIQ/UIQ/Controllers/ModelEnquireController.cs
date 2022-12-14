using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Attributes;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    [Authorize]
    public class ModelEnquireController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IEncodeService _encodeService;
        private readonly string _hpcCtl;
        private readonly string _systemName;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public ModelEnquireController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption
            , IUiqService uiqService, IEncodeService encodeService)
        {
            _uiqService = uiqService;
            _encodeService = encodeService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _systemName = configuration.GetValue<string>("SystemName");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        [MenuPageAuthorize(Enums.MenuEnum.ModelLogFiles)]
        public IActionResult ModelLogFiles()
        {
            return View();
        }

        public IActionResult ModelLogFilesSub()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.RunningStatus)]
        public IActionResult RunningStatus()
        {
            ViewBag.ConfigList = _uiqService.GetModelLogFileViewModels();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.DailyReport)]
        public IActionResult DailyReport()
        {
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls /{_systemName}/{_hpcCtl}/daily_log/ | grep -v old_log";
            var data = _uiqService.RunCommandAsync(command).GetAwaiter().GetResult();
            ViewBag.Data = (data ?? string.Empty).Split("\n").Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.TyphoonInitialData)]
        public IActionResult TyphoonInitialData()
        {
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat /{_systemName}/ncsatyp/TYP/M00/tmp/typhoon.ini";
            ViewBag.Data = _uiqService.RunCommandAsync(command).GetAwaiter().GetResult();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.BatchHistory)]
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
            modelName = _encodeService.HtmlEncode(modelName);

            if (string.IsNullOrWhiteSpace(modelName)) return new JsonResult(new ApiResponse<IEnumerable<string>>(new string[] { }));

            var datas = _uiqService.GetModelLogFileViewModels().Where(x => x.Md_Name == modelName);
            var memberNames = datas.Select(x => x.Mb_Name).Distinct().ToList();
            return new JsonResult(new ApiResponse<IEnumerable<string>>(memberNames) { Success = true });
        }

        [HttpPost]
        public JsonResult GetNicknameItems(string modelName, string memberName)
        {
            modelName = _encodeService.HtmlEncode(modelName);
            memberName = _encodeService.HtmlEncode(memberName);

            if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(memberName)) return new JsonResult(new ApiResponse<IEnumerable<string>>(new string[] { }));

            var datas = _uiqService.GetModelLogFileViewModels().Where(x => x.Md_Name == modelName && x.Mb_Name == memberName);
            var nickNames = datas.Select(x => x.Nickname).Distinct().ToList();
            return new JsonResult(new ApiResponse<IEnumerable<string>>(nickNames) { Success = true });
        }

        [HttpPost]
        public JsonResult GetModelTimeTableData(string modelName, string memberName, string nickname, int jtStartIndex = 0, int jtPageSize = 20)
        {
            modelName = _encodeService.HtmlEncode(modelName);
            memberName = _encodeService.HtmlEncode(memberName);
            nickname = _encodeService.HtmlEncode(nickname);

            if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(memberName) || string.IsNullOrWhiteSpace(nickname))
                return new JsonResult(new PageDataResponse<object>());

            var datas = _uiqService.GetModelTimeDatas(modelName, memberName, nickname, jtStartIndex, jtPageSize, out var totalCnt);

            return new JsonResult(new PageDataResponse<IEnumerable<ModelTimeViewModel>>(datas, totalCnt));
        }

        [HttpPost]
        public JsonResult GetBatchDetailTableData(string modelName, string memberName, string nickname, string runType, int typhoonMode, string cronMode, string round)
        {
            modelName = _encodeService.HtmlEncode(modelName);
            memberName = _encodeService.HtmlEncode(memberName);
            nickname = _encodeService.HtmlEncode(nickname);
            runType = _encodeService.HtmlEncode(runType);
            cronMode = _encodeService.HtmlEncode(cronMode);
            round = _encodeService.HtmlEncode(round);

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
            modelName = _encodeService.HtmlEncode(modelName);
            memberName = _encodeService.HtmlEncode(memberName);
            nickname = _encodeService.HtmlEncode(nickname);
            runType = _encodeService.HtmlEncode(runType);
            cronMode = _encodeService.HtmlEncode(cronMode);
            round = _encodeService.HtmlEncode(round);

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