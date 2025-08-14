using System.Text.Json;
using MediatR;
using InvestTracker.Domain.Investments;
using InvestTracker.Infrastructure.Outbox;
using InvestTracker.Infrastructure.Persistence;

namespace InvestTracker.Application.Investments;

public sealed class CreateInvestmentHandler : IRequestHandler<CreateInvestmentCommand, Guid>
{
    private readonly AppWriteDbContext _db;
    public CreateInvestmentHandler(AppWriteDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateInvestmentCommand req, CancellationToken ct)
    {
        var entity = Investment.Create(req.UserId, req.Type, req.Amount, req.Date, req.Description);
        await _db.Investments.AddAsync(entity, ct);

        var evt = new OutboxMessage
        {
            Type = "InvestmentCreatedV1",
            Payload = JsonSerializer.Serialize(new {
                entity.Id, entity.UserId, entity.Type, entity.Amount, Date = entity.Date.ToString("yyyy-MM-dd"), entity.CreatedAtUtc
            })
        };
        await _db.OutboxMessages.AddAsync(evt, ct);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
