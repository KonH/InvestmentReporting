using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Meta.Dto;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MetaService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class DashboardStateController : ControllerBase {
		readonly ILogger                   _logger;
		readonly ReadDashboardStateUseCase _readUseCase;

		public DashboardStateController(
			ILogger<DashboardStateController> logger, ReadDashboardStateUseCase readUseCase) {
			_logger      = logger;
			_readUseCase = readUseCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(DashboardStateDto), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get([Required] DateTimeOffset date, [Required] string dashboard) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve dashboard state for user '{userId}' at '{date}'");
			var state = await _readUseCase.Handle(date, userId, new(dashboard));
			var dto   = new DashboardStateDto(
				state.Tags
					.Select(t => new DashboardStateTagDto(
						t.Tag,
						t.Assets
							.Select(a => new DashboardAssetDto(a.Isin, a.Name, ConvertSums(a.Sums)))
							.ToArray(),
						ConvertSums(t.Sums)))
					.ToArray(),
				ConvertSums(state.Sums));
			return new JsonResult(dto);
		}

		Dictionary<string, SumStateDto> ConvertSums(IReadOnlyDictionary<CurrencyCode, SumState> sums) =>
			sums.ToDictionary(s => s.Key.ToString(), s => new SumStateDto(s.Value.RealSum, s.Value.VirtualSum));
	}
}