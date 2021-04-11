using System;

namespace InvestmentReporting.MarketService.Services {
	public sealed class ApiKeyProvider {
		public readonly string ApiKey;

		public ApiKeyProvider() {
			ApiKey = Environment.GetEnvironmentVariable("TINKOFF_API_SANDBOX_KEY") ?? string.Empty;
		}
	}
}