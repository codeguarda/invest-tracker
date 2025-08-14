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
        return docs.Select(d => new DashboardItemDto
        {
            Month = d.Month,
            Total = d.TotalInvested,
            ByType = d.ByType
        }).ToList();
    }
}
