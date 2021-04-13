using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class MetadataManager {
		readonly ILogger                  _logger;
		readonly IAssetMetadataRepository _repository;

		public MetadataManager(ILogger<MetadataManager> logger, IAssetMetadataRepository repository) {
			_logger     = logger;
			_repository = repository;
		}

		public AssetMetadata? GetMetadata(AssetISIN isin) {
			var models = _repository.GetAll();
			var model  = models.FirstOrDefault(m => m.Isin == isin);
			return (model != null)
				? new AssetMetadata(new(model.Isin), new(model.Figi), model.Name, new(model.Type))
				: null;
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