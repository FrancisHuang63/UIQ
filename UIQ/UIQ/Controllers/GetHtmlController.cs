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
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls {fullPath}/log | grep job | grep -v job1 | grep -v OP_";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n")
                .Where(x => string.IsNullOrWhiteSpace(x) == false).ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ModelLogResult(string modelName, string memberName, string nickname, string node)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat {fullPath}/log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n")
                .Where(x => string.IsNullOrWhiteSpace(x) == false).ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> RunningEnquire(string modelName, string memberName, string nickname, string keyword)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls {fullPath}/log {(!string.IsNullOrWhiteSpace(keyword) ? $"| grep -i {keyword}" : string.Empty)}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n")
                .Where(x => string.IsNullOrWhiteSpace(x) == false).ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> RunningMember(string modelName, string memberName, string nickname, string node)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            //File size
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $5}'";
            var fileSize = await _uiqService.RunCommandAsync(command);

            //File time
            command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $6}'";
            var fileTime = await _uiqService.RunCommandAsync(command);

            command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $7}'";
            fileTime += " " + await _uiqService.RunCommandAsync(command);

            command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $8}'";
            fileTime += " " + await _uiqService.RunCommandAsync(command);
            fileTime = fileTime.Replace("\n", string.Empty);

            //Get last 30 lines of the file
            command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} tail -n 30 {fullPath}/log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);
            var files = listData.Split("/\n/").Where(x => string.IsNullOrWhiteSpace(x) == false).ToList();

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
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat /{_systemName}/{_hpcCtl}/daily_log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);

            var response = new ApiResponse<IEnumerable<string>>(listData.Split("\n")
                .Where(x => string.IsNullOrWhiteSpace(x) == false).ToList());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ResetModelShow(string modelName, string memberName, string nickname)
        {
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} /usr/bin/pjstat -s ";
            var data = await _uiqService.RunCommandAsync(command);
            var showDatas = data.Split("[Job Statistical Information]");

            var jobDatas = new List<KeyValuePair<string, string>>();
            jobDatas.Add(new KeyValuePair<string, string>("ALL", "ALL"));
            if (showDatas.Any())
            {
                foreach (var item in showDatas)
                {
                    var dataLines = item.Split("\n").Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray();
                    if (dataLines.Length > 1)
                    {
                        var jobId = dataLines[0]?.Split(":")?.LastOrDefault()?.Trim();
                        var jobName = dataLines.Length > 5 ? dataLines[5]?.Split(":")?.LastOrDefault()?.Trim() : string.Empty;
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
                Acnt = secureAcnt.GetSecureStringToString(),
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var resetModel = await _uiqService.GetMemberResetModelAsync(modelName, memberName, nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";
            var message = "Cancel running job function is not available for this member.";
            if (string.IsNullOrWhiteSpace(resetModel))
            {
                await _logFileService.WriteUiActionLogFileAsync(message);
                command = $"echo {message}";
            }
            else
            {
                var decodeAcnt = secureAcnt.GetSecureStringToString();
                command = $"sudo -u {_rshAccount} ssh -l {decodeAcnt} {_loginIp} {fullPath}{resetModel} {fullPath}";
                if (decodeAcnt != $"{_prefix}weps")
                {
                    command = $"sudo -u {_rshAccount} ssh -l {decodeAcnt} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/cancel_job.ksh {decodeAcnt} {fullPath}{resetModel} {jobId} {fullPath}";
                    //result += await _uiqService.RunCommandAsync(command);
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var dtgValue = modelLogFile?.Member_Dtg_Value;
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var dtgAdjust = modelLogFile?.Dtg_Adjust;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            var message = "DTG adjust function is not available for this member";
            if (string.IsNullOrWhiteSpace(dtgAdjust))
            {
                datas.Add("DTG adjust function is not available for this member");
            }
            else
            {
                var command = $"sudo -u {_rshAccount} ssh -l {secureAcnt.GetSecureStringToString()} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/set_dtg.ksh {secureAcnt.GetSecureStringToString()} {fullPath}{dtgAdjust} {dtg} {fullPath}";
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            var command = $"sudo -u {_rshAccount} ssh -l {secureAcnt.GetSecureStringToString()} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/set_Lid.ksh {secureAcnt.GetSecureStringToString()} {fullPath} {lid}";
            var result = await _uiqService.RunCommandAsync(command);

            var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}{modelName} {memberName} {nickname} adjust LID value to {lid}.\r\n";
            await _logFileService.WriteUiActionLogFileAsync(message);

            var response = new ApiResponse<string>(data: result);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> SubmitShow(string modelName, string memberName, string nickname)
        {
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/Lid";
            var lid = await _uiqService.RunCommandAsync(command);

            var batchs = _uiqService.GetMemberRelay(modelName, memberName, nickname).ToList();
            var batchSelectDatas = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("", "---") };
            for (var i = 0; i < batchs?.Count; i++)
            {
                var item = batchs[i];
                var dataList_R = string.Empty;
                var tmpCommand = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls /{_systemName}/{secureAcnt.GetSecureStringToString()}/{modelName}/{memberName}/etc/{item}*";
                var output = await _uiqService.RunCommandAsync(tmpCommand);
                if (!string.IsNullOrEmpty(output))
                {
                    foreach (var tmpFile in output.Split("\n").Where(x => string.IsNullOrWhiteSpace(x) == false))
                    {
                        if (tmpFile != null && tmpFile.Trim() != "")
                        {
                            var aList = tmpFile.Split("/");
                            var tmp = aList[aList.Count() - 1];
                            var isStatus = false;
                            if (tmp.Substring(tmp.Length - 2) != "_R")
                            {
                                var optionText = tmp;
                                var tmp1 = aList[aList.Count() - 1];
                                isStatus = output.Split("\n").Any(x => x == (tmp + "_R"));
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var submitModel = modelLogFile?.Submit_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            var message = "submit model function is not avaliable for this member.\r\n";
            if (string.IsNullOrWhiteSpace(submitModel))
            {
                datas.Add(message);
            }
            else
            {
                var command = $"sudo -u {_rshAccount} ssh -l {secureAcnt.GetSecureStringToString()} -n {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/re_run.ksh {secureAcnt.GetSecureStringToString()} {fullPath}{submitModel} {modelName} {memberName} {batch} {fullPath}";
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
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
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
            var configs = _uiqService.GetArchiveViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                && x.Mb_Name == memberName
                                                && x.Nickname == nickname);
            var acnt = configs?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
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
                displayDatas.Add("請上班時間通知NCS調整UI設定");
                //displayDatas.AddRange(new string[]
                //{
                //    "No target directory!! Please edit MySQL data.",
                //    "---------------------------------------------",
                //    "Table name : archive",
                //    "Column name: target_directory",
                //    $"member_id  : {secureAcnt.GetSecureStringToString()}",
                //    $"data_id    : {dataId}",
                //});
            }
            else
            {
                foreach (var pathTok in fullPath.Split(' '))
                {
                    var command = $"sudo -u {_rshAccount} ssh -l archive hsmsvr1a ls -al {pathTok} | sed 's%\\/.\\+\\/%%' | grep -v ^total";
                    var commandResult = await _uiqService.RunCommandAsync(command);
                    displayDatas.Add(command);
                    if (string.IsNullOrWhiteSpace(commandResult) == false)
                    {
                        displayDatas.Add(commandResult);
                        tableDatas = commandResult.Split(" \n\t").ToList();
                    }
                    else
                    {
                        displayDatas.Add("No data!!");
                    }
                }
            }

            var response = new ApiResponse<dynamic>(new { DisplayDatas = displayDatas, TableDatas = tableDatas, IsPathExist = string.IsNullOrWhiteSpace(fullPath) });
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> ArchiveResult(string modelName, string memberName, string nickname, string method, string dtg, string node)
        {
            var configs = _uiqService.GetArchiveViewModels();
            var data = configs.FirstOrDefault(x => x.Md_Name == modelName
                                && x.Mb_Name == memberName
                                && x.Nickname == nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";
            var shell = await _uiqService.GetArchiveExecuteShellAsync(modelName, memberName, nickname, method);

            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_dataMvIp} /ncs/{_hpcCtl}/web/shell/run_Archive.ksh {_rshAccount} {fullPath}{shell} {dtg} {node} {modelName} {memberName} {fullPath}";
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
            var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            return Json(new ApiResponse<string>(data: dtg));
        }

        [HttpPost]
        public async Task<JsonResult> FixShowForEnquire(string modelName, string memberName, string nickname, string dtg)
        {
            var memberId = (await _uiqService.GetMemberItemAsync(modelName, memberName, nickname))?.Member_Id;
            var fullPath = (await _uiqService.GetModelMemberPathAsync(modelName, memberName, nickname)).FirstOrDefault() ?? string.Empty;
            fullPath = fullPath.Replace("{dtg}", dtg);
            var tableDatas = new List<string>();
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                tableDatas.Add("請上班時間通知NCS調整UI設定");
            }
            else
            {
                foreach (var path in fullPath.Split(' '))
                {
                    var command = $"sudo -u {_rshAccount} ssh -l {_rshAccount} {_loginIp} ls -ald {path}/*{dtg}* " + " | sed 's%\\/.\\+\\/%%'";
                    var data = await _uiqService.RunCommandAsync(command);
                    tableDatas.Add(command);
                    if (string.IsNullOrWhiteSpace(data) == false)
                    {
                        var items = data.Split("\n").Where(x => string.IsNullOrWhiteSpace(x) == false).ToList();
                        foreach (string item in items)
                            tableDatas.Add(item);
                    }
                    else
                    {
                        tableDatas.Add("No data!!");
                    }
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
            var modelLogFile = _uiqService.GetModelLogFileViewModels().FirstOrDefault(x => x.Md_Name == modelName
                                                      && x.Mb_Name == memberName
                                                      && x.Nickname == nickname);
            var acnt = modelLogFile?.Acnt;
            var secureAcnt = acnt.GetGetSecureString();
            var fixFailedModel = modelLogFile?.Fix_Failed_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname) + "/";

            if (fixFailedModel == null)
            {
                message = "fix failed model function is not avaliable for this member.\r\n";
                datas.Add(message);
            }
            else
            {
                command = $"sudo -u {_rshAccount} ssh -l {_hpcCtl} {_loginIp} /{_systemName}/{_hpcCtl}/web/shell/run_Fixfailed.ksh {secureAcnt.GetSecureStringToString()} {fullPath}{fixFailedModel} {dtg} {method} {modelName} {memberName} {fullPath}";
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
            if (typhoonSetDatas?.Any() ?? false)
            {
                foreach (var item in typhoonSetDatas)
                {
                    item.LatBefore6Hours = item.LatBefore6Hours ?? -1;
                    item.LngBefore6Hours = item.LngBefore6Hours ?? -1;
                    item.CenterPressure = item.CenterPressure ?? -1;
                    item.Radius15MPerS = item.Radius15MPerS ?? -1;
                    item.MaximumSpeed = item.MaximumSpeed ?? -1;
                    item.Radius25MPerS = item.Radius25MPerS ?? -1;
                }
            }
            return Json(new ApiResponse<TyphoonSetDataViewModel[]>(typhoonSetDatas));
        }
    }
}