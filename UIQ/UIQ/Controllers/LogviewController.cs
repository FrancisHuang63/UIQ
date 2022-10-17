﻿using Microsoft.AspNetCore.Mvc;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
    public class LogviewController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReadLogFileService _readLogFileService;

        public LogviewController(IHttpContextAccessor httpContextAccessor, IReadLogFileService readLogFileService)
        {
            _httpContextAccessor = httpContextAccessor;
            _readLogFileService = readLogFileService;
        }

        public async Task<ContentResult> Index(string modelName, string memberName, string account, bool isGetLastLine = false)
        {
            var baseUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host;
            var logUrl = $"{baseUrl}/log/{modelName}/{account}/{memberName}.log";
            var logPath = Path.Combine(_readLogFileService.RootPath, "log", modelName, account, $"{memberName}.log");
            var logContent = await _readLogFileService.ReadLogFileAsync(logPath);

            if(logContent == null) return Content($"<pre> There is no log file of {modelName}_{memberName}.");

            var returnContent = $@"<link rel=stylesheet type=""text/css"" href=""{baseUrl}/css/fjstyle.css"">
									<pre>";
            
            var logLineDatas = isGetLastLine ? new string[] { logContent.Split('\n').LastOrDefault(x => !string.IsNullOrEmpty(x)) } : logContent.Split('\n');
            foreach (var logLineData in logLineDatas)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(logLineData, @"^[A-Z]+$")
                    || logLineData.Contains("Finish"))
                {
                    returnContent += $@"<span class=""c4"">{(isGetLastLine ? "Finish" : logLineData)}</span>";
                }
                else if (logLineData.Contains("fail") || logLineData.Contains("cancel"))
                {
                    returnContent += $@"<span class=""c3"">{logLineData}</span>";
                }
                else returnContent += logLineData + "\n";
            }

            returnContent += "</pre>";

            return new ContentResult
            {
                ContentType = "text/html",
                Content = returnContent
            };
        }
    }
}