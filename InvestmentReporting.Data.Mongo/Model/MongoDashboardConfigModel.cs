using InvestmentReporting.Data.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvestmentReporting.Data.Mongo.Model {
	public sealed class MongoDashboardConfigModel {
		[BsonId]
		public ObjectId Id { get; set; }

		public string User { get; set; } = string.Empty;

		public DashboardConfigModel? Model { get; set; }
	}
}