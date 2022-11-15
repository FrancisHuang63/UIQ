using Microsoft.AspNetCore.Mvc;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
	public class LogviewController : Controller
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogFileService _readLogFileService;
		private readonly IEncodeService _encodeService;

		public LogviewController(IHttpContextAccessor httpContextAccessor, ILogFileService readLogFileService, IEncodeService encodeService)
		{
			_httpContextAccessor = httpContextAccessor;
			_readLogFileService = readLogFileService;
			_encodeService = encodeService;
		}

		public async Task<IActionResult> Index(string modelName, string memberName, string account)
		{
			var secureAccount = account.GetGetSecureString();
			ViewBag.ModelName = _encodeService.HtmlEncode(modelName);
			ViewBag.MemberName = _encodeService.HtmlEncode(memberName);
			ViewBag.Account = _encodeService.HtmlEncode(secureAccount.GetSecureStringToString());
			
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> GetLogData(string modelName, string memberName, string acnt, bool isGetLastLine = false)
        {
			modelName = _encodeService.HtmlEncode(modelName).GetFilterPathTraversal();
			memberName = _encodeService.HtmlEncode(memberName).GetFilterPathTraversal();
			acnt = _encodeService.HtmlEncode(acnt).GetFilterPathTraversal();

			var baseUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host;
			var logUrl = $"{baseUrl}/log/{modelName}/{acnt}/{memberName}.log";
			var logPath = Path.Combine(_readLogFileService.RootPath, "log", modelName, acnt, $"{memberName}.log");
			var logContent = await _readLogFileService.ReadLogFileAsync(logPath);

			if (logContent == null) return Json(new ApiResponse<IEnumerable<string>>(data: new string[] { }));

			var logLineDatas = isGetLastLine ? new string[] { logContent.Split('\n').LastOrDefault(x => !string.IsNullOrEmpty(x)) } : logContent.Split('\n');
			return Json(new ApiResponse<IEnumerable<string>>(data: logLineDatas));
		} 
	}
}