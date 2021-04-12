using System;

namespace InvestmentReporting.Market.Logic {
	public sealed class EnvironmentApiKeyProvider : IApiKeyProvider {
		public string ApiKey { get; }

		public EnvironmentApiKeyProvider() {
			ApiKey = Environment.GetEnvironmentVariable("TINKOFF_API_SANDBOX_KEY") ?? string.Empty;
		}
	}
}