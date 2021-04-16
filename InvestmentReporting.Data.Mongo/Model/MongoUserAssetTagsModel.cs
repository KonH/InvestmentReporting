using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvestmentReporting.Data.Mongo.Model {
	public record MongoUserAssetTagsModel {
		[BsonId]
		public ObjectId Id { get; set; }

		public string User { get; set; } = string.Empty;

		public Dictionary<string, List<string>> Tags { get; set; } = new();
	}
}