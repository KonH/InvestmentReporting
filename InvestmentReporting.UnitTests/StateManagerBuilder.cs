using System;
using System.Collections.Generic;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.UnitTests {
	sealed class StateManagerBuilder {
		readonly List<ICommandModel> _commands = new();

		DateTimeOffset _date       = DateTimeOffset.MinValue;
		string         _userId     = string.Empty;
		string         _brokerId   = string.Empty;
		string         _currencyCode = string.Empty;

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
			With(new CreateBrokerModel(_date, _userId, _brokerId, string.Empty));
			return this;
		}

		public StateManagerBuilder With(CurrencyCode currency) {
			_currencyCode = currency.ToString();
			return this;
		}

		public StateManagerBuilder With(AccountId account) {
			With(new CreateAccountModel(_date, _userId, _brokerId, account.ToString(), _currencyCode, string.Empty));
			return this;
		}

		public StateManagerBuilder With(AssetId asset, AssetISIN isin, int count) {
			With(new AddAssetModel(_date, _userId, _brokerId, asset, isin, _currencyCode, count));
			return this;
		}

		public StateManagerBuilder With(ICommandModel command) {
			_commands.Add(command);
			return this;
		}

		public StateManager Build() =>
			new(new InMemoryStateRepository(
				new TestLoggerFactory().CreateLogger<InMemoryStateRepository>(), _commands));
	}
}