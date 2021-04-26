namespace InvestmentReporting.Market.Dto {
	public record DividendStateDto(
		decimal PreviousDividend, decimal LastDividend, decimal YearDividend, decimal DividendSum);
}