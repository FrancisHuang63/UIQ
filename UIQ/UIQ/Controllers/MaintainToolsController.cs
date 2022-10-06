using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Enums;
using UIQ.Models;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    [Authorize]
    public class MaintainToolsController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _hpcCtl;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public MaintainToolsController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService, IHttpContextAccessor httpContextAccessor)
        {
            _uiqService = uiqService;
            _httpContextAccessor = httpContextAccessor;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        public IActionResult TyphoonInitialData()
        {
            return View();
        }

        public IActionResult Command()
        {
            var model = _uiqService.GetCommandItemsAsync().GetAwaiter().GetResult();
            return View(model);
        }

        public IActionResult CommandEdit(int? commandId)
        {
            var model = commandId.HasValue ? _uiqService.GetCommandItemsAsync().GetAwaiter().GetResult().FirstOrDefault(x => x.Command_Id == commandId.Value) : null;
            return View(model);
        }

        [HttpPost]
        public IActionResult CommandEdit(Command data)
        {
            if (data == null) return RedirectToAction(nameof(Command));

            _uiqService.UpsertCommandAsync(data);
            return RedirectToAction(nameof(Command));
        }

        public IActionResult CommandDelete(int commandId)
        {
            _uiqService.DeleteCommandAsync(commandId);
            return RedirectToAction(nameof(Command));
        }

        public IActionResult CronSetting()
        {
            var model = _uiqService.GetCronSettingViewModels();
            return View(model);
        }

        [HttpPost]
        public IActionResult CronSetting(string cronMode)
        {
            _uiqService.UpdateCrontabMasterGroup(cronMode);
            _uiqService.UpdateGroupValidationWhoHasCronMode(cronMode);
            _uiqService.UpdateGroupValidationWhoNotHasCronMode(cronMode);

            switch (cronMode)
            {
                case "Normal":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_httpContextAccessor.HttpContext.Request.Host} /ncs/${_hpcCtl}/shfun/shbin/Change_mode.ksh Normal");
                    break;

                case "Backup":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_httpContextAccessor.HttpContext.Request.Host} /ncs/{_hpcCtl}/shfun/shbin/Change_mode.ksh Backup");
                    break;

                case "Typhoon":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_httpContextAccessor.HttpContext.Request.Host} /ncs/{_hpcCtl}/shfun/shbin/Change_mode.ksh Typhoon");
                    break;

                default:
                    break;
            }

            var model = _uiqService.GetCronSettingViewModels();
            return View(model);
        }

        public IActionResult ModelMemberSet(int? memberId)
        {
            ViewBag.IsNew = memberId == null;
            ViewBag.ModelItems = _uiqService.GetModelItemsAsync().GetAwaiter().GetResult();
            ViewBag.DataItems = _uiqService.GetDataItemsAsync().GetAwaiter().GetResult();
            ViewBag.WorkItems = _uiqService.GetWorkItemsAsync().GetAwaiter().GetResult();
            ViewBag.CronTabItems = memberId.HasValue ? _uiqService.GetCronTabItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<CronTab> { new CronTab { Cron_Group = "Normal" } };
            ViewBag.BatchItems = memberId.HasValue ? _uiqService.GetBatchItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Batch> { new Batch() };
            ViewBag.ArchiveItems = memberId.HasValue ? _uiqService.GetArchiveItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Archive> { new Archive() };
            ViewBag.OutputItems = memberId.HasValue ? _uiqService.GetOutputItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Output> { new Output() };
            var model = memberId.HasValue ? _uiqService.GetMemberItemAsync(memberId.Value).GetAwaiter().GetResult() : null;
            return View(model);
        }

        [HttpPost]
        public IActionResult ModelMemberSet()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeleteModel()
        {
            return null;
        }

        [HttpPost]
        public async Task<JsonResult> GetCommandInfo(int? commandId)
        {
            if (commandId.HasValue == false) return Json(null);
            var command = await _uiqService.GetCommandItemAsync(commandId.Value);

            return Json(command);
        }

        [HttpPost]
        public async Task<string> CommandExecute(int commandId, string parameters, string passwd, string command, string execTime)
        {
            var commandItem = await _uiqService.GetCommandItemAsync(commandId);
            if (commandItem == null) return null;

            command = string.IsNullOrWhiteSpace(command) ? commandItem.Command_Content : command;
            command = string.IsNullOrWhiteSpace(parameters) ? command : command + " '\\\"$parameters\\\"'";
            command = $"rsh -l {_hpcCtl} {_loginIp} \"{commandItem.Command_Content}\" 2>&1";
            var result = await _uiqService.RunCommandAsync(command);
            return string.IsNullOrWhiteSpace(result) ? "No Result" : result;
        }

        [HttpPost]
        public async Task<string> CalculateCommandExecuteTime(int commandId, string parameters, string passwd, string command, int execTime)
        {
            var commandItem = await _uiqService.GetCommandItemAsync(commandId);
            if (commandItem == null) return null;

            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()) == false
                && passwd != commandItem.Command_Pwd)
            {
                return "Your password is Wrong!!!!";
            }

            command = string.IsNullOrWhiteSpace(parameters) ? command : command + " '\\\"$parameters\\\"'";
            var html = $@"rsh -l {_hpcCtl} {_loginIp} ""{command}"" 2>&1<br><br>";
            html += $@"Estimated Completion Time: <mark>{DateTime.Now.AddMinutes(execTime).ToString("yyyy/MM/dd HH:mm:ss")}</mark><br>
                      <mark>Please wait...</mark>\n<br>\n<br>";
            return html;
        }

        public IActionResult PermissionSetting() 
        {
            var model = _uiqService.GetRoleItemsAsync().GetAwaiter().GetResult();
            return View(model);
        }

        public IActionResult PermissionSetting_MenuSet()
        {
            return View();
        }
        public IActionResult PermissionSetting_UserSet()
        {
            return View();
        }

        public IActionResult UploadFile()
        {
            var model = _uiqService.GetUploadFileItemsAsync().GetAwaiter().GetResult();
            return View(model);
        }

        public IActionResult ParameterSetting()
        {
            return View();
        }
    }
}