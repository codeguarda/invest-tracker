namespace InvestTracker.Application.Investments;

public sealed class InvestmentListItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Date { get; set; } = default!; // yyyy-MM-dd
    public string? Description { get; set; }
}

public sealed class DashboardItemDto
{
    public string Month { get; set; } = default!; // yyyy-MM
    public decimal Total { get; set; }
    public Dictionary<string, decimal> ByType { get; set; } = new();
}
