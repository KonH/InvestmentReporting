using System;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class AddAssetTagUseCase {
		public Task Handle(UserId user, AssetISIN asset, AssetTag tag) {
			throw new NotImplementedException();
		}
	}
}