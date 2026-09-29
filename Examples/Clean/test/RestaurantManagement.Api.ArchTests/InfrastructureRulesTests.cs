using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class InfrastructureRulesTests
{
    [Fact]
    public void Repositories_ShouldEndWithRepositoryAndImplementApplicationInterface() =>
        AssertNoViolations(
            ProjectTypes(InfrastructureAssembly)
                .Where(t => t is { IsClass: true, Namespace: InfrastructureNs + ".Repositories" })
                .Where(t => !t.GetInterfaces().Any(i => i.Assembly == ApplicationAssembly || i.Assembly == DomainAssembly)
                    || !(t.Name.EndsWith("Repository", StringComparison.Ordinal) || t.Name == "UnitOfWork"))
                .Select(t => t.Name));

    [Fact]
    public void ReadServices_ShouldEndWithReadServiceAndImplementApplicationInterface() =>
        AssertNoViolations(
            ProjectTypes(InfrastructureAssembly)
                .Where(t => t is { IsClass: true, Namespace: InfrastructureNs + ".ReadServices" } && !t.IsNested)
                .Where(t => t.GetInterfaces().All(i => i.Assembly != ApplicationAssembly) || !t.Name.EndsWith("ReadService", StringComparison.Ordinal))
                .Select(t => t.Name));

    [Fact]
    public void EntityConfigurations_ShouldEndWithConfiguration() =>
        AssertNoViolations(
            ProjectTypes(InfrastructureAssembly)
                .Where(t => t.ImplementsOpenGeneric(typeof(IEntityTypeConfiguration<>))
                    && !t.Name.EndsWith("Configuration", StringComparison.Ordinal))
                .Select(t => t.Name));

    [Fact]
    public void RepositoriesAndReadServices_ShouldBeSealed()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespace(InfrastructureNs + ".Repositories")
            .Or().ResideInNamespace(InfrastructureNs + ".ReadServices")
            .And().AreClasses()
            .Should().BeSealed()
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Repositories_ShouldNotDependOnReadServices()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespace(InfrastructureNs + ".Repositories")
            .ShouldNot().HaveDependencyOn(InfrastructureNs + ".ReadServices")
            .GetResult();

        AssertSuccessful(result);
    }
}
