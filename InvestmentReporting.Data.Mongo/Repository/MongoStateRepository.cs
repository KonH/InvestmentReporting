using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoStateRepository : IStateRepository {
		readonly ILogger                           _logger;
		readonly IMongoCollection<StateEventModel> _collection;

		public MongoStateRepository(ILogger<MongoStateRepository> logger, IMongoDatabase database) {
			_logger = logger;
			RegisterCommandModels();
			_collection = database.GetCollection<StateEventModel>("stateEvents");
		}

		void RegisterCommandModels() {
			var assembly = typeof(ICommandModel).Assembly;
			var modelTypes = assembly.GetTypes()
				.Where(t => t.IsAssignableTo(typeof(ICommandModel)))
				.Where(t => !t.IsInterface)
				.ToArray();
			var targetOpenMethod = typeof(BsonClassMap)
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(m => m.Name == nameof(BsonClassMap.RegisterClassMap))
				.Where(m => m.IsGenericMethod)
				.Single(m => m.GetParameters().Length == 0);
			foreach ( var modelType in modelTypes ) {
				var method = targetOpenMethod.MakeGenericMethod(modelType);
				method.Invoke(null, Array.Empty<object>());
			}
		}

		public IReadOnlyCollection<string> ReadUsers(DateTimeOffset endDate) {
			var users = _collection.AsQueryable()
				.Select(e => e.Model)
				.Where(e => (e != null))
				.Select(e => e!)
				.ToArray()
				.Select(e => e.User)
				.Distinct()
				.ToArray();
			return users;
		}

		public IReadOnlyCollection<ICommandModel> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate) {
			var dbModels = _collection.AsQueryable()
				.Select(e => e.Model)
				.Where(e => (e != null))
				.Select(e => e!)
				.ToArray();
			var models = dbModels
				.Where(e => e.Date >= startDate)
				.Where(e => e.Date <= endDate)
				.ToArray();
			return models;
		}

		public IReadOnlyCollection<ICommandModel> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, string userId) {
			var dbModels = _collection.AsQueryable()
				.Select(e => e.Model)
				.Where(e => (e != null))
				.Select(e => e!)
				.ToArray();
			var models = dbModels
				.Where(e => e.Date >= startDate)
				.Where(e => e.Date <= endDate)
				.Where(e => e.User == userId)
				.ToArray();
			return models;
		}

		public async Task SaveCommand(ICommandModel model) {
			var dbModel = new StateEventModel {
				Model = model
			};
			await _collection.InsertOneAsync(dbModel);
			_logger.LogTrace($"Command saved: {model}");
		}

		public async Task DeleteCommands(IReadOnlyCollection<ICommandModel> commands) {
			var dbModels = _collection.AsQueryable()
				.Where(e => commands.Contains(e.Model))
				.Select(e => e.Id)
				.ToArray();
			await _collection.DeleteManyAsync(m => dbModels.Contains(m.Id));
		}
	}
}