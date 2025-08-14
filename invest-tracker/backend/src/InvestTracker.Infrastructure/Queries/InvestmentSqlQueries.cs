using Dapper;
using Npgsql;
using InvestTracker.Contracts.DTOs;

namespace InvestTracker.Infrastructure.Queries;

public sealed class InvestmentSqlQueries
{
    private readonly NpgsqlConnection _conn;
    public InvestmentSqlQueries(NpgsqlConnection conn) => _conn = conn;

    public async Task<IEnumerable<InvestmentListItemDto>> ListByUserAsync(Guid userId, int skip, int take)
    {
        var rows = await _conn.QueryAsync<InvestmentListItemDto>(@"
            SELECT
                i.""Id""             AS ""Id"",
                i.""Type""           AS ""Type"",
                i.""Amount""         AS ""Amount"",
                i.""Date""           AS ""Date"",
                i.""Description""    AS ""Description"",
                i.""CreatedAtUtc""   AS ""CreatedAtUtc""
            FROM investments AS i
            WHERE i.""UserId"" = @userId
            ORDER BY i.""Date"" DESC, i.""CreatedAtUtc"" DESC
            OFFSET @skip LIMIT @take;
        ", new { userId, skip, take });
        return rows;
    }
}
