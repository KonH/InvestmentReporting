using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class BuyAssetUseCase {
		readonly ExpenseCategory _buyAssetCategory    = new("Asset Buy");
		readonly ExpenseCategory _buyAssetFeeCategory = new("Asset Buy Broker Fee");

		readonly StateManager      _stateManager;
		readonly IIdGenerator      _idGenerator;
		readonly AddExpenseUseCase _addExpense;

		public BuyAssetUseCase(StateManager stateManager, IIdGenerator idGenerator, AddExpenseUseCase addExpense) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
			_addExpense   = addExpense;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId brokerId, AccountId payAccountId, AccountId feeAccountId,
			string name, AssetCategory category, AssetTicker ticker, decimal price, decimal fee, int count) {
			if ( string.IsNullOrWhiteSpace(name) ) {
				throw new InvalidAssetException();
			}
			if ( string.IsNullOrWhiteSpace(category.Value) ) {
				throw new InvalidAssetException();
			}
			if ( string.IsNullOrWhiteSpace(ticker.Value) ) {
				throw new InvalidAssetException();
			}
			if ( count <= 0 ) {
				throw new InvalidCountException();
			}
			var state  = await _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new InvalidBrokerException();
			}
			var payAccount = broker.Accounts.FirstOrDefault(a => a.Id == payAccountId);
			if ( payAccount == null ) {
				throw new InvalidAccountException();
			}
			var feeAccount = broker.Accounts.FirstOrDefault(a => a.Id == feeAccountId);
			if ( feeAccount == null ) {
				throw new InvalidAccountException();
			}
			switch ( price ) {
				case < 0:
					throw new InvalidPriceException();
				case 0:
					break;
				default:
					await _addExpense.Handle(
						date, user, brokerId, payAccountId, price, _buyAssetCategory);
					break;
			}
			switch ( fee ) {
				case < 0:
					throw new InvalidPriceException();
				case 0:
					break;
				default:
					await _addExpense.Handle(
						date, user, brokerId, feeAccountId, fee, _buyAssetFeeCategory);
					break;
			}
			var asset = broker.Inventory.FirstOrDefault(a => a.Ticker == ticker);
			if ( asset != null ) {
				await _stateManager.PushCommand(new IncreaseAssetCommand(date, user, brokerId, asset.Id, count));
				return;
			}
			var id = new AssetId(_idGenerator.GenerateNewId());
			await _stateManager.PushCommand(new AddAssetCommand(date, user, brokerId, id, name, category, ticker, count));
		}
	}
}