using System.Collections.Generic;

namespace InvestmentReporting.Domain.Entity {
	public record State(List<Broker> Brokers, List<Currency> Currencies) {}
}