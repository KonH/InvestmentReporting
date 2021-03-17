using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.TestService.Controllers {
	[Authorize]
	[ApiController]
	[Route("api/test/v1/[controller]")]
	public class HelloController : ControllerBase {
		[HttpGet]
		public string Get() => "Hello World!";
	}
}