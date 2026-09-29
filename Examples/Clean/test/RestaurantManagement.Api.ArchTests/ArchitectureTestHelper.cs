using System.Reflection;
using NetArchTest.Rules;
using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Api.ArchTests;

internal static class ArchitectureTestHelper
{
    public const string DomainNs = "RestaurantManagement.Domain";
    public const string ApplicationNs = "RestaurantManagement.Application";
    public const string InfrastructureNs = "RestaurantManagement.Infrastructure";
    public const string ApiNs = "RestaurantManagement.Api";

    public static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    public static readonly Assembly ApplicationAssembly = Assembly.Load(ApplicationNs);
    public static readonly Assembly InfrastructureAssembly = Assembly.Load(InfrastructureNs);
    public static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    public static readonly Assembly[] All = [DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly];

    public static void AssertSuccessful(TestResult result) =>
        Assert.True(
            result.IsSuccessful,
            "Violating types: " + string.Join(", ", result.FailingTypeNames ?? []));

    public static void AssertNoViolations(IEnumerable<string> violations)
    {
        var list = violations.ToList();
        Assert.True(list.Count == 0, "Violations: " + string.Join(", ", list));
    }

    public static bool ImplementsOpenGeneric(this Type type, Type openGeneric) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric);

    public static IEnumerable<Type> ProjectTypes(Assembly assembly) =>
        assembly.GetTypes().Where(t =>
            t.Namespace is not null
            && t.Namespace.StartsWith("RestaurantManagement", StringComparison.Ordinal)
            && !Attribute.IsDefined(t, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)));
}
