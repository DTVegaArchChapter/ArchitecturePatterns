using RestaurantManagement.Api.Endpoints;
using RestaurantManagement.Application;
using RestaurantManagement.Infrastructure;
using RestaurantManagement.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("RestaurantDb") ?? "Data Source=restaurant.db");

builder.Services.AddApplication();

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
app.MapMenuItemEndpoints();
app.MapOrderEndpoints();
app.MapTableEndpoints();

await app.RunAsync().ConfigureAwait(false);

public partial class Program { }
