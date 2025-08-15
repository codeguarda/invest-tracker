using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvestTracker.Infrastructure.Mongo;

public sealed class DashboardReadModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid UserId { get; set; }

    // Formato "yyyy-MM" (ex.: "2025-08")
    public string Month { get; set; } = default!;

    // GUARDE como Decimal128 no Mongo
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalInvested { get; set; }

    // Valores também devem ser decimais
    public Dictionary<string, decimal> ByType { get; set; } = new();
}

