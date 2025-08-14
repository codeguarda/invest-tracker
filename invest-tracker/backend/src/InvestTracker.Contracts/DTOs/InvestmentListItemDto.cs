namespace InvestTracker.Contracts.DTOs;

public sealed class InvestmentListItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }          // mapeia 'date' como DateTime (00:00:00)
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
