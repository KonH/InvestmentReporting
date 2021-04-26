using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class ReduceAssetUseCase {
		readonly IStateManager _stateManager;

		public ReduceAssetUseCase(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId brokerId, AssetId assetId, int count) {
			if ( count <= 0 ) {
				throw new InvalidCountException();
			}
			var state  = _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new InvalidBrokerException();
			}
			if ( string.IsNullOrWhiteSpace(assetId) ) {
				throw new InvalidAssetException();
			}
			var asset = broker.Inventory.FirstOrDefault(a => a.Id == assetId);
			if ( asset == null ) {
				throw new AssetNotFoundException();
			}
			var remainingCount = asset.Count - count;
			if ( remainingCount < 0 ) {
				throw new InvalidCountException();
			}
			await _stateManager.AddCommand(new ReduceAssetCommand(date, user, brokerId, assetId, count));
		}
	}
}