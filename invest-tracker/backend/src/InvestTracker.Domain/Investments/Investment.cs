namespace InvestTracker.Domain.Investments;

public sealed class Investment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // ctor para o EF
    private Investment() { }

    private Investment(Guid id, Guid userId, string type, decimal amount, DateOnly date, string? description)
    {
        if (userId == Guid.Empty)
            throw new InvalidOperationException("UserId inválido ao criar Investment.");
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type é obrigatório.", nameof(type));

        Id = id;
        UserId = userId;
        Type = type;
        Amount = amount;
        Date = date;
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Investment Create(Guid userId, string type, decimal amount, DateOnly date, string? description)
        => new Investment(Guid.NewGuid(), userId, type, amount, date, description);

    public void Update(string type, decimal amount, DateOnly date, string? description)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type é obrigatório.", nameof(type));

        Type = type;
        Amount = amount;
        Date = date;
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
