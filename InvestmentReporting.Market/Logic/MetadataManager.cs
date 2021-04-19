using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.State.Logic;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class MetadataManager {
		readonly ILogger                  _logger;
		readonly IAssetMetadataRepository _repository;
		readonly IStateManager            _stateManager;

		public MetadataManager(
			ILogger<MetadataManager> logger, IAssetMetadataRepository repository, IStateManager stateManager) {
			_logger       = logger;
			_repository   = repository;
			_stateManager = stateManager;
		}

		public AssetMetadata GetMetadataWithFallback(AssetISIN isin, UserId user) {
			var metadata = GetMetadata(isin);
			return (metadata != null) ? metadata : GetFallbackMetadata(isin, user);
		}

		AssetMetadata? GetMetadata(AssetISIN isin) {
			var models = _repository.GetAll();
			var model  = models.FirstOrDefault(m => m.Isin == isin);
			return (model != null)
				? new AssetMetadata(new(model.Isin), new(model.Figi), model.Name, new(model.Type))
				: null;
		}

		AssetMetadata GetFallbackMetadata(AssetISIN isin, UserId user) {
			var rawName = _stateManager.ReadState(user).Brokers
				.SelectMany(b => b.Inventory)
				.Where(a => a.Isin == isin)
				.Select(a => a.RawName)
				.FirstOrDefault();
			return new AssetMetadata(isin, null, rawName ?? string.Empty, null);
		}

		public async Task TryAdd(MarketInstrument instrument) {
			var models = _repository.GetAll();
			if ( models.Any(m => m.Isin == instrument.Isin) ) {
				_logger.LogTrace($"Instrument '{instrument.Isin}' already present");
				return;
			}
			_logger.LogTrace($"Add instrument: {instrument}");
			await _repository.Add(new AssetMetadataModel(
				instrument.Isin, instrument.Figi, instrument.Name, instrument.Type.ToString()));
		}

		public IReadOnlyCollection<AssetMetadata> GetAll() =>
			_repository.GetAll()
				.Select(m => new AssetMetadata(new(m.Isin), new(m.Figi), m.Name, new(m.Type)))
				.ToArray();
	}
}