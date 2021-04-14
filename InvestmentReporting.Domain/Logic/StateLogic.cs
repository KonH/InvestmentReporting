using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	static class StateLogic {
		public static void Configure(this StateManager manager) {
			manager.Bind<CreateBrokerCommand, CreateBrokerModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Id, cmd.DisplayName),
				m => new(m.Date, new(m.User), new(m.Id), m.DisplayName),
				(state, m) => {
					state.Brokers.Add(new(
						new(m.Id), m.DisplayName, new List<Account>(), new List<Asset>()));
				}
			);
			manager.Bind<CreateCurrencyCommand, CreateCurrencyModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Id, cmd.Code, cmd.Format),
				m => new(m.Date, new(m.User), new(m.Id), new(m.Code), new(m.Format)),
				(state, m) => {
					state.Currencies.Add(new(
						new(m.Id), new(m.Code), new(m.Format)));
				}
			);
			manager.Bind<CreateAccountCommand, CreateAccountModel>(
				cmd => new (cmd.Date, cmd.User, cmd.Broker, cmd.Id, cmd.Currency, cmd.DisplayName),
				m => new (m.Date, new(m.User), new(m.Broker), new(m.Id), new(m.Currency), m.DisplayName),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					broker.Accounts.Add(new(
						new(m.Id), new(m.Currency), m.DisplayName));
				}
			);
			manager.Bind<AddIncomeCommand, AddIncomeModel>(
				cmd => {
					var asset = (cmd.Asset != null) ? (string) cmd.Asset : null;
					return new(
						cmd.Date, cmd.User, cmd.Broker, cmd.Account, cmd.Id,
						cmd.Amount, cmd.Category, asset);
				},
				m => {
					var asset = (m.Asset != null) ? new AssetId(m.Asset) : null;
					return new(
						m.Date, new(m.User), new(m.Broker), new(m.Account), new(m.Id),
						m.Amount, new(m.Category), asset);
				},
				(state, m) => {
					var brokerId  = new BrokerId(m.Broker);
					var broker    = state.Brokers.First(b => b.Id == brokerId);
					var accountId = new AccountId(m.Account);
					var account   = broker.Accounts.First(a => a.Id == accountId);
					account.Balance += m.Amount;
				}
			);
			manager.Bind<AddExpenseCommand, AddExpenseModel>(
				cmd => {
					var asset = (cmd.Asset != null) ? (string)cmd.Asset : null;
					return new(cmd.Date, cmd.User, cmd.Broker, cmd.Account, cmd.Id,
						cmd.Amount, cmd.Category, asset);
				},
				m => {
					var asset = (m.Asset != null) ? new AssetId(m.Asset) : null;
					return new(m.Date, new(m.User), new(m.Broker), new(m.Account), new(m.Id),
						m.Amount, new(m.Category), asset);
				},
				(state, m) => {
					var brokerId  = new BrokerId(m.Broker);
					var broker    = state.Brokers.First(b => b.Id == brokerId);
					var accountId = new AccountId(m.Account);
					var account   = broker.Accounts.First(a => a.Id == accountId);
					account.Balance -= m.Amount;
				}
			);
			manager.Bind<AddAssetCommand, AddAssetModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Broker, cmd.Asset, cmd.Isin, cmd.Count),
				m => new(m.Date, new(m.User), new(m.Broker), new(m.Id), new(m.Isin), m.Count),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					broker.Inventory.Add(new(
						new(m.Asset), new(m.Isin), m.Count));
				}
			);
			manager.Bind<ReduceAssetCommand, ReduceAssetModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Broker, cmd.Asset, cmd.Count),
				m => new(m.Date, new(m.User), new(m.Broker), new(m.Id), m.Count),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					var asset    = broker.Inventory.First(a => a.Id == m.Asset);
					asset.Count -= m.Count;
				}
			);
			manager.Bind<IncreaseAssetCommand, IncreaseAssetModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Broker, cmd.Id, cmd.Count),
				m => new(m.Date, new(m.User), new(m.Broker), new(m.Id), m.Count),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					var asset    = broker.Inventory.First(a => a.Id == m.Id);
					asset.Count += m.Count;
				}
			);
		}
	}
}