namespace InvestTracker.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public string Type { get; init; } = default!; // e.g., "InvestmentCreatedV1"
    public string Payload { get; init; } = default!; // JSON do evento
    public DateTime? ProcessedAtUtc { get; set; }
}
