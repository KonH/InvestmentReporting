using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.State.Logic;

namespace InvestmentReporting.Meta.Logic {
	public sealed class DashboardManager {
		readonly IDashboardRepository  _repository;
		readonly AssetTagManager       _tagManager;
		readonly MetadataManager       _metadataManager;
		readonly ExchangeManager       _exchangeManager;
		readonly CurrencyConfiguration _currencyConfig;

		public DashboardManager(
			IDashboardRepository repository, AssetTagManager tagManager, MetadataManager metadataManager,
			ExchangeManager exchangeManager, CurrencyConfiguration currencyConfig) {
			_repository      = repository;
			_tagManager      = tagManager;
			_metadataManager = metadataManager;
			_exchangeManager = exchangeManager;
			_currencyConfig  = currencyConfig;
		}

		public async Task<DashboardConfigState> GetConfig(UserId user) {
			var configs = await _repository.GetUserDashboardConfigs(user);
			var dashboards = configs
				.Select(m => new DashboardConfig(
					new(m.Id),
					m.Name,
					m.Tags
						.Select(t => new DashboardConfigTag(new(t.Tag), t.Target))
						.ToArray()))
				.ToArray();
			return new DashboardConfigState(dashboards);
		}

		public async Task UpdateDashboard(UserId user, DashboardConfig dashboard) {
			var dashboardModel = new DashboardConfigModel(
				dashboard.Id,
				dashboard.Name,
				dashboard.Tags
					.Select(t => new DashboardConfigTagModel(t.Tag, t.Target))
					.ToList());
			await _repository.AddOrUpdateDashboard(user, dashboardModel);
		}

		public async Task<DashboardState> GetState(
			DateTimeOffset date, UserId user, DashboardId dashboardId, ReadOnlyState state, VirtualState virtualState) {
			var configs       = await _repository.GetUserDashboardConfigs(user);
			var dashboard     = configs.First(c => c.Id == dashboardId);
			var tagState      = await _tagManager.GetTags(user);
			var assetNames    = CollectAssetNames(state.Brokers.SelectMany(b => b.Inventory).ToArray(), user);
			var assetTags     = CollectAssetTags(tagState);
			var virtualAssets = CollectVirtualAssets(virtualState);
			var tags = dashboard.Tags
				.Select(t => {
					if ( !assetTags.TryGetValue(new(t.Tag), out var assetIsins) ) {
						assetIsins = Array.Empty<AssetISIN>();
					}
					var assets = assetIsins
						.Where(isin => virtualAssets.Any(a => a.Isin == isin))
						.Select(isin => {
							var name = assetNames.GetValueOrDefault(isin) ?? string.Empty;
							var sums = CalculateAssetSums(isin, virtualAssets, date);
							return new DashboardAsset(isin, name, sums);
						})
						.ToArray();
					var assetSums = AggregateSums(assets.Select(a => a.Sums));
					return new DashboardStateTag(t.Tag, assets, assetSums);
				})
				.ToArray();
			var tagSums = AggregateSums(tags.Select(t => t.Sums));
			return new(tags, tagSums);
		}

		IReadOnlyCollection<VirtualAsset> CollectVirtualAssets(VirtualState virtualState) =>
			virtualState.Balances
				.SelectMany(b => b.Inventory)
				.Where(a => a.Count > 0)
				.ToArray();

		IReadOnlyDictionary<AssetISIN, string> CollectAssetNames(
			IReadOnlyCollection<ReadOnlyAsset> assets, UserId user) =>
			assets
				.GroupBy(a => a.Isin)
				.ToDictionary(
					a => a.Key,
					a => {
						var isin     = a.First().Isin;
						var metadata = _metadataManager.GetMetadataWithFallback(isin, user);
						return metadata.Name;
					});

		IReadOnlyDictionary<AssetTag, IReadOnlyCollection<AssetISIN>> CollectAssetTags(AssetTagState tagState) =>
			tagState.Tags
				.ToDictionary(
					t => t,
					t => (IReadOnlyCollection<AssetISIN>) tagState.Assets
						.Where(a => a.Tags.Contains(t))
						.Select(a => a.Isin)
						.ToArray());

		IReadOnlyDictionary<CurrencyCode, SumState> CalculateAssetSums(
			AssetISIN isin, IReadOnlyCollection<VirtualAsset> allAssets, DateTimeOffset date) {
			var assets = allAssets
				.Where(a => a.Isin == isin)
				.ToArray();
			if ( assets.Length == 0 ) {
				return new Dictionary<CurrencyCode, SumState>();
			}
			var baseCurrency   = assets[0].Currency;
			var baseRealSum    = assets.Sum(a => a.RealSum);
			var baseVirtualSum = assets.Sum(a => a.VirtualSum);
			return _currencyConfig.GetAll()
				.ToDictionary(
					c => c,
					c => {
						if ( c == baseCurrency ) {
							return new SumState(baseRealSum, baseVirtualSum);
						}
						var realSum    = _exchangeManager.Exchange(baseCurrency, c, baseRealSum, date);
						var virtualSum = _exchangeManager.Exchange(baseCurrency, c, baseRealSum, date);
						return new SumState(realSum, virtualSum);
					}
				);
		}

		IReadOnlyDictionary<CurrencyCode, SumState> AggregateSums(
			IEnumerable<IReadOnlyDictionary<CurrencyCode, SumState>> sums) =>
			sums.Aggregate(
				new Dictionary<CurrencyCode, SumState>(),
				(dict, sumState) => {
					foreach ( var (currency, sum) in sumState ) {
						if ( !dict.TryGetValue(currency, out var aggregateSum) ) {
							aggregateSum = new SumState(0, 0);
						}
						dict[currency] = aggregateSum with {
							RealSum = aggregateSum.RealSum + sum.RealSum,
							VirtualSum = aggregateSum.VirtualSum + sum.VirtualSum
						};
					}
					return dict;
				});
	}
}