using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.Meta.Entity;

namespace InvestmentReporting.Meta.Logic {
	public sealed class DashboardManager {
		readonly IDashboardRepository _repository;
		readonly AssetTagManager      _tagManager;
		readonly MetadataManager      _metadataManager;
		readonly ExchangeManager      _exchangeManager;

		public DashboardManager(
			IDashboardRepository repository, AssetTagManager tagManager, MetadataManager metadataManager,
			ExchangeManager exchangeManager) {
			_repository      = repository;
			_tagManager      = tagManager;
			_metadataManager = metadataManager;
			_exchangeManager = exchangeManager;
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
			var currencies    = state.Currencies;
			var tagState      = await _tagManager.GetTags(user);
			var assetNames    = CollectAssetNames(state.Brokers.SelectMany(b => b.Inventory).ToArray());
			var assetTags     = CollectAssetTags(tagState);
			var virtualAssets = virtualState.Balances.SelectMany(b => b.Inventory).ToArray();
			var tags = dashboard.Tags
				.Select(t => {
					if ( !assetTags.TryGetValue(new(t.Tag), out var assetIsins) ) {
						assetIsins = Array.Empty<AssetISIN>();
					}
					var assets = assetIsins
						.Select(isin => {
							var name = assetNames.GetValueOrDefault(isin) ?? string.Empty;
							var sums = CalculateAssetSums(isin, currencies, virtualAssets, date, user);
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

		IReadOnlyDictionary<AssetISIN, string> CollectAssetNames(IReadOnlyCollection<ReadOnlyAsset> assets) =>
			assets
				.GroupBy(a => a.Isin)
				.ToDictionary(
					a => a.Key,
					a => {
						var isin     = a.First().Isin;
						var metadata = _metadataManager.GetMetadata(isin);
						return metadata?.Name ?? string.Empty;
					});

		IReadOnlyDictionary<AssetTag, IReadOnlyCollection<AssetISIN>> CollectAssetTags(AssetTagState tagState) =>
			tagState.Tags
				.ToDictionary(
					t => t,
					t => (IReadOnlyCollection<AssetISIN>) tagState.Assets
						.Where(a => a.Tags.Contains(t))
						.Select(a => a.Isin)
						.ToArray());

		IReadOnlyDictionary<CurrencyId, SumState> CalculateAssetSums(
			AssetISIN isin, IReadOnlyCollection<ReadOnlyCurrency> currencies,
			IReadOnlyCollection<VirtualAsset> allAssets, DateTimeOffset date, UserId user) {
			var assets = allAssets
				.Where(a => a.Isin == isin)
				.ToArray();
			if ( assets.Length == 0 ) {
				return new Dictionary<CurrencyId, SumState>();
			}
			var baseCurrency   = assets[0].Currency;
			var baseRealSum    = assets.Sum(a => a.RealSum);
			var baseVirtualSum = assets.Sum(a => a.VirtualSum);
			return currencies
				.ToDictionary(
					c => c.Id,
					c => {
						if ( c.Id == baseCurrency ) {
							return new SumState(baseRealSum, baseVirtualSum);
						}
						var realSum    = _exchangeManager.Exchange(baseCurrency, c.Id, baseRealSum, date, user);
						var virtualSum = _exchangeManager.Exchange(baseCurrency, c.Id, baseRealSum, date, user);
						return new SumState(realSum, virtualSum);
					}
				);
		}

		IReadOnlyDictionary<CurrencyId, SumState> AggregateSums(
			IEnumerable<IReadOnlyDictionary<CurrencyId, SumState>> sums) =>
			sums.Aggregate(
				new Dictionary<CurrencyId, SumState>(),
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