using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class AddAssetTagUseCase {
		readonly ILogger         _logger;
		readonly AssetTagManager _assetTagManager;

		public AddAssetTagUseCase(ILogger<AddAssetTagUseCase> logger, AssetTagManager assetTagManager) {
			_logger          = logger;
			_assetTagManager = assetTagManager;
		}

		public async Task Handle(UserId user, AssetISIN asset, AssetTag tag) {
			_logger.LogTrace($"Add tag '{tag}' to asset '{asset}' for user '{user}'");
			await _assetTagManager.AddTag(user, asset, tag);
		}
	}
}