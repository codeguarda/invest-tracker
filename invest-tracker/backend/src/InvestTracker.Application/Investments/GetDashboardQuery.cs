using MediatR;

namespace InvestTracker.Application.Investments;

public sealed record GetDashboardQuery(Guid UserId, DateOnly? From, DateOnly? To)
    : IRequest<IReadOnlyList<DashboardItemDto>>;
