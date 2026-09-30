using Dapper;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Data;

namespace RestaurantManagement.Infrastructure.ReadServices;

public sealed class DapperTableReadService(IDbConnectionFactory connectionFactory) : ITableReadService
{
    public async Task<IReadOnlyList<TableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, TableNumber, Capacity, Status, ReservedAt
            FROM Tables
            ORDER BY TableNumber
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<TableRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows
            .Select(r => new TableDto(r.Id, r.TableNumber, r.Capacity, ((TableStatus)r.Status).ToString(), r.ReservedAt))
            .ToList();
    }

    private sealed class TableRow
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public int Status { get; set; }
        public DateTime? ReservedAt { get; set; }
    }
}
