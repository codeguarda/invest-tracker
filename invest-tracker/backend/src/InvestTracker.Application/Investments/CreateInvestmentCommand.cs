using MediatR;

namespace InvestTracker.Application.Investments;

public sealed record CreateInvestmentCommand(
    Guid UserId, string Type, decimal Amount, DateOnly Date, string? Description
) : IRequest<Guid>;
