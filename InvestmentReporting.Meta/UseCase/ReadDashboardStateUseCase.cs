using System;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Market.UseCase;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class ReadDashboardStateUseCase {
		readonly DashboardManager        _manager;
		readonly ReadStateUseCase        _readStateUseCase;
		readonly ReadVirtualStateUseCase _readVirtualStateUseCase;

		public ReadDashboardStateUseCase(
			DashboardManager manager, ReadStateUseCase readStateUseCase,
			ReadVirtualStateUseCase readVirtualStateUseCase) {
			_manager                 = manager;
			_readStateUseCase        = readStateUseCase;
			_readVirtualStateUseCase = readVirtualStateUseCase;
		}

		public async Task<DashboardState> Handle(DateTimeOffset date, UserId user, DashboardId dashboard) {
			var state        = _readStateUseCase.Handle(date, user);
			var virtualState = _readVirtualStateUseCase.Handle(date, user);
			return await _manager.GetState(date, user, dashboard, state, virtualState);
		}
	}
}