using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class MongoContext
{
    public IMongoCollection<DashboardReadModel> Dashboards { get; }

    public MongoContext(IOptions<MongoOptions> opt)
    {
        var client = new MongoClient(opt.Value.ConnectionString);
        var db = client.GetDatabase(opt.Value.Database);

        Dashboards = db.GetCollection<DashboardReadModel>("dashboard");

        var keys = Builders<DashboardReadModel>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.Month);

        Dashboards.Indexes.CreateOne(
            new CreateIndexModel<DashboardReadModel>(keys, new CreateIndexOptions { Unique = true })
        );
    }
}
