using InvestmentReporting.Data.Core.Repository;
using MongoDB.Bson;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class ObjectIdGenerator : IIdGenerator {
		public string GenerateNewId() => ObjectId.GenerateNewId().ToString();
	}
}