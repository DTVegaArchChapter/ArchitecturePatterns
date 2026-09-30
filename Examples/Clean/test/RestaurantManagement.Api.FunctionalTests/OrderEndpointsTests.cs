using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RestaurantManagement.Api.Contracts.Orders;

namespace RestaurantManagement.Api.FunctionalTests;

public sealed class OrderEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        var request = new CreateOrderRequest(3, [new OrderItemRequest(1, 2, null)], "no onions");

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(order.GetProperty("id").GetInt32() > 0);
    }

    [Fact]
    public async Task CreateOrder_UnknownTable_ReturnsNotFound()
    {
        var request = new CreateOrderRequest(9999, [new OrderItemRequest(1, 1, null)], null);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_NoItems_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest(1, [], null);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_UnknownOrder_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/orders/9999/status",
            new UpdateOrderStatusRequest("InPreparation"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_PendingToInPreparation_ReturnsUpdatedOrder()
    {
        var orderId = await _client.CreateOrderAsync(tableId: 4);

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/status",
            new UpdateOrderStatusRequest("InPreparation"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("InPreparation", order.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidStatus_ReturnsBadRequest()
    {
        var orderId = await _client.CreateOrderAsync(tableId: 5);

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/status",
            new UpdateOrderStatusRequest("NotAStatus"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetKitchenOrders_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/orders/kitchen");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetKitchenOrders_IncludesPendingOrder()
    {
        var orderId = await _client.CreateOrderAsync(tableId: 2, menuItemId: 3, quantity: 2);

        var orders = await _client.GetFromJsonAsync<JsonElement>("/api/orders/kitchen");

        Assert.Contains(orders.EnumerateArray(), o => o.GetProperty("id").GetInt32() == orderId);
    }
}
