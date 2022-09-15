using Microsoft.AspNetCore.Mvc;

namespace UIQ.Controllers
{
    public class LogviewController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogviewController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index(string modelName, string memberName, string account)
        {
            var returnContent = string.Empty;
            var baseUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host;
            var logUrl = $"{baseUrl}/log/{modelName}/{account}/{memberName}.log";
            var logPath = $"{Directory.GetCurrentDirectory()}/wwwroot/log/{modelName}/{account}/{memberName}.log";
            if (System.IO.File.Exists(logPath))
            {
                returnContent += $@"<link rel=stylesheet type=""text/css"" href=""{baseUrl}/css/fjstyle.css"">
									<pre>";
                var logContnet = System.IO.File.ReadAllText(logPath);
                var logLineDatas = logContnet.Split('\n');
                foreach (var logLineData in logLineDatas)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(logLineData, @"^[A-Z]+$")
                        || logLineData.Contains("Finish"))
                    {
                        returnContent += $@"<span class=""c4"">{logLineData}</span>";
                    }
                    else if (logLineData.Contains("fail") || logLineData.Contains("cancel"))
                    {
                        returnContent += $@"<span class=""c3"">{logLineData}</span>";
                    }
                    else returnContent += logLineData + "\n";
                }

                returnContent += "</pre>";
            }
            else
            {
                returnContent = $"<pre> There is no log file of {modelName}_{memberName}.";
            }

            return Content(returnContent);
        }
    }
}