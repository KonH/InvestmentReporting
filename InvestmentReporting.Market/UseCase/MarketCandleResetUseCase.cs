using System.Threading.Tasks;
using InvestmentReporting.Market.Logic;

namespace InvestmentReporting.Market.UseCase {
	public sealed class MarketCandleResetUseCase {
		readonly AssetPriceManager    _assetPriceManager;
		readonly CurrencyPriceManager _currencyPriceManager;

		public MarketCandleResetUseCase(
			AssetPriceManager assetPriceManager, CurrencyPriceManager currencyPriceManager) {
			_assetPriceManager    = assetPriceManager;
			_currencyPriceManager = currencyPriceManager;
		}

		public async Task Handle() {
			await _assetPriceManager.Reset();
			await _currencyPriceManager.Reset();
		}
	}
}