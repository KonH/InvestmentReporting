using InvestmentReporting.Data.Core.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvestmentReporting.Data.Mongo.Model {
	public sealed class StateEventModel {
		[BsonId]
		public ObjectId Id { get; set; }

		public ICommandModel? Model { get; set; }
	}
}