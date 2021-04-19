using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class AssetController : ControllerBase {
		readonly ILogger          _logger;
		readonly BuyAssetUseCase  _buyUseCase;
		readonly SellAssetUseCase _sellUseCase;

		public AssetController(
			ILogger<AssetController> logger, BuyAssetUseCase buyUseCase, SellAssetUseCase sellUseCase) {
			_logger      = logger;
			_buyUseCase  = buyUseCase;
			_sellUseCase = sellUseCase;
		}

		[HttpPost("BuyAsset")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> BuyAsset(
			[Required] DateTimeOffset date, [Required] string broker, [Required] string payAccount,
			[Required] string feeAccount, [Required] string name, [Required] string category, [Required] string isin,
			[Required] decimal price, [Required] decimal fee, [Required] int count) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation(
				$"Buy asset (name: '{name}', category: '{category}', isin: '{isin}', price: {price}, fee: {fee}, count: {count}) " +
				$"at '{date}' for user '{userId}' on broker '{broker}' and pay account '{payAccount}' / fee account '{feeAccount}'");
			try {
				await _buyUseCase.Handle(
					date, userId, new(broker), new(payAccount), new(feeAccount),
					new(isin), price, fee, name, count);
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}

		[HttpPost("SellAsset")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> SellAsset(
			[Required] DateTimeOffset date, [Required] string broker, [Required] string payAccount,
			[Required] string feeAccount, [Required] string asset, [Required] decimal price, [Required] decimal fee,
			[Required] int count) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation(
				$"Sell asset (asset: '{asset}', price: {price}, fee: {fee}, count: {count}) " +
				$"at '{date}' for user '{userId}' on broker '{broker}' and pay account '{payAccount}' / fee account '{feeAccount}'");
			try {
				await _sellUseCase.Handle(
					date, userId, new(broker), new(payAccount), new(feeAccount),
					new(asset), price, fee, count);
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}
	}
}