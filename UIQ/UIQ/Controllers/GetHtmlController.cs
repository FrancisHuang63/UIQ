using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Controllers
{
    public class GetHtmlController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IReadLogFileService _readLogFileService;
        private readonly string _hpcCtl;
        private readonly string _systemName;
        private readonly string _systemDirectoryName;
        private readonly string _rshAccount;
        private readonly string _loginIp;
        private readonly string _prefix;
        private readonly string _logDirectoryPath;

        public GetHtmlController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService, IReadLogFileService readLogFileService)
        {
            _uiqService = uiqService;
            _readLogFileService = readLogFileService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _systemName = configuration.GetValue<string>("SystemName");
            _systemName = configuration.GetValue<string>("SystemDirectoryName");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
            _prefix = runningJobInfo?.Items?.FirstOrDefault()?.Datas?.FirstOrDefault()?.Prefix;
            _logDirectoryPath = $"{_readLogFileService.RootPath}/log";
        }

        [HttpPost]
        public async Task<string> ModelLogEnquire(string modelName, string memberName, string nickname)
        {
            if (modelName == null || memberName == null || nickname == null) return string.Empty;

            var html = @"Date:<select id=""node"" class=""form""><option>-----</option>";
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} ls {fullPath}/log | grep job | grep -v job1 | grep -v OP_";

            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("\n\t"))
            {
                html += $"<option>{item}</option>";
            }
            html += $@"</select>
                      <input type=""submit"" value=enquire"" class=""form"" OnClick=""sendAJAXRequest('post', '{Url.Action(nameof(GetHtmlController.ModelLogResult))}', 'result')"";>";

            return html;
        }

        [HttpPost]
        public async Task<string> ModelLogResult(string modelName, string memberName, string nickname, string node)
        {
            if (modelName == null || memberName == null || nickname == null || node == null) return string.Empty;

            var html = @"<pre>";
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/log/{node}";

            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("/\n/"))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(item, @"^[A-Z]+$")
                        || item.Contains("Finish"))
                {
                    html += $@"<span class=""c4"">{item}</span>";
                }
                else if (item.Contains("fail") || item.Contains("cancel"))
                {
                    html += $@"<span class=""c3"">{item}</span>";
                }
                else html += item;
            }
            html += $@"</pre>";

            return html;
        }

        [HttpPost]
        public async Task<string> RunningEnquire(string modelName, string memberName, string nickname, string keyword)
        {
            if (modelName == null || memberName == null || nickname == null) return string.Empty;

            var html = @"LogFile:<select id=""node"" class=""form"" ><option>-----</option>";
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} ls {fullPath}/log {(!string.IsNullOrWhiteSpace(keyword) ? $"| grep -i {keyword}" : string.Empty)}";

            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("\n\t"))
            {
                html += $"<option>{item}</option>";
            }
            html += $@"</select>
                      <input type=""submit"" value=""enquire"" class=""form"" OnClick=""sendAJAXRequest('post', {Url.Action(nameof(GetHtmlController.RunningMember))}', 'show')"">";

            return html;
        }

        [HttpPost]
        public async Task<string> RunningMember(string modelName, string memberName, string nickname, string node)
        {
            if (modelName == null || memberName == null || nickname == null) return string.Empty;

            var html = @"<pre>";
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $5}'";

            //File size
            var fileSize = await _uiqService.RunCommandAsync(command);
            html += $"File size: {fileSize}<br>";

            //File time
            command = $"rsh -l {_rshAccount} {_loginIp} ls -l {fullPath}/log/{node}" + " | awk '{print $6\" \"$7\" \"$8}'";
            var fileTime = await _uiqService.RunCommandAsync(command);
            html += $"File time: {fileTime}<br>";

            //Get last 30 lines of the file
            command = $"rsh -l {_rshAccount} {_loginIp} tail -n 30 {fullPath}/log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("/\n/"))
            {
                html += $"{item}<br>";
            }
            html += "</pre>";

            return html;
        }

        [HttpPost]
        public async Task<string> RunningSms(string modelName, string memberName)
        {
            if (modelName == null || memberName == null) return string.Empty;

            var html = @"<pre>";
            var command = $"{_readLogFileService.RootPath}/shell/sms_enquire.ksh {modelName} {memberName} | grep -v 'Goodbye \\| Welcome \\|logged \\|logout'";

            var result = await _uiqService.RunCommandAsync(command);
            html += $"{result}</pre>";

            return html;
        }

        [HttpPost]
        public async Task<string> DailyResult(string node)
        {
            var html = "<pre>";
            var command = $"rsh -l {_rshAccount} {_loginIp} cat /{_systemName}/{_hpcCtl}/daily_log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("/\n/"))
            {
                html += $@"<span class=""{(item.Contains("fail") ? "c3" : string.Empty)}"">{item}</span><br>";
            }

            html += "</pre>";
            return html;
        }

        [HttpPost]
        public async Task<string> ResetModelShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var account = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname)?.Account;
            var html = @"<div id=""show"" class=""enquire"">
                            <pre>======================current Job status======================
                        ";

            var command = $"rsh -l {_rshAccount} {_loginIp} /usr/bin/pjstat -A -s | sed '1,4d'";
            var data = await _uiqService.RunCommandAsync(command);
            data = data.Replace("#fx100#", string.Empty).Replace("#fx10#", string.Empty);
            var dataArray = data.Split("Statistical Information");
            html += dataArray.Count() > 2 ? dataArray[1] : string.Empty;

            var isWeps = modelName == "WEPS" && memberName == "CEN01";
            html += $"<h3>{modelName}_{memberName}(User: {account}, Nickname: {nickname})</h3>";

            var jobIds = new List<string>();
            var jobNames = new List<string>();
            if (dataArray.Any() == false)
            {
                html += "There is no job!<br>";
            }
            else
            {
                foreach (var item in dataArray)
                {
                    html += $"{item}<br>";
                    html += $"-----------------------------------------------------------------<br>";

                    var dataLines = item.Split("\n");
                    if (dataLines.Length > 1) jobIds.Add(dataLines[1]);
                    if (dataLines.Length > 6) jobNames.Add(dataLines[6]);
                }
            }
            html += "</div>";

            if (jobIds.Any() && isWeps)
            {
                html += "<select id=\"jobid\" class=\"form\">";
                html += "    <option>-----</option>";
                html += "    <option value=\"ALLWEPS\">ALL jobs above</option>";
                html += "</select>";
            }
            else
            {
                html += "<select id=\"jobid\" class=\"form\">";
                html += "   <option>-----</option>";
                for (var i = 0; i < jobIds.Count; i++)
                {
                    html += "    <option value=\"$jobidarr[$i]\">$jobnmarr[$i]($jobidarr[$i])</option>";
                }
                html += "</select>";
            }

            html += $@"<input id=""node"" type=""hidden"" value="""">
                        <input id=""account"" type=""hidden"" value=""{account}"">
                        <input id=""adjust"" type=""hidden"" value="""">
                        <input type=""button"" class=""form"" value=""kill"" OnClick=""if(confirm('Do you want to submit?'))  {{sendAJAXRequest('post', '{Url.Action(nameof(GetHtmlController.ResetModelResult))}', 'result');}} "">
                        <div id=""result"" class=""short"">result...</div>";

            return html;
        }

        [HttpPost]
        public async Task<string> ResetModelResult(string modelName, string memberName, string nickname, string jobId)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var account = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname)?.Account;
            var member = await _uiqService.GetMemberItemAsync(modelName, memberName, nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var shellName = member?.Reset_Model;
            if (string.IsNullOrWhiteSpace(shellName))
                return "Cancel running job function is not available for this member.";

            var command = account == $"{_prefix}weps"
                ? $"rsh -l {account} {_loginIp} {fullPath}{shellName} {fullPath}"
                : $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/{_systemDirectoryName}/wwwroot/shell/cancel_job.ksh {account} {fullPath}{shellName} {jobId} {fullPath}";

            var result = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + $"Kill Model of {modelName} {memberName} {nickname}\n";
            result += await _uiqService.RunCommandAsync(command);

            return result;
        }

        [HttpPost]
        public async Task<string> DtgShow(string modelName, string memberName, string nickname)
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
            var html = $@"<div class=""short"">
                            [Model]={modelName}, [Member]={memberName}, [Nickname]={nickname}<br><br>DTG={dtg}
                           </div>
                         <br>
                         <form class=""form"">Adjust value
                            <select id=""dtg"" class=""form"">
                                <option value=""{dtgValue}"">+</option>
                                <option value=""{dtgValue}"">-</option>
                            </select>
                            {dtgValue}
                            <input type=""button"" class=""form"" value=""Submit"" OnClick=""sendAJAXRequest('post', '{Url.Action(nameof(GetHtmlController.DtgResult))}', 'result'); "">
                        </form>";

            return html;
        }

        [HttpPost]
        public async Task<string> DtgResult(string modelName, string memberName, string nickname, string dtg)
        {
            var html = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var dtgAdjust = configData?.Dtg_Adjust;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            var message = "DTG adjust function is not available for this member.\r\n";
            if (string.IsNullOrWhiteSpace(dtgAdjust))
            {
                html += "DTG adjust function is not available for this member.\r\n";
            }
            else
            {
                var command = $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/{_systemDirectoryName}/wwwroot/shell/set_dtg.ksh {account} {fullPath}{dtgAdjust} {dtg} {fullPath}";
                html += $"{command} <br><br>";
                html += await _uiqService.RunCommandAsync(command);

                //輸出結果
                message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {modelName} {memberName} {nickname} adjust DTG value of {dtg} as follows.";
                message = message.Replace("/\n/", "<br>");
                html += message;
            }

            await _readLogFileService.WriteDataIntoLogFileAsync(_logDirectoryPath, $"{_logDirectoryPath}/UI_actions.log", message);
            return html;
        }

        [HttpPost]
        public async Task<string> LidShow(string modelName, string memberName, string nickname)
        {
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"cat {fullPath}/etc/Lid";
            var lid = await _uiqService.RunCommandAsync(command);
            var html = $@"[Model]={modelName}, [Member]={memberName}, [Nickname]={nickname}<br><br>LID={lid}";

            return html;
        }

        [HttpPost]
        public async Task<string> LidResult(string modelName, string memberName, string nickname, string lid)
        {
            var html = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            var command = $"rsh -l {account} {_loginIp} /{_systemName}/{_hpcCtl}/web/{_systemDirectoryName}/wwwroot/shell/set_Lid.ksh {account} {fullPath} {lid}";
            html += command;
            html += await _uiqService.RunCommandAsync(command);

            var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}{modelName} {memberName} {nickname} adjust LID value to {lid}.\r\n";
            await _readLogFileService.WriteDataIntoLogFileAsync(_logDirectoryPath, $"{_logDirectoryPath}/UI_actions.log", message);
            return html;
        }

        [HttpPost]
        public async Task<string> SubmitShow(string modelName, string memberName, string nickname)
        {
            var html = string.Empty;
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

            var batchs = _uiqService.GetMemberRelay(modelName, memberName, nickname);
            var batchSelectHtml = @"<select id=""batch"" class=""form""><option value="""">---</option>";
            foreach (var item in batchs)
            {
                // TODO: add batchSelectHtml <option>
            }
            batchSelectHtml += "</select>";

            html = $@"<div class=""short"">
                        [Model] ={modelName}, [Member]={memberName}, [Nickname]={nickname}<br><br>
                        DTG ={dtg} ;
                        Lid ={lid} ;
                        batch:{batchSelectHtml} ;
                    </div>
                    <input type=""hidden"" id=""dtg"" value=""{dtg}"">
                    <input type=""button"" {(lid == "1" ? "disabled" : string.Empty)} class=""form"" value=""Submit"" OnClick=""sendAJAXRequest('post', '{Url.Action(nameof(SubmitResult))}', 'result');"">";
            return html;
        }

        [HttpPost]
        public async Task<string> SubmitResult(string modelName, string memberName, string nickname, string dtg, string batch)
        {
            var html = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var submitModel = configData?.Submit_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            var message = "submit model function is not avaliable for this member.\r\n";
            if (string.IsNullOrWhiteSpace(submitModel))
            {
                html += message;
            }
            else
            {
                var command = $"rsh -l {account} -n {_loginIp} /{_systemName}/{_hpcCtl}/web/{_systemDirectoryName}/wwwroot/shell/re_run.ksh {account}{fullPath}{submitModel} {modelName} {memberName} {batch} {fullPath}";
                message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Rerun the {modelName} {memberName} {nickname} with {dtg} in {batch} run \r\n";
                html += message;
                html += await _uiqService.RunCommandAsync(command);
            }

            await _readLogFileService.WriteDataIntoLogFileAsync(_logDirectoryPath, $"{_logDirectoryPath}/UI_actions.log", message);
            return html;
        }

        [HttpPost]
        public async Task<string> ArchiveShow(string modelName, string memberName, string nickname)
        {
            var dataTypes = _uiqService.GetArchiveDataTypes(modelName, memberName, nickname);
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            var html = $@"Current DTG<input id=""dtg"" type=""text"" class=""form"" cols=""70"" value=""{dtg}""> (the DTG format is yymmddhh)
                          <br><br>Data type
                          <select id=""method"" class=""form"">";

            foreach (var item in dataTypes)
            {
                html += $"<option>{item}</option>";
            }
            html += "</select>";

            return html;
        }

        [HttpPost]
        public async Task<string> ArchiveShowForEnquire()
        {
            return "Enquire completed";
        }

        [HttpPost]
        public async Task<string> ArchiveResult()
        {
            return "Sumbit completed";
        }

        [HttpPost]
        public async Task<string> FixShow(string modelName, string memberName, string nickname)
        {
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);
            var command = $"rsh -l {_rshAccount} {_loginIp} cat {fullPath}/etc/crdate" + " | awk '{print $1}'";
            var dtg = await _uiqService.RunCommandAsync(command);
            var html = $@"Current DTG<input id=""dtg"" type=""text"" class=""form"" cols=""70"" value=""{dtg}""> (the DTG format is yymmddhh)";
            return html;
        }

        [HttpPost]
        public async Task<string> FixShowForEnquire(string modelName, string memberName, string nickname, string dtg)
        {
            var html = "<pre>";
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var memberId = configData?.Model_Id;
            var fullPath = (await _uiqService.GetModelMemberPathAsync(modelName, memberName, nickname)).FirstOrDefault() ?? string.Empty;
            fullPath = fullPath.Replace("{dtg}", dtg);
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                html += $@"No target directory!! Please edit MySQL data.<br>
                          ---------------------------------------------<br>
                        Table name : member<br>
                        Column name: fix_failed_target_directory<br>
                        member_id  : {memberId}<br>";
            }
            else
            {
                foreach (var path in fullPath.Split(' '))
                {
                    var command = $"rsh -l {_rshAccount} {_loginIp} ls -ald {path}/*{dtg}* " + " | sed 's%\\/.\\+\\/%%'";
                    html += command;
                    var data = await _uiqService.RunCommandAsync(command);
                    html += "<table><tr>";
                    var idx = 0;
                    foreach (var item in data.Split("\n\t"))
                    {
                        html += $"<td{item}</td>{((idx % 9 == 8) ? "</td></tr><tr>" : string.Empty)}";
                        idx++;
                    }
                    html += "</table><br><br>";
                }
            }

            html += "</pre>";
            return html;
        }

        [HttpPost]
        public async Task<string> FixResult(string modelName, string memberName, string nickname, string dtg, string parameter, string method)
        {
            var html = string.Empty;
            var message = string.Empty;
            var command = string.Empty;
            var configList = _uiqService.GetModelLogFileViewModels();
            var configData = configList.FirstOrDefault(x => x.Model_Name == modelName
                                                      && x.Member_Name == memberName
                                                      && x.Nickname == nickname);
            var account = configData?.Account;
            var fixFailedModel = configData?.Fix_Failed_Model;
            var fullPath = await _uiqService.GetFullPathAsync(modelName, memberName, nickname);

            if (fixFailedModel == null)
            {
                message = "fix failed model function is not avaliable for this member.\r\n";
                html += "fix failed model function is not avaliable for this member.\r\n";
            }
            else
            {
                command = $"rsh -l {_hpcCtl} {_loginIp} /{_systemName}/{_hpcCtl}/web/{_systemDirectoryName}/wwwroot/shell/run_Fixfailed.ksh {account} {fullPath}{fixFailedModel} {dtg} {method} {modelName} {memberName} {fullPath}";
                if (string.IsNullOrWhiteSpace(parameter) == false)
                    command += $@" '""\""{parameter}\""""'";

                html += $"<br>{command}";
                message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} Fixed failed model on {modelName} {memberName} in {method} with {dtg}\r\n";

                var result = await _uiqService.RunCommandAsync(command);
                result = result.Replace("\n", "\n<br>");
                html += $"<br>{result}";
            }

            await _readLogFileService.WriteDataIntoLogFileAsync(_logDirectoryPath, $"{_logDirectoryPath}/UI_actions.log", message);
            return html;
        }

        [HttpPost]
        public async Task<string> TyphoonShow(string dtg, int adjust)
        {
            var typhoonEntry = new string[] { "Name", "Lat.", "Long.", "6-hr ago Lat.", "6-hr ago Long.", "Center Pressure", "15M/S Radius", "Maximum Speed", "25M/S Radius" };
            var requiredEntry = new string[] { "Name", "Lat.", "Long.", };
            var typhoonEntryNames = new string[]
            {
                nameof(TyphoonSetDataViewModel.Name),
                nameof(TyphoonSetDataViewModel.Lat),
                nameof(TyphoonSetDataViewModel.Lng),
                nameof(TyphoonSetDataViewModel.LatBefore6Hours),
                nameof(TyphoonSetDataViewModel.LngBefore6Hours),
                nameof(TyphoonSetDataViewModel.CenterPressure),
                nameof(TyphoonSetDataViewModel.Radius15MPerS),
                nameof(TyphoonSetDataViewModel.MaximumSpeed),
                nameof(TyphoonSetDataViewModel.Radius25MPerS),
            };
            var formatDescription = new string[] { "(颱風/TD 名稱：PHANFONE/TD201905)",
                                                   "(緯度：13.9) (填寫範圍: 0.0~90.0)",
                                                   "(經度：117.7) (填寫範圍: 0.0~180.0)",
                                                   "(6hr 前緯度：13.4) (填寫範圍: 0.0~90.0)",
                                                   "(6hr 前經度：118.0) (填寫範圍: 0.0~180.0)",
                                                   "(中心氣壓：965)",
                                                   "(七級風暴風半徑：180)",
                                                   "(最大風速：35)",
                                                   "(十級風暴風半徑：50)",
                                                 };
            var html = $@"<form id=""typhoonSetDataFrom"">
                            <table>
                            <tr>
                                <td>DTG<font color=""red""><font>*</font>:</td>
                                <td><input id=""dtg"" type=""text"" value=""dtg"" name=""dtg""></td>
                                <td class=""input_description"">(DTG:19122612)<br></td>
                            </tr>
                            <tr style=""display:none;"">
                                <td>Typhoon number:</td>
                                <td><input id=""num"" type=""text"" value=""{adjust}"" name=""num"" disabled></td>
                            </tr>
                            <tr>
                                <td><hr></td>
                                <td><hr></td>
                            </tr>";
            for (int i = 0; i < adjust; i++)
            {
                for (int colIdx = 0; colIdx < typhoonEntry.Length; colIdx++)
                {
                    html += @$"<tr>
								<td>{typhoonEntry[colIdx]} {(requiredEntry.Contains(typhoonEntry[colIdx]) ? @"<font color=""red"">*</font>:</td>" : string.Empty)}
								<td><input type=""text"" value="""" name=""TyphoonSetDatas[{i}].{typhoonEntryNames[colIdx]}""></td>
								<td class=""input_description"">{formatDescription[colIdx]}</td>
							</tr>";
                }

                html += "<tr><td><hr></td><td><hr></td></tr>";
            }

            html += $@"<tr>
                         <td></td>
                         <td>
                             <input type=""button"" class=""form"" value=""View"" OnClick=""sendTyphoonDataRequest('post', '{Url.Action(nameof(TyphoonPreview))}', 'file_content', '9')"">
                         </td>
                       </tr>
                  </table>
               </form>";

            return html;
        }

        [HttpPost]
        public async Task<string> TyphoonPreview(string dtg, int num, TyphoonSetDataViewModel[] typhoonSetDatas)
        {
            if (typhoonSetDatas == null || typhoonSetDatas.Any(x =>
                                             string.IsNullOrWhiteSpace(x.Name)
                                             || x.Lat.HasValue == false
                                             || x.Lng.HasValue == false))
            {
                return "*欄位必填";
            }

            var dirNameParameters = new[]
            {
                new{ DirName = "ty", FilePrefix = "dat" },
                new{ DirName = "ty", FilePrefix = "txt" },
                new{ DirName = "tdty", FilePrefix = "dat" },
                new{ DirName = "tdty", FilePrefix = "txt" },
            };
            var html = @$"<font size=""1"">
							<table class=""xdebug-error xe-notice"" dir=""ltr"" border=""1"" cellspacing=""0"" cellpadding=""1""></table>
						</font>";

            foreach (var data in typhoonSetDatas)
            {
                foreach (var dirNameParam in dirNameParameters)
                {
                    var isDatType = dirNameParam.FilePrefix == "dat";
                    var content = $@"{(isDatType ? string.Empty : $"20{dtg}<br>")}
									 {typhoonSetDatas.Count()}<br>{data.Name.PadRight((isDatType ? 9 : 16), ' ')} {data.Lat.Value} N   {data.Lng} E  {data.LatBefore6Hours} N   {data.LngBefore6Hours} E    {data.CenterPressure} MB   {data.Radius15MPerS} KM  {data.MaximumSpeed} M/S   {data.Radius25MPerS} KM";
                    html += $@"<h4 class=""typhoon_file_name"">{dirNameParam.DirName}/typhoon{dtg}.dat</h4>
							   <div id='{dirNameParam.DirName}_{dirNameParam.FilePrefix}_content'>
									<pre>{content}</pre>
							   </div>";
                }
            }

            html += @$"<br><input type=""button"" class=""form"" value=""Submit"" OnClick=""sendTyphoonDataRequest('post', '{Url.Action(nameof(MaintainToolsController.SaveTyphoonData), "MaintainTools")}', 'file_created_result', '9')"">";
            return html;
        }
    }
}