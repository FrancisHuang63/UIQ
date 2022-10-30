using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIQ.Attributes;
using UIQ.Services.Interfaces;

namespace UIQ.Controllers
{
	[Authorize]
	public class HpcEnquireController : Controller
	{
		private readonly IUiqService _uiqService;
		private readonly IEncodeService _encodeService;

		public HpcEnquireController(IUiqService uiqService, IEncodeService encodeService)
		{
			_uiqService = uiqService;
			_encodeService = encodeService;
		}

		[MenuPageAuthorize(Enums.MenuEnum.NwpRunningJobs)]
		public IActionResult NwpRunningJobs()
		{
			return View();
		}

		[MenuPageAuthorize(Enums.MenuEnum.CronStatus)]
		public IActionResult CronStatus()
		{
			return View();
		}

		[MenuPageAuthorize(Enums.MenuEnum.HpcStatus)]
		public IActionResult HpcStatus()
		{
			return View();
		}

		[HttpPost]
		public async Task<ContentResult> NwpRunningNodes(string node)
		{
			node = _encodeService.HtmlEncode(node);

			if (node == "0") return new ContentResult { Content = string.Empty, ContentType = "text/html" };
			var html = await _uiqService.GetExecuteNwpRunningNodesCommandHtmlAsync(node);
			return new ContentResult { Content = html, ContentType = "text/html" };
		}
	}
}