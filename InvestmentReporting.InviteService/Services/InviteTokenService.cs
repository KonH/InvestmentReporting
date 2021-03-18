using Microsoft.Extensions.Logging;

namespace InvestmentReporting.InviteService.Services {
	public sealed class InviteTokenService {
		readonly ILogger _logger;

		public string ActiveToken { get; private set; } = string.Empty;

		public InviteTokenService(ILogger<InviteTokenService> logger) {
			_logger = logger;
		}

		public void Rotate() {
			ActiveToken = System.Guid.NewGuid().ToString();
			_logger.LogInformation($"New invite token is '{ActiveToken}'");
		}
	}
}