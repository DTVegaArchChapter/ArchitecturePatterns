using NetArchTest.Rules;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class ApiRulesTests
{
    [Fact]
    public void Endpoints_ShouldEndWithEndpoints()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(ApiNs + ".Endpoints")
            .Should().HaveNameEndingWith("Endpoints")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Endpoints_ShouldNotDependOnDomainEntities()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(ApiNs + ".Endpoints")
            .ShouldNot().HaveDependencyOn(DomainNs)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Endpoints_ShouldNotDependOnRepositoriesOrDbContext()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(ApiNs + ".Endpoints")
            .ShouldNot().HaveDependencyOnAny(
                InfrastructureNs,
                ApplicationNs + ".Common.Interfaces",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Contracts_ShouldResideInApiAndEndWithRequest()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespaceStartingWith(ApiNs + ".Contracts")
            .Should().HaveNameEndingWith("Request").Or().HaveNameEndingWith("Response")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Contracts_ShouldNotDependOnDomain()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespaceStartingWith(ApiNs + ".Contracts")
            .ShouldNot().HaveDependencyOn(DomainNs)
            .GetResult();

        AssertSuccessful(result);
    }
}
