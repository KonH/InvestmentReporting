using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Market.Dto;
using InvestmentReporting.Market.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MarketService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class VirtualStateController : ControllerBase {
		readonly ILogger                 _logger;
		readonly ReadVirtualStateUseCase _useCase;

		public VirtualStateController(ILogger<VirtualStateController> logger, ReadVirtualStateUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(VirtualStateDto), StatusCodes.Status200OK)]
		public IActionResult Get([Required] DateTimeOffset date) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve virtual state for user '{userId}' at {date}");
			var state = _useCase.Handle(date, userId);
			var balancesDto = state.Balances
				.Select(b => new VirtualBalanceDto(b.RealPrice, b.VirtualPrice, b.Currency))
				.ToArray();
			var inventoryDto = state.Inventory
				.Select(a => new VirtualAssetDto(
					a.Id, a.Broker, a.Isin,
					a.Name ?? string.Empty, a.Type ?? string.Empty,
					a.Count, a.RealPrice, a.VirtualPrice, a.Currency))
				.ToArray();
			var dto   = new VirtualStateDto(balancesDto, inventoryDto);
			return new JsonResult(dto);
		}
	}
}