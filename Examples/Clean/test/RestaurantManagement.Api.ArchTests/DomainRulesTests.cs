using System.Reflection;
using System.Runtime.CompilerServices;
using NetArchTest.Rules;
using RestaurantManagement.Domain.Common;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class DomainRulesTests
{
    private static IEnumerable<Type> Entities =>
        DomainAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)));

    [Fact]
    public void EntitiesInEntitiesNamespace_ShouldInheritBaseEntity()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace(DomainNs + ".Entities")
            .And().AreClasses()
            .And().DoNotHaveNameEndingWith("Status")
            .Should().Inherit(typeof(BaseEntity))
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void AggregateRoots_ShouldBeEntities()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ImplementInterface(typeof(IAggregateRoot))
            .Should().Inherit(typeof(BaseEntity))
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Entities_ShouldNotHavePublicSetters() =>
        AssertNoViolations(
            Entities.SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(p => p.SetMethod is { IsPublic: true }
                    && !p.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
                .Select(p => $"{p.DeclaringType!.Name}.{p.Name}"));

    [Fact]
    public void Entities_ShouldNotBeSealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().Inherit(typeof(BaseEntity))
            .ShouldNot().BeSealed()
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void DomainTypes_ShouldNotUseApplicationOrApiNamingSuffixes()
    {
        string[] suffixes = ["Dto", "Request", "Response", "Command", "Query", "Handler"];

        AssertNoViolations(
            ProjectTypes(DomainAssembly)
                .Where(t => suffixes.Any(s => t.Name.EndsWith(s, StringComparison.Ordinal)))
                .Select(t => t.Name));
    }
}
