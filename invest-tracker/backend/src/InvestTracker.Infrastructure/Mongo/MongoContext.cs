using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class MongoContext
{
    // coleção principal (nome "dashboard")
    public IMongoCollection<DashboardReadModel> Dashboard { get; }

    // ✅ alias em plural para compatibilidade com chamadas .Dashboards
    public IMongoCollection<DashboardReadModel> Dashboards => Dashboard;

    public MongoContext(IOptions<MongoOptions> opt)
    {
        var client = new MongoClient(opt.Value.ConnectionString);
        var db = client.GetDatabase(opt.Value.Database);

        Dashboard = db.GetCollection<DashboardReadModel>("dashboard");

        // índice composto (UserId, Month) ÚNICO
        var keys = Builders<DashboardReadModel>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.Month);

        // Use o nome que já existe em muitos ambientes criados sem "Name":
        const string indexName = "UserId_1_Month_1";

        // Verifica se já existe um índice com esse nome (ou com as mesmas chaves)
        var existingIndexes = Dashboard.Indexes.List().ToList();
        var existsSameName = existingIndexes.Any(ix =>
            ix.TryGetValue("name", out var n) && n.IsString && n.AsString == indexName);

        var existsSameKeys = existingIndexes.Any(ix =>
        {
            if (!ix.TryGetValue("key", out var keyDoc) || keyDoc is not BsonDocument kd) return false;
            // Precisamos de exatamente { UserId: 1, Month: 1 }
            return kd.ElementCount == 2
                   && kd.TryGetValue("UserId", out var v1) && v1.IsInt32 && v1.AsInt32 == 1
                   && kd.TryGetValue("Month", out var v2) && v2.IsInt32 && v2.AsInt32 == 1;
        });

        // Cria apenas se não existir (por nome OU pelas mesmas chaves)
        if (!existsSameName && !existsSameKeys)
        {
            var model = new CreateIndexModel<DashboardReadModel>(
                keys,
                new CreateIndexOptions { Unique = true, Name = indexName }
            );
            Dashboard.Indexes.CreateOne(model);
        }
        // Se existir com nome diferente, deixamos como está para evitar conflito.
    }
}
