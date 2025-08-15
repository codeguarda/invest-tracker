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
        // sanity-check: nunca aceitar UserId vazio
        if (req.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId vazio no comando de criação de investimento.");

        // cria a entidade de domínio
        var entity = Investment.Create(req.UserId, req.Type, req.Amount, req.Date, req.Description);

        // (opcional) sanity extra: se por algum motivo a factory não setar, falha cedo
        if (entity.UserId == Guid.Empty)
            throw new InvalidOperationException("A entidade Investment foi criada sem UserId.");

        Console.WriteLine($"[CreateInvestmentHandler] req.UserId={req.UserId} entity.UserId={entity.UserId}");
        await _db.Investments.AddAsync(entity, ct);

        // evento de outbox que o worker vai projetar no Mongo (usar SEMPRE o UserId do request)
        var payload = new InvestmentCreatedV1(
            Id: entity.Id,
            UserId: req.UserId, // ← chave: usa o ID vindo do token
            Type: entity.Type,
            Amount: entity.Amount,
            Date: entity.Date.ToString("yyyy-MM-dd")
        );

        var evt = new OutboxMessage
        {
            Type = "InvestmentCreatedV1",
            Payload = JsonSerializer.Serialize(payload)
        };

        await _db.OutboxMessages.AddAsync(evt, ct);

        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    // model forte p/ manter consistência do payload
    private sealed record InvestmentCreatedV1(Guid Id, Guid UserId, string Type, decimal Amount, string Date);
}
