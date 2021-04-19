using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Logic {
	public static class StateManagerExtensions {
		static readonly DateTimeOffset _minDate = DateTimeOffset.MinValue;
		static readonly DateTimeOffset _maxDate = DateTimeOffset.MaxValue;

		public static IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(this IStateManager stateManager) =>
			stateManager.ReadStates(_maxDate);

		public static ReadOnlyState ReadState(this IStateManager stateManager, UserId user) =>
			stateManager.ReadState(_maxDate, user);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate)
			where TCommand : class, ICommand =>
			Filter<TCommand>(stateManager.ReadCommands(startDate, endDate));

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset endDate)
			where TCommand : class, ICommand =>
			stateManager.ReadCommands<TCommand>(_minDate, endDate);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager)
			where TCommand : class, ICommand =>
			stateManager.ReadCommands<TCommand>(_minDate, _maxDate);

		public static IEnumerable<ICommand> ReadCommands(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user) =>
			stateManager.ReadCommands(startDate, endDate)
				.Where(c => c.User == user);

		public static IEnumerable<ICommand> ReadCommands(
			this IStateManager stateManager, DateTimeOffset endDate, UserId user) =>
			stateManager.ReadCommands(_minDate, endDate, user);

		public static IEnumerable<ICommand> ReadCommands(
			this IStateManager stateManager, UserId user) =>
			stateManager.ReadCommands(_minDate, _maxDate, user);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user)
			where TCommand : class, ICommand =>
			Filter<TCommand>(stateManager.ReadCommands(startDate, endDate, user));

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, UserId user)
			where TCommand : class, ICommand =>
			stateManager.ReadCommands<TCommand>(_minDate, _maxDate, user);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId broker)
			where TCommand : class, IBrokerCommand =>
			stateManager.ReadCommands<TCommand>(startDate, endDate, user)
				.Where(c => c.Broker == broker);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset endDate, UserId user, BrokerId broker)
			where TCommand : class, IBrokerCommand =>
			stateManager.ReadCommands<TCommand>(_minDate, endDate, user, broker);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, UserId user, BrokerId broker)
			where TCommand : class, IBrokerCommand =>
			stateManager.ReadCommands<TCommand>(_minDate, _maxDate, user, broker);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId broker, AccountId account)
			where TCommand : class, IAccountCommand =>
			stateManager.ReadCommands<TCommand>(startDate, endDate, user, broker)
				.Where(c => c.Account == account);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId broker, AssetId asset)
			where TCommand : class, IAssetCommand =>
			stateManager.ReadCommands<TCommand>(startDate, endDate, user, broker)
				.Where(c => c.Asset == asset);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset startDate, DateTimeOffset endDate, UserId user, AssetId asset)
			where TCommand : class, IAssetCommand =>
			stateManager.ReadCommands<TCommand>(startDate, endDate, user)
				.Where(c => c.Asset == asset);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, DateTimeOffset endDate, UserId user, AssetId asset)
			where TCommand : class, IAssetCommand =>
			stateManager.ReadCommands<TCommand>(_minDate, endDate, user, asset);

		public static IEnumerable<TCommand> ReadCommands<TCommand>(
			this IStateManager stateManager, UserId user, AssetId asset)
			where TCommand : class, IAssetCommand =>
			stateManager.ReadCommands<TCommand>(_minDate, _maxDate, user, asset);

		static IEnumerable<TCommand> Filter<TCommand>(IEnumerable<ICommand> commands) where TCommand : class, ICommand =>
			commands
				.Select(c => c as TCommand)
				.Where(c => c != null)
				.Select(c => c!);
	}
}