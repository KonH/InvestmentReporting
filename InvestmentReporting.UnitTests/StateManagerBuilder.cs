using System;
using System.Collections.Generic;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.UnitTests {
	sealed class StateManagerBuilder {
		readonly List<ICommandModel> _commands = new();

		DateTimeOffset _date       = DateTimeOffset.MinValue;
		string         _userId     = string.Empty;
		string         _brokerId   = string.Empty;
		string         _currencyId = string.Empty;

		public StateManagerBuilder With(DateTimeOffset date) {
			_date = date;
			return this;
		}

		public StateManagerBuilder With(UserId user) {
			_userId = user.ToString();
			return this;
		}

		public StateManagerBuilder With(BrokerId broker) {
			_brokerId = broker.ToString();
			_commands.Add(new CreateBrokerModel(_date, _userId, _brokerId, string.Empty));
			return this;
		}

		public StateManagerBuilder With(CurrencyId currency) {
			_currencyId = currency.ToString();
			_commands.Add(new CreateCurrencyModel(_date, _userId, _currencyId, string.Empty, string.Empty));
			return this;
		}

		public StateManagerBuilder With(AccountId account) {
			_commands.Add(new CreateAccountModel(_date, _userId, _brokerId, account.ToString(), _currencyId, string.Empty));
			return this;
		}

		public StateManager Build() =>
			new(new InMemoryStateRepository(_commands));
	}
}