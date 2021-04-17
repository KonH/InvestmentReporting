using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class RemoveAssetTagUseCase {
		readonly ILogger         _logger;
		readonly AssetTagManager _assetTagManager;

		public RemoveAssetTagUseCase(ILogger<RemoveAssetTagUseCase> logger, AssetTagManager assetTagManager) {
			_logger          = logger;
			_assetTagManager = assetTagManager;
		}

		public async Task Handle(UserId user, AssetISIN asset, AssetTag tag) {
			_logger.LogTrace($"Remove tag '{tag}' from asset '{asset}' for user '{user}'");
			await _assetTagManager.RemoveTag(user, asset, tag);
		}
	}
}