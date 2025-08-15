using System.Data;
using Dapper;
using InvestTracker.Contracts.DTOs;
using Npgsql;

namespace InvestTracker.Infrastructure.Queries;

public sealed class InvestmentSqlQueries
{
    private readonly NpgsqlConnection _conn;

    public InvestmentSqlQueries(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    public async Task<IEnumerable<InvestmentListItemDto>> ListByUserAsync(Guid userId, int skip, int take)
    {
        const string sql = @"
    SELECT
        i.""Id""            AS ""Id"",
        i.""UserId""        AS ""UserId"",
        i.""Type""          AS ""Type"",
        i.""Amount""        AS ""Amount"",
        i.""Date""          AS ""Date"",
        i.""Description""   AS ""Description"",
        i.""CreatedAtUtc""  AS ""CreatedAtUtc"",
        i.""UpdatedAtUtc""  AS ""UpdatedAtUtc""
    FROM investments i
    WHERE i.""UserId"" = @userId
    ORDER BY i.""CreatedAtUtc"" DESC
    OFFSET @skip LIMIT @take;";

        return await _conn.QueryAsync<InvestmentListItemDto>(sql, new { userId, skip, take });
    }
}
