using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.TestService.Controllers {
	[ApiController]
	[Route("api/test/v1/[controller]")]
	public class HelloController : ControllerBase {
		[HttpGet]
		public string Get() => "Hello World!";
	}
}