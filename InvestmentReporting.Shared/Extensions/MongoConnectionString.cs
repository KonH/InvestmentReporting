using System;

namespace InvestmentReporting.Shared.Extensions {
	public static class MongoConnectionString {
		public static string Create() {
			var mongoUserName = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME");
			var mongoPassword = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");
			return $"mongodb://{mongoUserName}:{mongoPassword}@mongo:27017/InvestmentReporting?authSource=admin";
		}
	}
}