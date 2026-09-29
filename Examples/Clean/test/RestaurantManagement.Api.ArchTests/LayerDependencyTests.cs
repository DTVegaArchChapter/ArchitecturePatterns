using System.Reflection;
using NetArchTest.Rules;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class LayerDependencyTests
{
    private static readonly string[] InfrastructurePackages =
        ["Microsoft.EntityFrameworkCore", "Dapper", "Microsoft.Data", "System.Data"];

    [Fact]
    public void Domain_ShouldNotDependOnOtherLayers() =>
        AssertNoDependency(DomainAssembly, ApplicationNs, InfrastructureNs, ApiNs);

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi() =>
        AssertNoDependency(ApplicationAssembly, InfrastructureNs, ApiNs);

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi() =>
        AssertNoDependency(InfrastructureAssembly, ApiNs);

    [Fact]
    public void Domain_ShouldNotDependOnFrameworksOrPackages() =>
        AssertNoDependency(
            DomainAssembly,
            [.. InfrastructurePackages, "Microsoft.AspNetCore", "Mediator", "FluentValidation", "Microsoft.Extensions"]);

    [Fact]
    public void Application_ShouldNotDependOnInfrastructurePackages() =>
        AssertNoDependency(ApplicationAssembly, [.. InfrastructurePackages, "Microsoft.AspNetCore"]);

    [Fact]
    public void Api_ShouldNotDependOnInfrastructure_ExceptCompositionRoot() =>
        AssertNoDependency(ApiAssembly, InfrastructureNs);

    private static void AssertNoDependency(Assembly assembly, params string[] forbidden)
    {
        var types = Types.InAssembly(assembly);
        var filtered = assembly == ApiAssembly
            ? types.That().ResideInNamespaceStartingWith(ApiNs)
            : types.That().ResideInNamespaceStartingWith("RestaurantManagement");

        AssertSuccessful(filtered.ShouldNot().HaveDependencyOnAny(forbidden).GetResult());
    }
}
