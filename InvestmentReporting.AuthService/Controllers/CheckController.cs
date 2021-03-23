using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class CheckController : ControllerBase {
		[Authorize]
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult Check() => Ok();
	}
}