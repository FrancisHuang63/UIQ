using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Attributes;
using UIQ.Enums;
using UIQ.Models;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    [Authorize]
    public class MaintainToolsController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReadLogFileService _readLogFileService;
        private readonly IUploadFileService _uploadFileService;
        private readonly string _hpcCtl;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public MaintainToolsController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService
            , IHttpContextAccessor httpContextAccessor, IReadLogFileService readLogFileService, IUploadFileService uploadFileService)
        {
            _uiqService = uiqService;
            _httpContextAccessor = httpContextAccessor;
            _readLogFileService = readLogFileService;
            _uploadFileService = uploadFileService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        [MenuPageAuthorize(Enums.MenuEnum.SetTyphoonData)]
        public IActionResult TyphoonInitialData()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> SaveTyphoonData(string dtg, TyphoonSetDataViewModel[] typhoonSetDatas)
        {
            var dirNameParameters = new[]
            {
                new{ DirName = "ty", FilePrefix = "dat" },
                new{ DirName = "ty", FilePrefix = "txt" },
                new{ DirName = "tdty", FilePrefix = "dat" },
                new{ DirName = "tdty", FilePrefix = "txt" },
            };

            var datContents = new List<string>();
            var txtContents = new List<string>();
            foreach (var data in typhoonSetDatas)
            {
                datContents.Add($"{typhoonSetDatas.Count()}\n{data.Name.PadRight(9, ' ')} {data.Lat.Value} N   {data.Lng} E  {data.LatBefore6Hours} N   {data.LngBefore6Hours} E    {data.CenterPressure} MB   {data.Radius15MPerS} KM  {data.MaximumSpeed} M/S   {data.Radius25MPerS} KM");
                txtContents.Add($"20{dtg}\n{typhoonSetDatas.Count()}\n{data.Name.PadRight(16, ' ')} {data.Lat.Value} N   {data.Lng} E  {data.LatBefore6Hours} N   {data.LngBefore6Hours} E    {data.CenterPressure} MB   {data.Radius15MPerS} KM  {data.MaximumSpeed} M/S   {data.Radius25MPerS} KM");
            }

            foreach (var dirNameParameter in dirNameParameters)
            {
                var directoryPath = $"{_readLogFileService.RootPath}\\temp\\{dirNameParameter.DirName}";
                var fileName = $"typhoon.{dirNameParameter.FilePrefix}";
                var fullFilePath = $"{directoryPath}\\{fileName}";

                var newDataContent = "\n" + (dirNameParameter.FilePrefix == "dat" ? string.Join("\n\n", datContents) : string.Join("\n\n", txtContents));
                await _readLogFileService.WriteDataIntoLogFileAsync(directoryPath, fullFilePath, newDataContent);
            }

            return "Success";
        }

        [MenuPageAuthorize(Enums.MenuEnum.Command)]
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

        [MenuPageAuthorize(Enums.MenuEnum.CronSetting)]
        public IActionResult CronSetting()
        {
            var model = _uiqService.GetCronSettingViewModels();
            return View(model);
        }

        [MenuPageAuthorize(Enums.MenuEnum.CronSetting)]
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

        [MenuPageAuthorize(Enums.MenuEnum.ModelMemberSet)]
        public IActionResult ModelMemberSet(int? memberId)
        {
            ViewBag.IsNew = memberId == null;
            ViewBag.ModelItems = _uiqService.GetModelItemsAsync().GetAwaiter().GetResult();
            ViewBag.DataItems = _uiqService.GetDataItemsAsync().GetAwaiter().GetResult();
            ViewBag.WorkItems = _uiqService.GetWorkItemsAsync().GetAwaiter().GetResult();
            ViewBag.CronTabItems = memberId.HasValue ? _uiqService.GetCronTabItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<CronTab> { new CronTab { Cron_Group = "Normal", Master_Group = "Normal" } };
            ViewBag.BatchItems = memberId.HasValue ? _uiqService.GetBatchItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Batch> { new Batch() };
            ViewBag.ArchiveItems = memberId.HasValue ? _uiqService.GetArchiveItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Archive> { new Archive() };
            ViewBag.OutputItems = memberId.HasValue ? _uiqService.GetOutputItemsAsync(memberId.Value).GetAwaiter().GetResult() : new List<Output> { new Output() };
            var model = memberId.HasValue ? _uiqService.GetMemberItemAsync(memberId.Value).GetAwaiter().GetResult() : null;
            return View(model);
        }

        [MenuPageAuthorize(Enums.MenuEnum.ModelMemberSet)]
        [HttpPost]
        public IActionResult ModelMemberSet(ModelMemberSetSaveDataViewModel data, int? memberId)
        {
            if (data == null) return View("Error");

            _uiqService.SaveModelMemberSetData(data);

            return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });
        }

        [MenuPageAuthorize(Enums.MenuEnum.PermissionSetting)]
        public IActionResult PermissionSetting()
        {
            var model = _uiqService.GetRoleItemsAsync().GetAwaiter().GetResult();
            return View(model);
        }

        [MenuPageAuthorize(Enums.MenuEnum.PermissionSetting)]
        public IActionResult PermissionSetting_MenuSet(int? roleId)
        {
            var menus = _uiqService.GetMenuRoleSetItemsAsync(roleId).GetAwaiter().GetResult().ToList();
            var role = roleId.HasValue ? _uiqService.GetRoleItemAsync(roleId.Value).GetAwaiter().GetResult() : null;
            ViewBag.Role = role;
            return View(menus);
        }

        [MenuPageAuthorize(Enums.MenuEnum.PermissionSetting)]
        [HttpPost]
        public IActionResult PermissionSetting_MenuSet(int? roleId, string roleName, int[] menuIds)
        {
            var actualRoleId = roleId ?? 0;
            if (roleId == null) _uiqService.AddNewRole(roleName, out actualRoleId);
            else _uiqService.UpdateRoleAsync(roleId.Value, roleName);

            _uiqService.UpdateMenuToRole(actualRoleId, menuIds);
            return RedirectToAction(nameof(PermissionSetting));
        }

        [MenuPageAuthorize(Enums.MenuEnum.PermissionSetting)]
        public IActionResult PermissionSetting_UserSet(int roleId)
        {
            var users = _uiqService.GetUserRoleSetItemsAsync(roleId).GetAwaiter().GetResult().ToList();
            var role = _uiqService.GetRoleItemAsync(roleId).GetAwaiter().GetResult();
            ViewBag.Role = role;
            return View(users);
        }

        [MenuPageAuthorize(Enums.MenuEnum.PermissionSetting)]
        [HttpPost]
        public IActionResult PermissionSetting_UserSet(int roleId, int[] userIds)
        {
            _uiqService.UpdateUserToRole(roleId, userIds);
            return RedirectToAction(nameof(PermissionSetting));
        }

        [MenuPageAuthorize(Enums.MenuEnum.UploadFile)]
        public IActionResult UploadFile()
        {
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.UploadFile)]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile[] postedFiles)
        {
            if (postedFiles?.Any() == false)
            {
                ViewBag.Message = "No file!!";
                return View();
            }

            var uploadFileDatas = new List<UploadFile>();
            foreach (var file in postedFiles)
            {
                if (file.Length == 0) continue;
                await _uploadFileService.UploadFileAsync(file);

                var fileName = Path.GetFileName(file.FileName);
                var filePath = _uploadFileService.GetUploadPathUrl(fileName);
                uploadFileDatas.Add(new UploadFile(fileName, filePath));
            }

            await _uiqService.SetUploadFileItems(uploadFileDatas);
            ViewBag.Message = "Upload success!!";
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.ParameterSetting)]
        public IActionResult ParameterSetting()
        {
            var model = _uiqService.GetParameterItemAsync().GetAwaiter().GetResult();
            return View(model);
        }

        [MenuPageAuthorize(Enums.MenuEnum.ParameterSetting)]
        [HttpPost]
        public IActionResult ParameterSetting(Parameter data)
        {
            if (data == null) return RedirectToAction(nameof(ParameterSetting));

            _uiqService.UpdateParameterAsync(data);
            return RedirectToAction(nameof(ParameterSetting));
        }

        [HttpPost]
        public IActionResult DeleteModel(int? memberId, int? model_Id)
        {
            if (model_Id.HasValue == false) return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });

            _uiqService.DeleteModelAsync(model_Id.Value);
            return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });
        }

        [HttpPost]
        public IActionResult DeleteMember(int? memberId)
        {
            if (memberId.HasValue == false) return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });

            _uiqService.DeleteMemberAsync(memberId.Value);
            return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });
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

        [HttpPost]
        public JsonResult GetUploadFile(int jtStartIndex = 0, int jtPageSize = 20)
        {
            var datas = _uiqService.GetUploadFilePageItems(jtStartIndex, jtPageSize, out var totalCnt).ToList();
            return new JsonResult(new PageDataResponse<IEnumerable<UploadFile>>(datas, totalCnt));
        }
    }
}