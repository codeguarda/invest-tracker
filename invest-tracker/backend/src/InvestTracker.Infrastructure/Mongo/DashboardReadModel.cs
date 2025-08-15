using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace InvestTracker.Infrastructure.Mongo;

// Ignora qualquer campo extra que exista no documento (seguro para _id e afins)
[BsonIgnoreExtraElements]
public sealed class DashboardReadModel
{
    // Mapeia o _id do Mongo
    [BsonId]
    public ObjectId Id { get; set; }

    // Guarde/Leia Guid como string no Mongo (combina com o que o worker escreve)
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    // Formato "yyyy-MM"
    public string Month { get; set; } = default!;

    // Números da coleção são double (foi isso que evitou os erros de $inc)
    public double TotalInvested { get; set; }

    // Dicionário com totais por tipo, também double
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<string, double> ByType { get; set; } = new();
}
