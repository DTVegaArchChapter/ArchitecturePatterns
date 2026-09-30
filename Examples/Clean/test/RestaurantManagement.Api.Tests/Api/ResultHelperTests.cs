using Microsoft.AspNetCore.Http;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Application.Common;

namespace RestaurantManagement.Api.Tests.Api;

public class ResultHelperTests
{
    [Fact]
    public void Success_ReturnsOk()
    {
        var result = Result<int>.Success(5).ToApiResult();

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, status.StatusCode);
    }

    [Fact]
    public void Success_WithOnSuccess_UsesCustomResult()
    {
        var result = Result<int>.Success(5).ToApiResult(id => Results.Created($"/items/{id}", id));

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status201Created, status.StatusCode);
    }

    [Theory]
    [InlineData(ResultType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ResultType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ResultType.Failure, StatusCodes.Status400BadRequest)]
    public void Failure_MapsResultTypeToStatusCode(ResultType type, int expected)
    {
        var result = Result<int>.Failure("boom", type).ToApiResult();

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(expected, status.StatusCode);
    }
}
