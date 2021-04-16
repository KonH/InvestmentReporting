using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Dto;
using InvestmentReporting.Meta.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MetaService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class TagController : ControllerBase {
		readonly ILogger               _logger;
		readonly ReadAssetTagsUseCase  _readTagsUseCase;
		readonly AddAssetTagUseCase    _addTagUseCase;
		readonly RemoveAssetTagUseCase _removeTagUseCase;

		public TagController(
			ILogger<TagController> logger, ReadAssetTagsUseCase readTagsUseCase,
			AddAssetTagUseCase addTagUseCase, RemoveAssetTagUseCase removeTagUseCase) {
			_logger           = logger;
			_readTagsUseCase  = readTagsUseCase;
			_addTagUseCase    = addTagUseCase;
			_removeTagUseCase = removeTagUseCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(AssetTagStateDto), StatusCodes.Status200OK)]
		public IActionResult Get() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve asset tags for user '{userId}'");
			var state = _readTagsUseCase.Handle(userId);
			var dto   = new AssetTagStateDto(state.Assets
				.Select(a => new AssetTagSetDto(a.Isin, a.Name, a.Tags.Select(t => t.ToString()).ToArray()))
				.ToArray());
			return new JsonResult(dto);
		}

		[HttpPost("add")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> AddTag(string asset, string tag) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Add tag for user '{userId}' asset '{asset}': '{tag}'");
			await _addTagUseCase.Handle(userId, new(asset), new(tag));
			return Ok();
		}

		[HttpPost("remove")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> RemoveTag(string asset, string tag) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Remove tag for user '{userId}' asset '{asset}': '{tag}'");
			await _removeTagUseCase.Handle(userId, new(asset), new(tag));
			return Ok();
		}
	}
}