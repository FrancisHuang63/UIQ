using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
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
        private readonly ILogFileService _logFileService;
        private readonly IUploadFileService _uploadFileService;
        private readonly IEncodeService _urlEncodeService;
        private readonly string _hpcCtl;
        private readonly string _systemName;
        private readonly string _rshAccount;
        private readonly string _loginIp;
        private readonly string _cronIp;
        private readonly string _dataMvIp;
        private readonly string _uiPath;
        private readonly string _typhoonAccount;

        public MaintainToolsController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService
            , IHttpContextAccessor httpContextAccessor, ILogFileService logFileService, IUploadFileService uploadFileService, IEncodeService urlEncodeService)
        {
            _uiqService = uiqService;
            _httpContextAccessor = httpContextAccessor;
            _logFileService = logFileService;
            _uploadFileService = uploadFileService;
            _urlEncodeService = urlEncodeService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _systemName = configuration.GetValue<string>("SystemName");
            _rshAccount = configuration.GetValue<string>("RshAccount");
            _uiPath = configuration.GetValue<string>("UiPath");
            _typhoonAccount = configuration.GetValue<string>("TyphoonAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
            _cronIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.CronIp;
            _dataMvIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.DataMvIp;
        }

        [MenuPageAuthorize(Enums.MenuEnum.SetTyphoonData)]
        public IActionResult TyphoonInitialData()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveTyphoonData(string dtg, TyphoonSetDataViewModel[] typhoonSetDatas)
        {
            var datas = new List<string>();
            var dirNameParameters = new[]
            {
                new{ DirName = "ty", FilePrefix = "dat" },
                new{ DirName = "ty", FilePrefix = "txt" },
                new{ DirName = "tdty", FilePrefix = "dat" },
                new{ DirName = "tdty", FilePrefix = "txt" },
            };

            var datContents = new Dictionary<string, string>();
            var txtContents = new Dictionary<string, string>();
            foreach (var dirNameParameter in dirNameParameters.GroupBy(x => x.DirName))
            {
                var tmpDatContents = new List<string>();
                var tmpTxtContents = new List<string>();
                var dataCount = dirNameParameter.Key == "ty"
                        ? typhoonSetDatas.Count(x => !x.Name.StartsWith("TD"))
                        : typhoonSetDatas.Count();
                foreach (var data in typhoonSetDatas)
                {
                    if (dirNameParameter.Key == "ty" && data.Name.StartsWith("TD")) continue;

                    tmpDatContents.Add($"{data.Name.PadRight(8, ' ')} {data.Lat.Value.ToString(".0").PadLeft(4, ' ')} N {data.Lng.Value.ToString(".0").PadLeft(5, ' ')} E {data.LatBefore6Hours.Value.ToString(".0").PadLeft(4, ' ')} N {data.LngBefore6Hours.Value.ToString(".0").PadLeft(5, ' ')} E {data.CenterPressure.ToString().PadLeft(4, ' ')} MB {data.Radius15MPerS.ToString().PadLeft(3, ' ')} KM {data.MaximumSpeed.ToString().PadLeft(2, ' ')} M/S {data.Radius25MPerS.ToString().PadLeft(3, ' ')} KM");
                    tmpTxtContents.Add($"{data.Name.PadRight(15, ' ')} {data.Lat.Value.ToString(".0").PadLeft(4, ' ')} N {data.Lng.Value.ToString(".0").PadLeft(5, ' ')} E {data.LatBefore6Hours.Value.ToString(".0").PadLeft(4, ' ')} N {data.LngBefore6Hours.Value.ToString(".0").PadLeft(5, ' ')} E {data.CenterPressure.ToString().PadLeft(4, ' ')} MB {data.Radius15MPerS.ToString().PadLeft(3, ' ')} KM {data.MaximumSpeed.ToString().PadLeft(2, ' ')} M/S {data.Radius25MPerS.ToString().PadLeft(3, ' ')} KM");
                }

                datContents.Add(dirNameParameter.Key, $"{dataCount}\n{string.Join("\n", tmpDatContents)}");
                txtContents.Add(dirNameParameter.Key, $"20{dtg}\n{dataCount}\n{string.Join("\n", tmpTxtContents)}");
            }

            foreach (var dirNameParameter in dirNameParameters)
            {
                var command = string.Empty;
                var currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                var tmpPath = $"{_uiPath}wwwroot/temp/{dirNameParameter.DirName}";
                var tmpFilePath = $"{tmpPath}/typhoon.{dirNameParameter.FilePrefix}";
                var realDirectory = $"/{_systemName}/{_typhoonAccount}/TYP/M00/dtg/{dirNameParameter.DirName}/";
                var realFileName = $"typhoon{dtg}.{dirNameParameter.FilePrefix}";
                var realFilePath = realDirectory + realFileName;

                var newDataContent = (dirNameParameter.FilePrefix == "dat" ? string.Join("\n\n", datContents[dirNameParameter.DirName]) : string.Join("\n\n", txtContents[dirNameParameter.DirName]));
                //刪除舊檔
                if (System.IO.File.Exists(tmpFilePath))
                {
                    System.IO.File.Delete(tmpFilePath);
                }

                //寫入暫存檔
                await _logFileService.WriteDataIntoLogFileAsync(tmpPath, tmpFilePath, newDataContent);
                datas.Add($"Write {tmpFilePath}...");

                //檢查同 DTG 檔案是否已存在，存在則變更檔名
                if (System.IO.File.Exists(realFilePath))
                {
                    command = $"rsh -l {_typhoonAccount} {_loginIp} mv {realFilePath} {realFilePath}{currentTime}";
                    await _uiqService.RunCommandAsync(command);
                    datas.Add($"Rename {realFilePath} to {realFileName}{currentTime}...");
                }

                //將檔案傳回HPC主機
                command = $"rsh -l {_typhoonAccount} {_loginIp} cp {tmpFilePath} {realFilePath}";
                await _uiqService.RunCommandAsync(command);
                datas.Add($"Copy to ${realFilePath}...");
            }

            var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Edit the typhoon{dtg}.dat and typhoon{dtg}.txt\r\n";
            await _logFileService.WriteUiActionLogFileAsync(message);

            var response = new ApiResponse<IEnumerable<string>>(datas);
            return Json(response);
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
        public async Task<IActionResult> CommandEdit(Command data)
        {
            if (data == null) return RedirectToAction(nameof(Command));

            var result = await _uiqService.UpsertCommandAsync(data);
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

            _uiqService.SqlSync();
            switch (cronMode)
            {
                case "Normal":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_cronIp} /{_systemName}/{_hpcCtl}/shfun/shbin/Change_mode.ksh Normal");
                    break;

                case "Backup":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_cronIp} /{_systemName}/{_hpcCtl}/shfun/shbin/Change_mode.ksh Backup");
                    break;

                case "Typhoon":
                    //Do anything.
                    _uiqService.RunCommandAsync($"rsh -l {_hpcCtl} {_cronIp} /{_systemName}/{_hpcCtl}/shfun/shbin/Change_mode.ksh Typhoon");
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
        public async Task<JsonResult> ModelMemberSet(ModelMemberSetSaveDataViewModel data, int? memberId)
        {
            var result = new ApiResponse<string>("Error");
            if (data == null) return Json(result);

            var saveResult = await _uiqService.SaveModelMemberSetData(data);
            await _uiqService.SqlSync();

            result.Success = saveResult;
            return Json(result);
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
            ViewBag.Roles = _uiqService.GetRoleItemsAsync().GetAwaiter().GetResult();
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.UploadFile)]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile[] postedFiles, int[] roleIds, bool isAllRole)
        {
            ViewBag.Roles = _uiqService.GetRoleItemsAsync().GetAwaiter().GetResult();
            if (postedFiles == null || postedFiles.Any() == false)
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

            await _uiqService.SetUploadFileItems(uploadFileDatas, isAllRole ? new int[] { Common.ALL_ROLE_PERMISSON_ID } : roleIds);
            ViewBag.Message = "Upload success!!";
            return View();
        }

        [MenuPageAuthorize(Enums.MenuEnum.UploadFile)]
        [HttpPost]
        public async Task<ApiResponse<string>> DeleteUploadFile(int? fileId)
        {
            if (fileId == null) return new ApiResponse<string>("") { Message = "File sn is error" };

            var result = await _uiqService.DeleteUploadFile(fileId.Value);
            return new ApiResponse<string>("") { Success = result, Message = $"File delete {(result ? "success" : "error")}" };
        }

        [MenuPageAuthorize(Enums.MenuEnum.DownloadFile)]
        public IActionResult DownloadFile()
        {
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
        public JsonResult DeleteModel(int? memberId, int? model_Id)
        {
            if (model_Id.HasValue == false) return Json(new ApiResponse<string>("Model id is null"));

            _uiqService.DeleteModelAsync(model_Id.Value);
            _uiqService.SqlSync();
            return Json(new ApiResponse<string>(data: "success"));
        }

        [HttpPost]
        public IActionResult DeleteMember(int? memberId)
        {
            if (memberId.HasValue == false) return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });

            _uiqService.DeleteMemberAsync(memberId.Value);
            _uiqService.SqlSync();
            return RedirectToAction(nameof(ModelMemberSet), new { memberId = memberId });
        }

        [HttpPost]
        public async Task<JsonResult> GetShell(CheckPointInfoViewModel data)
        {
            var command = $"rsh -l {_rshAccount} {_loginIp} cat /{_systemName}/{_hpcCtl}/shfun/shetc/setMode | awk " + " '{print $1}'";
            var cron_mode = await _uiqService.RunCommandAsync(command);
            data.Cron_Mode = Regex.Replace(cron_mode, "/\\s\\s+/", "").Trim();

            var result = await _uiqService.GetShell(data);
            return Json(new ApiResponse<IEnumerable<CheckPointInfoResultViewModel>>(result));
        }

        [HttpPost]
        public async Task<JsonResult> GetUnselectedShell(CheckPointInfoViewModel data)
        {
            var result = await _uiqService.GetUnselectedShell(data);
            return Json(new ApiResponse<IEnumerable<CheckPointInfoResultViewModel>>(result));
        }

        [HttpPost]
        public async Task<JsonResult> GetCheckPoints(int? memberId)
        {
            if (memberId.HasValue == false) return Json(new ApiResponse<IEnumerable<ShowCheckPointInfoViewModel>>(new List<ShowCheckPointInfoViewModel>()));

            var datas = await _uiqService.GetShowCheckPointInfoDatas(memberId.Value);
            return Json(new ApiResponse<IEnumerable<ShowCheckPointInfoViewModel>>(datas));
        }

        [HttpPost]
        public async Task<JsonResult> GetCommandInfo(int? commandId)
        {
            if (commandId.HasValue == false) return Json(null);
            var command = await _uiqService.GetCommandItemAsync(commandId.Value);
            return Json(command);
        }

        [HttpPost]
        public async Task<JsonResult> CommandExecute(int commandId, string parameters, string command)
        {
            parameters = _urlEncodeService.HtmlEncode(parameters);
            command = _urlEncodeService.HtmlEncode(command);

            var commandItem = await _uiqService.GetCommandItemAsync(commandId);
            if (commandItem == null) return Json(new ApiResponse<string>("Error"));

            command = string.IsNullOrWhiteSpace(command) ? commandItem.Command_Content : command;
            command = string.IsNullOrWhiteSpace(parameters) ? command : command + " '\\\"$parameters\\\"'";
            var result = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<string>(data: string.IsNullOrWhiteSpace(result) ? "No Result" : result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> CalculateCommandExecuteTime(int commandId, string parameters, string passwd, string command, int execTime)
        {
            parameters = _urlEncodeService.HtmlEncode(parameters);
            passwd = _urlEncodeService.HtmlEncode(passwd);
            command = _urlEncodeService.HtmlEncode(command);

            var commandItem = await _uiqService.GetCommandItemAsync(commandId);
            if (commandItem == null) return Json(new ApiResponse<string>("Error"));

            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()) == false
                && passwd != commandItem.Command_Pwd)
            {
                return Json(new ApiResponse<string>("Your password is Wrong!!!!"));
            }

            command = string.IsNullOrWhiteSpace(parameters) ? command : command + $" '\\\"{parameters}\\\"'";
            command = $@"rsh -l {_hpcCtl} {_loginIp} ""{(command ?? string.Empty).Split("\n").FirstOrDefault()}"" 2>&1";
            var result = await _uiqService.RunCommandAsync(command);

            var datas = new { Result = result, StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), EstimatedCompletionTime = DateTime.Now.AddMinutes(execTime).ToString("yyyy/MM/dd HH:mm:ss") };
            var response = new ApiResponse<dynamic>(datas);
            return Json(response);
        }

        [HttpPost]
        public JsonResult GetUploadFile(int jtStartIndex = 0, int jtPageSize = 20, bool isUnPermisson = false)
        {
            var datas = _uiqService.GetUploadFilePageItems(jtStartIndex, jtPageSize, isUnPermisson, out var totalCnt).ToList();
            return new JsonResult(new PageDataResponse<IEnumerable<UploadFile>>(datas, totalCnt));
        }
    }
}