using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class DashboardReadModel
{
    [BsonId] public ObjectId Id { get; set; }
    public Guid UserId { get; set; }
    public string Month { get; set; } = default!; // yyyy-MM
    public decimal TotalInvested { get; set; }
    public Dictionary<string, decimal> ByType { get; set; } = new();
}
