using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Infrastructure.Data;
using RestaurantManagement.Infrastructure.ReadServices;
using RestaurantManagement.Infrastructure.Repositories;

namespace RestaurantManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
        services.AddScoped<IOrderReadService, DapperOrderReadService>();
        services.AddScoped<ITableReadService, DapperTableReadService>();
        services.AddScoped<IMenuItemReadService, DapperMenuItemReadService>();

        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
