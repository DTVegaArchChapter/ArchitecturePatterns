using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.MenuItems.GetMenuItems;
using RestaurantManagement.Application.Orders.CreateOrder;
using RestaurantManagement.Application.Orders.GetKitchenOrders;
using RestaurantManagement.Application.Orders.UpdateOrderStatus;
using RestaurantManagement.Application.Tables.GetAllTables;
using RestaurantManagement.Application.Tables.UpdateTableStatus;
using RestaurantManagement.Infrastructure.Data;
using RestaurantManagement.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database context - using In-Memory for demo
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseInMemoryDatabase("RestaurantDb"));

// Repository registration (Infrastructure layer implements Application interfaces)
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// FluentValidation — validators are in the Application assembly
builder.Services.AddValidatorsFromAssembly(
    typeof(CreateOrderRequestValidator).Assembly,
    ServiceLifetime.Scoped);

// Use Cases (Application Business Rules) — scoped, injected directly into controllers
builder.Services.AddScoped<GetAllTablesUseCase>();
builder.Services.AddScoped<UpdateTableStatusUseCase>();
builder.Services.AddScoped<GetMenuItemsUseCase>();
builder.Services.AddScoped<CreateOrderUseCase>();
builder.Services.AddScoped<UpdateOrderStatusUseCase>();
builder.Services.AddScoped<GetKitchenOrdersUseCase>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

public partial class Program { }
