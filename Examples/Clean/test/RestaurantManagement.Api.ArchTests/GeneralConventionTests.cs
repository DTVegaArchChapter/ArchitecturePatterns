using System.Reflection;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class GeneralConventionTests
{
    [Fact]
    public void Interfaces_ShouldStartWithI() =>
        AssertNoViolations(
            All.SelectMany(ProjectTypes)
                .Where(t => t.IsInterface && !(t.Name.Length > 1 && t.Name[0] == 'I' && char.IsUpper(t.Name[1])))
                .Select(t => t.FullName!));

    [Fact]
    public void Types_ShouldNotHaveStaticMutableState() =>
        AssertNoViolations(
            All.SelectMany(ProjectTypes)
                .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .Where(f => !f.IsInitOnly && !f.IsLiteral
                    && !Attribute.IsDefined(f, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
                .Select(f => $"{f.DeclaringType!.FullName}.{f.Name}"));

    [Fact]
    public void Namespaces_ShouldStartWithAssemblyName() =>
        AssertNoViolations(
            All.SelectMany(a => ProjectTypes(a).Select(t => (Assembly: a, Type: t)))
                .Where(x => !x.Type.Namespace!.StartsWith(x.Assembly.GetName().Name!, StringComparison.Ordinal))
                .Select(x => x.Type.FullName!));

    [Fact]
    public void ConcreteClasses_ShouldBeSealedOrAbstractOrStaticOrEntities() =>
        AssertNoViolations(
            new[] { ApplicationAssembly, InfrastructureAssembly }
                .SelectMany(ProjectTypes)
                .Where(t => t is { IsClass: true, IsSealed: false, IsAbstract: false } && !typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(t) && !t.Name.EndsWith("Dto", StringComparison.Ordinal))
                .Select(t => t.FullName!));
}
