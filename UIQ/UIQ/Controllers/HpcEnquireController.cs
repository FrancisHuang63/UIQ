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
		public async Task<JsonResult> NwpRunningNodes(string node)
		{
			node = _encodeService.HtmlEncode(node);
			var response = await _uiqService.GetExecuteNwpRunningNodesCommandResponseAsync(node);
			return Json(response);
		}
	}
}