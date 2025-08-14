using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class MongoContext
{
    // Propriedade coleção "dashboard"
    public IMongoCollection<DashboardReadModel> Dashboard { get; }

    // Compatibilizar .Dashboards
    public IMongoCollection<DashboardReadModel> Dashboards => Dashboard;

    public MongoContext(IOptions<MongoOptions> opt)
    {
        var client = new MongoClient(opt.Value.ConnectionString);
        var db = client.GetDatabase(opt.Value.Database);

        Dashboard = db.GetCollection<DashboardReadModel>("dashboard");

        // Índice composto (UserId, Month)
        var keys = Builders<DashboardReadModel>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.Month);

        Dashboard.Indexes.CreateOne(
            new CreateIndexModel<DashboardReadModel>(
                keys,
                new CreateIndexOptions { Unique = true, Name = "ix_dashboard_user_month" }
            )
        );
    }
}
