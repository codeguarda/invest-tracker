using MongoDB.Driver;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class MongoContext
{
    public IMongoCollection<DashboardReadModel> Dashboard { get; }

    public MongoContext(IMongoDatabase db)
    {
        Dashboard = db.GetCollection<DashboardReadModel>("dashboard_v2");

        var keys = Builders<DashboardReadModel>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.Month);

        var opt = new CreateIndexOptions { Unique = true, Name = "UserId_1_Month_1" };
        Dashboard.Indexes.CreateOne(new CreateIndexModel<DashboardReadModel>(keys, opt));
    }
}
