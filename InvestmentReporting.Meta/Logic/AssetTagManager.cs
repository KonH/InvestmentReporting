using System;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Meta.Logic {
	public sealed class AssetTagManager {
		readonly ILogger             _logger;
		readonly IAssetTagRepository _repository;

		public AssetTagManager(ILogger<AssetTagManager> logger, IAssetTagRepository repository) {
			_logger     = logger;
			_repository = repository;
		}

		public AssetTagState GetTags(UserId user) {
			throw new NotImplementedException();
		}

		public Task AddTag(UserId user, AssetISIN asset, AssetTag tag) {
			throw new NotImplementedException();
		}

		public Task RemoveTag(UserId user, AssetISIN asset, AssetTag tag) {
			throw new NotImplementedException();
		}
	}
}