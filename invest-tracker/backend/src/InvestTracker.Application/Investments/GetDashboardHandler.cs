using System.Linq;                  // ⬅️ ADICIONE
using InvestTracker.Infrastructure.Mongo;
using MediatR;
using MongoDB.Driver;

namespace InvestTracker.Application.Investments;

public sealed class GetDashboardHandler : IRequestHandler<GetDashboardQuery, IReadOnlyList<DashboardItemDto>>
{
    private readonly MongoContext _mongo;
    public GetDashboardHandler(MongoContext mongo) => _mongo = mongo;

    public async Task<IReadOnlyList<DashboardItemDto>> Handle(GetDashboardQuery req, CancellationToken ct)
    {
        var filter = Builders<DashboardReadModel>.Filter.Eq(x => x.UserId, req.UserId);
        var and = new List<FilterDefinition<DashboardReadModel>> { filter };

        if (req.From is not null) and.Add(Builders<DashboardReadModel>.Filter.Gte(x => x.Month, req.From.Value.ToString("yyyy-MM")));
        if (req.To   is not null) and.Add(Builders<DashboardReadModel>.Filter.Lte(x => x.Month, req.To.Value.ToString("yyyy-MM")));

        var final = Builders<DashboardReadModel>.Filter.And(and);
        var docs = await _mongo.Dashboard.Find(final).SortBy(x => x.Month).ToListAsync(ct);

        // ⬇️ CONVERSÕES: double -> decimal e Dictionary<string,double> -> Dictionary<string,decimal>
        var result = docs.Select(d => new DashboardItemDto
        {
            Month = d.Month,
            Total = (decimal)d.TotalInvested,
            ByType = (d.ByType ?? new Dictionary<string, double>())
                        .ToDictionary(kv => kv.Key, kv => (decimal)kv.Value)
        }).ToList();

        return result;
    }
}
