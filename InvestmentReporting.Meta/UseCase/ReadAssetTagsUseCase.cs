using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class ReadAssetTagsUseCase {
		readonly ILogger         _logger;
		readonly AssetTagManager _assetTagManager;

		public ReadAssetTagsUseCase(ILogger<ReadAssetTagsUseCase> logger, AssetTagManager assetTagManager) {
			_logger          = logger;
			_assetTagManager = assetTagManager;
		}

		public async Task<AssetTagState> Handle(UserId user) {
			_logger.LogTrace($"Request assets tags for user '{user}'");
			return await _assetTagManager.GetTags(user);
		}
	}
}