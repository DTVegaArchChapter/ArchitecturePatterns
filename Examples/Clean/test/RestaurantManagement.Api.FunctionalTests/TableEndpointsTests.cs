using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RestaurantManagement.Api.FunctionalTests;

public sealed class TableEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private Task<HttpResponseMessage> UpdateStatusAsync(int tableId, string newStatus) =>
        _client.PutAsJsonAsync($"/api/tables/{tableId}/status", new { newStatus });

    [Fact]
    public async Task GetTables_ReturnsSeededTables()
    {
        var response = await _client.GetAsync("/api/tables");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tables = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, tables.GetArrayLength());
    }

    [Fact]
    public async Task UpdateTableStatus_AvailableToOccupied_ReturnsUpdatedTable()
    {
        var response = await UpdateStatusAsync(1, "Occupied");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var table = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, table.GetProperty("id").GetInt32());
        Assert.Equal("Occupied", table.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UpdateTableStatus_AvailableToReserved_SetsReservedAt()
    {
        var response = await UpdateStatusAsync(2, "Reserved");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var table = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Reserved", table.GetProperty("status").GetString());
        Assert.NotEqual(JsonValueKind.Null, table.GetProperty("reservedAt").ValueKind);
    }

    [Fact]
    public async Task UpdateTableStatus_IsCaseInsensitive()
    {
        var response = await UpdateStatusAsync(3, "outofservice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var table = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("OutOfService", table.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UpdateTableStatus_AlreadyAvailable_ReturnsConflict()
    {
        var response = await UpdateStatusAsync(4, "Available");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTableStatus_ReserveOccupiedTable_ReturnsConflict()
    {
        Assert.Equal(HttpStatusCode.OK, (await UpdateStatusAsync(5, "Occupied")).StatusCode);

        var response = await UpdateStatusAsync(5, "Reserved");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTableStatus_InvalidStatus_ReturnsBadRequest()
    {
        var response = await UpdateStatusAsync(1, "NotAStatus");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTableStatus_NonPositiveTableId_ReturnsBadRequest()
    {
        var response = await UpdateStatusAsync(0, "Occupied");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTableStatus_UnknownTable_ReturnsNotFound()
    {
        var response = await UpdateStatusAsync(999, "Occupied");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
