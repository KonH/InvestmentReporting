using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Market.Entity {
	public record VirtualBalance(decimal RealPrice, decimal VirtualPrice, CurrencyId Currency);
}