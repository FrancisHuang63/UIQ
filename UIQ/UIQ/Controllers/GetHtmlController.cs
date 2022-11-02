using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    public class GetHtmlController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly ILogFileService _logFileService;
        private readonly string _hpcCtl;
        private readonly string _systemName;
        private readonly string _uiPath;
        private readonly string _systemDirectoryName;
        private readonly string _rshAccount;
        private readonly string _loginIp;
        private readonly string _prefix;
        private readonly string _dataMvIp;

        public GetHtmlController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService, ILogFileService logFileService)
        {
            _uiqService = uiqService;
            _logFileService = logFileService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _systemName = configuration.GetValue<string>("SystemName");
            _uiPath = configuration.GetValue<string>("UiPath");
            _systemDirectoryName = configuration.GetValue<string>("SystemDirectoryName");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
            _prefix = runningJobInfo?.Items?.FirstOrDefault()?.Datas?.FirstOrDefault()?.Prefix;
            _dataMvIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.DataMvIp;
        }

        [HttpPost]
        public async Task<JsonResult> ModelLogEnquire(string modelName, string memberName, string nickname)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} ls {fullPath}/log | grep job | grep -v job1 | grep -v OP_";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n").ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ModelLogResult(string modelName, string memberName, string nickname, string node)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("/\n/").ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> RunningEnquire(string modelName, string memberName, string nickname, string keyword)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} ls {fullPath}/log {(!string.IsNullOrWhiteSpace(keyword) ? $"| grep -i {keyword}" : string.Empty)}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n").ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> RunningMember(string modelName, string memberName, string nickname, string node)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            //File size
            var command = $"rsh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $5}'";
            var fileSize = await _uiqService.RunCommandAsync(command);
            /*html += $"File size: {fileSize}<br>"*/
            ;

            //File time
            command = $"rsh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $6\" \"$7\" \"$8}'";
            
            var fileTime = await _uiqService.RunCommandAsync(command);
            //html += $"File time: {fileTime}<br>";

            //Get last 30 lines of the file
            command = $"rsh -l {_rshAccount} {_loginIp} tail -n 30 {fullPath}/log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);
            var files = listData.Split("/\n/");

            var data = new
            {
                FileSize = fileSize,
                FileTime = fileTime,
                Files = files,
            };
            var response = new ApiResponse<dynamic>(data);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> RunningSms(string modelName, string memberName)
        {
            var command = $"{_uiPath}wwwroot/shell/sms_enquire.ksh {modelName} {memberName} | grep -v 'Goodbye \\| Welcome \\|logged \\|logout'";
            var result = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DailyResult(string node)
        {
            var command = $"rsh -l {_rshAccount} {_loginIp} cat /{_systemName}/{_hpcCtl}/daily_log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n").ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ResetModelShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var account = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname)?.Account;
            var command = $"rsh -l {_rshAccount} {_loginIp} /usr/bin/pjstat -s ";
            var data = await _uiqService.RunCommandAsync(command);
            var showDatas = Regex.Split(data, "/\\[Job\\s+Statistical\\s+Information\\]/");

            var jobDatas = new List<KeyValuePair<string, string>>();
            if (showDatas.Any())
            {
                foreach (var item in showDatas)
                {
                    var dataLines = item.Split("\n");
                    if (dataLines.Length > 1)
                    {
                        var jobId = dataLines[1];
                        var jobName = dataLines.Length > 6 ? dataLines[6] : string.Empty;
                        jobDatas.Add(new KeyValuePair<string, string>(jobId, $"{jobName}({jobId})"));
                    }
                }
            }

            var isWeps = modelName == "WEPS" && memberName == "CEN01";
            if (jobDatas.Any() && isWeps)
            {
                jobDatas = new List<KeyValuePair<string, string>>
                {
                    { new KeyValuePair<string, string>("ALLWEPS", "ALL jobs above") }
                };
            }

            var response = new ApiResponse<dynamic>(new
            {
                Account = account,
                ShowDatas = showDatas,
                JobDatas = jobDatas,
            });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ResetModelResult(string modelName, string memberName, string nickname, string jobId)
        {
            var result = string.Empty;
            var command = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var account = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname)?.Account;
            var member = await _uiqService.GetMemberItemAsync(modelName, memberName, nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";
            var resetModel = member?.Reset_Model;
            var message = "Cancel running job function is not available for this member.";
            if (string.IsNullOrWhiteSpace(resetModel))
            {
                await _logFileService.WriteUiActionLogFileAsync(message);
                command = $"echo {message}";
            }
            else
            {
                command = $"rsh -l {account} {_loginIp} {fullPath}{resetModel} {fullPath}";
                if (account != $"{_prefix}weps")
                {
                    command = $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/cancel_job.ksh {account} {fullPath}{resetModel} {jobId} {fullPath}";
                    result += await _uiqService.RunCommandAsync(command);
                }
            }

            await _logFileService.WriteUiActionLogFileAsync("kill start");
            await _logFileService.WriteUiActionLogFileAsync(command);

            message = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + $"Kill Model of {modelName} {memberName} {nickname}\n";
            var commandResult = await _uiqService.RunCommandAsync(command);
            result = (message + commandResult);

            await _logFileService.WriteUiActionLogFileAsync(commandResult);
            await _logFileService.WriteUiActionLogFileAsync("kill end");

            //result = "<br>" + Regex.Replace(result, "/\n/", "\n<br>");
            await _logFileService.WriteUiActionLogFileAsync(message);

            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DtgShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var dtgValue = configData?.Member_Dtg_Value;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<dynamic>(new { Dtg = dtg, DtgValue = dtgValue });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DtgResult(string modelName, string memberName, string nickname, string dtg)
        {
            var datas = new List<string>();
            dtg = dtg ?? string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var dtgAdjust = configData?.Dtg_Adjust;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            var message = "DTG adjust function is not available for this member";
            if (string.IsNullOrWhiteSpace(dtgAdjust))
            {
                datas.Add("DTG adjust function is not available for this member");
            }
            else
            {
                var command = $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/set_dtg.ksh {account} {fullPath}{dtgAdjust} {dtg} {fullPath}";
                datas.Add(command);
                var result = await _uiqService.RunCommandAsync($"{command} 2>&1");
                foreach (var item in (result ?? string.Empty).Split("\n"))
                {
                    datas.Add(item);
                }

                //輸出結果
                datas.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {modelName} {memberName} {nickname} adjust DTG value of {dtg} as follows.");
                message = Regex.Replace(message, "/\n/", "<br>");
            }

            await _logFileService.WriteUiActionLogFileAsync(message);
            var response = new ApiResponse<IEnumerable<string>>(datas);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> LidShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"cat {fullPath}/etc/Lid";
            var lid = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<string>(data: lid);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> LidResult(string modelName, string memberName, string nickname, string lid)
        {
            lid = lid ?? string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            var command = $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/set_Lid.ksh {account} {fullPath} {lid}";
            var result = await _uiqService.RunCommandAsync(command);

            var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}{modelName} {memberName} {nickname} adjust LID value to {lid}.\r\n";
            await _logFileService.WriteUiActionLogFileAsync(message);

            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> SubmitShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/Lid";
            var lid = await _uiqService.RunCommandAsync(command);

            var batchs = _uiqService.GetMemberRelay(modelName, memberName, nickname).ToList();
            var batchSelectDatas = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("", "---") };
            for (var i = 0; i < batchs?.Count; i++)
            {
                var item = batchs[i];
                var dataList_R = string.Empty;
                var tmpCommand = $"rsh -l {_rshAccount} {_loginIp} ls /{_systemName}/{account}/{modelName}/{memberName}/etc/{item}*";
                var output = await _uiqService.RunCommandAsync(tmpCommand);
                if (!string.IsNullOrEmpty(output))
                {
                    foreach (var tmpFile in output.Split("\n"))
                    {
                        if (tmpFile != null && tmpFile.Trim() != "")
                        {
                            var aList = tmpFile.Split("/");
                            var tmp = aList[aList.Count() - 1];
                            var isStatus = false;
                            if (tmp.Substring(tmp.Length - 2) != "_R")
                            {
                                var optionText = tmp;
                                var tmp1 = (tmp + "_R");
                                isStatus = output.Split("\n").Any(x => x == tmp1);
                                if (isStatus)
                                {
                                    dataList_R += dataList_R == string.Empty ? tmp1 : $"{dataList_R}|{tmp1}";
                                }

                                if (tmp.Substring(tmp.Length - 2) == "_M")
                                {
                                    optionText = $"{tmp}ajor";
                                }
                                else if (tmp.Substring(tmp.Length - 2) == "_P")
                                {
                                    optionText = $"{tmp}ost";
                                }
                                else if (tmp.Substring(tmp.Length - 3) == "_P2")
                                {
                                    optionText = isStatus ? $"{tmp.Substring(0, tmp.Length - 1)}ost2" : $"{tmp}ost";
                                }
                                else if (tmp.Substring(tmp.Length - 3) == "_MC")
                                {
                                    optionText = $"{tmp.Substring(0, tmp.Length - 1)}ajorC";
                                }

                                batchSelectDatas.Add(isStatus
                                    ? new KeyValuePair<string, string>(tmp1, optionText)
                                    : new KeyValuePair<string, string>(tmp, optionText)
                                );
                            }

                            if (tmp.Substring(tmp.Length - 2) == "_R")
                            {
                                isStatus = dataList_R.Split("|").Any(x => x == tmp);
                                if (isStatus == false)
                                    batchSelectDatas.Add(new KeyValuePair<string, string>(tmp, tmp));
                            }
                        }
                    }
                }
               
            }
            var response = new ApiResponse<dynamic>(new { Dtg = dtg, Lid = lid, BatchDatas = batchSelectDatas, });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> SubmitResult(string modelName, string memberName, string nickname, string dtg, string batch)
        {
            var datas = new List<string>();
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var submitModel = configData?.Submit_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            var message = "submit model function is not avaliable for this member.\r\n";
            if (string.IsNullOrWhiteSpace(submitModel))
            {
                datas.Add(message);
            }
            else
            {
                var command = $"rsh -l {account} -n {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/re_run.ksh {account} {fullPath}{submitModel} {modelName} {memberName} {batch} {fullPath}";
                message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Rerun the {modelName} {memberName} {nickname} with {dtg} in {batch} run \r\n";
                datas.Add(message);
                datas.Add(await _uiqService.RunCommandAsync(command));
            }

            await _logFileService.WriteUiActionLogFileAsync(message);
            var response = new ApiResponse<IEnumerable<string>>(datas);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ArchiveShow(string modelName, string memberName, string nickname)
        {
            var dataTypes = _uiqService.GetArchiveDataTypes(modelName, memberName, nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            var methodDatas = new List<KeyValuePair<string, string>>();
            foreach (var item in dataTypes)
            {
                methodDatas.Add(new KeyValuePair<string, string>(item, item));
            }

            var response = new ApiResponse<dynamic>(new { Dtg = dtg, MethodDatas = methodDatas });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ArchiveShowForEnquire(string modelName, string memberName, string nickname, string dtg, string method)
        {
            var displayDatas = new List<string>();
            var tableDatas = new List<string>();
            var configs = _uiqService.GetArchiveViewModels();
            var account = configs.FirstOrDefault(x => x.Model_Name == modelName
                                                && x.Member_Name == memberName
                                                && x.Nickname == nickname)?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var yymmdd = (await _uiqService.RunCommandAsync($"echo {dtg} | cut -c-6")).Trim();
            var yy = (await _uiqService.RunCommandAsync($"echo {dtg} | cut -c-2")).Trim();
            var mm = (await _uiqService.RunCommandAsync($"echo {dtg} | cut -c3-4")).Trim();
            var source = await _uiqService.GetArchiveResultPathAsync(modelName, memberName, nickname, method) ?? string.Empty;
            var dataId = await _uiqService.GetDataIdAsync(method);
            fullPath = source.Replace("{dtg}", dtg);
            fullPath = fullPath.Replace("{yymmdd}", yymmdd);
            fullPath = fullPath.Replace("{yy}", yy);
            fullPath = fullPath.Replace("{mm}", mm);
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                displayDatas.AddRange(new string[]
                {
                    "No target directory!! Please edit MySQL data.",
                    "---------------------------------------------",
                    "Table name : archive",
                    "Column name: target_directory",
                    $"member_id  : {account}",
                    $"data_id    : {dataId}",
                });
            }
            else
            {
                var pathArray = fullPath.Split(' ');
                foreach (var pathTok in pathArray)
                {
                    var command = $"rsh -l archive hsmsvr1a ls -al {pathTok} | sed 's%\\/.\\+\\/%%' | grep -v ^total";
                    var commandResult = await _uiqService.RunCommandAsync(command);
                    displayDatas.Add(commandResult);
                    if (string.IsNullOrWhiteSpace(commandResult) == false)
                    {
                        tableDatas = commandResult.Split(" \n\t").ToList();
                    }
                }
            }

            var response = new ApiResponse<dynamic>(new { DisplayDatas = displayDatas, TableDatas = tableDatas });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ArchiveResult(string modelName, string memberName, string nickname, int method, string dtg, string node)
        {
            var configs = _uiqService.GetArchiveViewModels();
            var data = configs.FirstOrDefault(x => x.Model_Name == modelName
                                && x.Member_Name == memberName
                                && x.Nickname == nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";
            var shell = await _uiqService.GetArchiveExecuteShellAsync(modelName, memberName, nickname, method);

            var command = $"rsh -l {_rshAccount} {_dataMvIp} /ncs/{_hpcCtl}/web/shell/run_Archive.ksh {_rshAccount} {fullPath}{shell} {dtg} {node} {modelName} {memberName} {fullPath}";
            var result = await _uiqService.RunCommandAsync($"{command} 2>&1");

            var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}  Archive redo with {dtg} \r\n";
            await _logFileService.WriteUiActionLogFileAsync(message);

            var response = new ApiResponse<IEnumerable<string>>(result.Split("\n"));
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> FixShow(string modelName, string memberName, string nickname)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            return Json(new ApiResponse<string>(data: dtg));
        }

        [HttpPost]
        public async Task<JsonResult> FixShowForEnquire(string modelName, string memberName, string nickname, string dtg)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var memberId = (await _uiqService.GetMemberItemAsync(modelName, memberName, nickname))?.Member_Id;
            var fullPath = (await _uiqService.GetModelMemberPathAsync(modelName, memberName, nickname)).FirstOrDefault() ?? string.Empty;
            fullPath = fullPath.Replace("{dtg}", dtg);
            var tableDatas = new List<string>();
            if (string.IsNullOrWhiteSpace(fullPath) == false)
            {
                foreach (var path in fullPath.Split(' '))
                {
                    var command = $"rsh -l {_rshAccount} {_loginIp} ls -ald {path}/*{dtg}* " + " | sed 's%\\/.\\+\\/%%'";
                    var data = await _uiqService.RunCommandAsync(command);
                    tableDatas = data.Split("\n").ToList();
                }
            }

            var response = new ApiResponse<dynamic>(new { MemberId = memberId, TableDatas = tableDatas });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> FixResult(string modelName, string memberName, string nickname, string dtg, string parameter, string method)
        {
            var datas = new List<string>();
            var message = string.Empty;
            var command = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fixFailedModel = configData?.Fix_Failed_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            if (fixFailedModel == null)
            {
                message = "fix failed model function is not avaliable for this member.\r\n";
                datas.Add(message);
            }
            else
            {
                command = $"rsh -l {_hpcCtl} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/run_Fixfailed.ksh {account} {fullPath}{fixFailedModel} {dtg} {method} {modelName} {memberName} {fullPath}";
                if (string.IsNullOrWhiteSpace(parameter) == false)
                    command += $@" '""\""{parameter}\""""'";

                datas.Add(await _uiqService.RunCommandAsync(command));
                message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Fixed failed model on {modelName} {memberName} in {method} with {dtg}\r\n";
            }

            await _logFileService.WriteUiActionLogFileAsync(message);
            return Json(new ApiResponse<IEnumerable<string>>(datas));
        }

        [HttpPost]
        public async Task<JsonResult> GetTyphoonSetDatas(TyphoonSetDataViewModel[] typhoonSetDatas)
        {
            return Json(new ApiResponse<TyphoonSetDataViewModel[]>(typhoonSetDatas));
        }
    }
}