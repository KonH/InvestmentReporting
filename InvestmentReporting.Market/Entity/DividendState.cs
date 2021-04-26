namespace InvestmentReporting.Market.Entity {
	public record DividendState(
		decimal PreviousDividend, decimal LastDividend, decimal YearDividend, decimal DividendSum);
}