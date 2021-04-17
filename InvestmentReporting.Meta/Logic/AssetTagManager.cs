using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.Meta.Entity;

namespace InvestmentReporting.Meta.Logic {
	public sealed class AssetTagManager {
		readonly IStateManager       _stateManager;
		readonly IAssetTagRepository _repository;
		readonly MetadataManager     _metadataManager;

		public AssetTagManager(
			IStateManager stateManager, IAssetTagRepository repository, MetadataManager metadataManager) {
			_stateManager    = stateManager;
			_repository      = repository;
			_metadataManager = metadataManager;
		}

		public async Task<AssetTagState> GetTags(UserId user) {
			var state = _stateManager.ReadState(user);
			var model = await _repository.Get(user) ?? new(user, new Dictionary<string, List<string>>());
			var assetIsins = state.Brokers
				.SelectMany(b => b.Inventory)
				.Select(a => a.Isin)
				.Distinct();
			var assetTags = assetIsins
				.Select(isin => {
					var name      = _metadataManager.GetMetadata(isin)?.Name ?? string.Empty;
					var modelTags = model.Tags.TryGetValue(isin, out var value) ? value : new List<string>();
					var tags      = modelTags.Select(t => new AssetTag(t)).ToHashSet();
					return new AssetTagSet(isin, name, tags);
				})
				.ToArray();
			var commonTags = assetTags
				.SelectMany(a => a.Tags)
				.Distinct()
				.ToHashSet();
			return new(commonTags, assetTags);
		}

		public async Task AddTag(UserId user, AssetISIN asset, AssetTag tag) {
			var model = await _repository.Get(user);
			Func<UserAssetTagsModel, Task> apply =
				(model != null) ? _repository.Update : _repository.Add;
			if ( model == null ) {
				model = new UserAssetTagsModel(user, new Dictionary<string, List<string>>());
			}
			if ( !model.Tags.TryGetValue(asset, out var tags) ) {
				tags = new List<string>();
				model.Tags[asset] = tags;
			}
			tags.Add(tag);
			await apply(model);
		}

		public async Task RemoveTag(UserId user, AssetISIN asset, AssetTag tag) {
			var model = await _repository.Get(user);
			if ( model == null ) {
				return;
			}
			if ( !model.Tags.TryGetValue(asset, out var tags) ) {
				return;
			}
			tags.Remove(tag);
			await _repository.Update(model);
		}
	}
}