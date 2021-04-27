using System;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class EnvironmentApiKeyProvider : IApiKeyProvider {
		public string ApiKey { get; }

		public EnvironmentApiKeyProvider(ILogger<EnvironmentApiKeyProvider> logger) {
			ApiKey = Environment.GetEnvironmentVariable("TINKOFF_API_SANDBOX_KEY") ?? string.Empty;
			if ( string.IsNullOrWhiteSpace(ApiKey) ) {
				logger.LogError(
					"Please provide valid TINKOFF_API_SANDBOX_KEY environment variable " +
					"(see https://tinkoffcreditsystems.github.io/invest-openapi/auth/)");
			}
		}
	}
}