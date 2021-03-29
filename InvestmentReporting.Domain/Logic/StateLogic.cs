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
				(state, m) => {
					state.Brokers.Add(new(
						new(m.Id), m.DisplayName, new List<Account>(), new List<Asset>()));
				}
			);
			manager.Bind<CreateCurrencyCommand, CreateCurrencyModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Id, cmd.Code, cmd.Format),
				(state, m) => {
					state.Currencies.Add(new(
						new(m.Id), new(m.Code), new(m.Format)));
				}
			);
			manager.Bind<CreateAccountCommand, CreateAccountModel>(
				cmd => new (cmd.Date, cmd.User, cmd.Broker, cmd.Id, cmd.Currency, cmd.DisplayName),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					broker.Accounts.Add(new(
						new(m.Id), new(m.Currency), m.DisplayName));
				}
			);
			manager.Bind<AddIncomeCommand, AddIncomeModel>(
				cmd => new(
					cmd.Date, cmd.User, cmd.Broker, cmd.Account, cmd.Id,
					cmd.Currency, cmd.Amount, cmd.ExchangeRate, cmd.Category),
				(state, m) => {
					var brokerId  = new BrokerId(m.Broker);
					var broker    = state.Brokers.First(b => b.Id == brokerId);
					var accountId = new AccountId(m.Account);
					var account   = broker.Accounts.First(a => a.Id == accountId);
					account.Balance += m.Amount * m.ExchangeRate;
				}
			);
			manager.Bind<AddExpenseCommand, AddExpenseModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Broker, cmd.Account, cmd.Id,
					cmd.Currency, cmd.Amount, cmd.ExchangeRate, cmd.Category),
				(state, m) => {
					var brokerId  = new BrokerId(m.Broker);
					var broker    = state.Brokers.First(b => b.Id == brokerId);
					var accountId = new AccountId(m.Account);
					var account   = broker.Accounts.First(a => a.Id == accountId);
					account.Balance -= m.Amount * m.ExchangeRate;
				}
			);
			manager.Bind<AddAssetCommand, AddAssetModel>(
				cmd => new(cmd.Date, cmd.User, cmd.Broker, cmd.Id, cmd.Name, cmd.Category, cmd.Ticker,
					cmd.BoughtCurrency, cmd.BoughtPrice, cmd.Count),
				(state, m) => {
					var brokerId = new BrokerId(m.Broker);
					var broker   = state.Brokers.First(b => b.Id == brokerId);
					broker.Inventory.Add(new(
						new(m.Id), m.Name, new(m.Category), new(m.Ticker),
						new(m.BoughtCurrency), m.BoughtPrice, m.Date, m.Count));
				}
			);
		}
	}
}