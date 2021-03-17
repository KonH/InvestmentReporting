using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class CheckController : ControllerBase {
		[Authorize]
		[HttpGet]
		public IActionResult Check() => Ok();
	}
}