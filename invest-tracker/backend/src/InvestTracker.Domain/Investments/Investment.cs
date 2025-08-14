namespace InvestTracker.Domain.Investments;

public sealed class Investment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Investment() { }

    public static Investment Create(Guid userId, string type, decimal amount, DateOnly date, string? description)
        => new()
        {
            UserId = userId,
            Type = type,
            Amount = amount,
            Date = date,
            Description = description
        };

    public void Update(string type, decimal amount, DateOnly date, string? description)
    {
        Type = type;
        Amount = amount;
        Date = date;
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
