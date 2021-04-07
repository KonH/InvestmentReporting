using System;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.UnitTests {
	public sealed class TestLogger : ILogger {
		public void Log<TState>(
			LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
			var text = formatter(state, exception);
			Console.WriteLine(text);
		}

		public bool IsEnabled(LogLevel logLevel) => true;

		public IDisposable BeginScope<TState>(TState state) => throw new NotSupportedException();
	}

	public sealed class TestLoggerFactory : ILoggerFactory {
		public void Dispose() {}

		public ILogger CreateLogger(string categoryName) => new TestLogger();

		public void AddProvider(ILoggerProvider provider) {}
	}
}