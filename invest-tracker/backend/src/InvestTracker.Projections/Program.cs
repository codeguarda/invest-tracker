using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using InvestTracker.Infrastructure.Mongo;
using InvestTracker.Infrastructure.Outbox;
using InvestTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // Postgres (EF Core)
        services.AddDbContext<AppWriteDbContext>(o =>
            o.UseNpgsql(cfg.GetConnectionString("Postgres")));

        // MongoDB (IMongoClient + IMongoDatabase + MongoContext)
        services.AddSingleton<IMongoClient>(_ =>
        {
            var cs = cfg["Mongo:ConnectionString"] ?? "mongodb://localhost:27017";
            return new MongoClient(cs);
        });

        services.AddSingleton(sp =>
        {
            var dbName = cfg["Mongo:Database"] ?? "investread";
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(dbName);
        });

        services.AddSingleton<MongoContext>();

        // Worker + Logging
        services.AddHostedService<OutboxProjectionWorker>();
        services.AddLogging(b => b.AddConsole());
    })
    .Build();

await host.RunAsync();

public sealed class OutboxProjectionWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxProjectionWorker> _log;

    public OutboxProjectionWorker(IServiceProvider sp, ILogger<OutboxProjectionWorker> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("Projection worker started");
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppWriteDbContext>();
                var mongo = scope.ServiceProvider.GetRequiredService<MongoContext>();

                var messages = await db.OutboxMessages
                    .Where(x => x.ProcessedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(100)
                    .ToListAsync(ct);

                foreach (var msg in messages)
                {
                    if (msg.Type == "InvestmentCreatedV1")
                    {
                        var e = JsonSerializer.Deserialize<InvestmentCreatedV1>(msg.Payload)!;

                        var month = e.Date[..7]; // yyyy-MM

                        var filter = Builders<DashboardReadModel>.Filter.And(
                            Builders<DashboardReadModel>.Filter.Eq(x => x.UserId, e.UserId),
                            Builders<DashboardReadModel>.Filter.Eq(x => x.Month, month)
                        );

                        var update = Builders<DashboardReadModel>.Update
                            .Inc(x => x.TotalInvested, e.Amount)
                            .Inc($"ByType.{e.Type}", e.Amount)
                            .SetOnInsert(x => x.UserId, e.UserId)
                            .SetOnInsert(x => x.Month, month);

                        await mongo.Dashboards.UpdateOneAsync(
                            filter,
                            update,
                            new UpdateOptions { IsUpsert = true },
                            ct
                        );
                    }

                    msg.ProcessedAtUtc = DateTime.UtcNow;
                }

                if (messages.Count > 0)
                    await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Projection loop error");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }
    }

    private sealed record InvestmentCreatedV1(Guid Id, Guid UserId, string Type, [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Amount, string Date);
}
