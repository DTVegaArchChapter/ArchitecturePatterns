using System.Net.Http.Json;
using System.Text.Json;
using RestaurantManagement.Api.Contracts.Orders;

namespace RestaurantManagement.Api.FunctionalTests;

internal static class TestDataHelpers
{
    public static async Task<int> CreateOrderAsync(this HttpClient client, int tableId = 1, int menuItemId = 1, int quantity = 1)
    {
        var request = new CreateOrderRequest(tableId, [new OrderItemRequest(menuItemId, quantity, null)], null);

        var response = await client.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();

        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        return order.GetProperty("id").GetInt32();
    }
}
