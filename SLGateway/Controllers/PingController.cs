using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLGateway.Data;

namespace SLGateway.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PingController : Controller
	{
		[Route("")]
		public IActionResult Status()
		{
			return Ok();
		}

		[Authorize(ApiKeyAuthenticationPolicy.Client)]
		[Route("clientkey")]
		public IActionResult ClientKey()
		{
			return Ok();
		}

		[Authorize(ApiKeyAuthenticationPolicy.Object)]
		[Route("objectkey")]
		public IActionResult ObjectKey()
		{
			return Ok();
		}
	}
}
