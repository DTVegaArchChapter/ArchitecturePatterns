using Dapper;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Infrastructure.Data;

namespace RestaurantManagement.Infrastructure.ReadServices;

public sealed class DapperMenuItemReadService(IDbConnectionFactory connectionFactory) : IMenuItemReadService
{
    public async Task<IReadOnlyList<MenuItemDto>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Category, Price, Description, IsAvailable
            FROM MenuItems
            WHERE IsAvailable = 1
            ORDER BY Category, Name
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<MenuItemRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows
            .Select(r => new MenuItemDto(r.Id, r.Name, r.Category, r.Price, r.Description, r.IsAvailable))
            .ToList();
    }

    private sealed class MenuItemRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}
