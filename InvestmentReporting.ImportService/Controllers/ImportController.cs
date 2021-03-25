using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.ImportService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ImportController : ControllerBase {
		readonly ILogger _logger;

		public ImportController(ILogger<ImportController> logger) {
			_logger = logger;
		}
	}
}