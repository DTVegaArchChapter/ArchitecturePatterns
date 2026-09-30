using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RestaurantManagement.Api.FunctionalTests;

public sealed class MenuItemEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMenuItems_ReturnsSeededItems()
    {
        var response = await _client.GetAsync("/api/menuitems");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(8, items.GetArrayLength());
    }
}
