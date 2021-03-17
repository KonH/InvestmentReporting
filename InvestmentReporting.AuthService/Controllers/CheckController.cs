using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("api/auth/v1/[controller]")]
	public class CheckController : ControllerBase {
		[Authorize]
		[HttpGet]
		public IActionResult Check() => Ok();
	}
}