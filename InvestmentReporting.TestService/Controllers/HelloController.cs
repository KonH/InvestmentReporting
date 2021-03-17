using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.TestService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class HelloController : ControllerBase {
		[HttpGet]
		public string Get() => "Hello World!";
	}
}