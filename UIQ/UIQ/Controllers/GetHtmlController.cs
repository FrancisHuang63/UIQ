using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    public class GetHtmlController : Controller
    {
        private readonly IUiqService _uiqService;
        private readonly IReadLogFileService _readLogFileService;
        private readonly string _hpcCtl;
        private readonly string _rshAccount;
        private readonly string _loginIp;

        public GetHtmlController(IConfiguration configuration, IOptions<RunningJobInfoOption> runningJobInfoOption, IUiqService uiqService, IReadLogFileService readLogFileService)
        {
            _uiqService = uiqService;
            _readLogFileService = readLogFileService;
            _hpcCtl = configuration.GetValue<string>("HpcCTL");
            _rshAccount = configuration.GetValue<string>("RshAccount");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
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
            var command = $"rsh -l {_rshAccount} {_loginIp} cat /ncs/{_hpcCtl}/daily_log/{node}";
            var listData = await _uiqService.RunCommandAsync(command);
            foreach (var item in listData.Split("/\n/"))
            {
                html += $@"<span class=""{(item.Contains("fail") ? "c3" : string.Empty)}"">{item}</span><br>";
            }

            html += "</pre>";
            return html;
        }
    }
}