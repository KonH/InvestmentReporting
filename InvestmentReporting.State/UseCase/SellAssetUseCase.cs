using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class SellAssetUseCase {
		readonly IStateManager     _stateManager;
		readonly AddIncomeUseCase  _addIncome;
		readonly AddExpenseUseCase _addExpense;

		public SellAssetUseCase(IStateManager stateManager, AddIncomeUseCase addIncome, AddExpenseUseCase addExpense) {
			_stateManager = stateManager;
			_addIncome    = addIncome;
			_addExpense   = addExpense;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId brokerId, AccountId payAccountId, AccountId feeAccountId,
			AssetId assetId, decimal price, decimal fee, int count) {
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
					await _addIncome.Handle(
						date, user, brokerId, payAccountId, price, IncomeCategory.SellAsset, assetId);
					break;
			}
			switch ( fee ) {
				case < 0:
					throw new InvalidPriceException();
				case 0:
					break;
				default:
					await _addExpense.Handle(
						date, user, brokerId, feeAccountId, fee, ExpenseCategory.SellAssetFee, assetId);
					break;
			}
			await _stateManager.AddCommand(new ReduceAssetCommand(date, user, brokerId, assetId, count));
		}
	}
}