using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Mediator;
using NetArchTest.Rules;
using static RestaurantManagement.Api.ArchTests.ArchitectureTestHelper;

namespace RestaurantManagement.Api.ArchTests;

public class ApplicationRulesTests
{
    private static readonly Type[] RequestInterfaces = [typeof(ICommand<>), typeof(IQuery<>)];

    private static IEnumerable<Type> Requests =>
        ProjectTypes(ApplicationAssembly).Where(t => t.IsClass && RequestInterfaces.Any(t.ImplementsOpenGeneric));

    private static IEnumerable<Type> Commands =>
        Requests.Where(t => t.ImplementsOpenGeneric(typeof(ICommand<>)));

    private static IEnumerable<Type> Queries =>
        Requests.Where(t => t.ImplementsOpenGeneric(typeof(IQuery<>)));

    private static IEnumerable<Type> Handlers =>
        ProjectTypes(ApplicationAssembly).Where(t => t.IsClass
            && (t.ImplementsOpenGeneric(typeof(ICommandHandler<,>)) || t.ImplementsOpenGeneric(typeof(IQueryHandler<,>))));

    [Fact]
    public void Commands_ShouldEndWithCommand() =>
        AssertNoViolations(Commands.Where(t => !t.Name.EndsWith("Command", StringComparison.Ordinal)).Select(t => t.Name));

    [Fact]
    public void Queries_ShouldEndWithQuery() =>
        AssertNoViolations(Queries.Where(t => !t.Name.EndsWith("Query", StringComparison.Ordinal)).Select(t => t.Name));

    [Fact]
    public void RequestsAndHandlers_ShouldResideInFeatureNamespaces() =>
        AssertNoViolations(
            Requests.Concat(Handlers)
                .Where(t => t.Namespace!.StartsWith(ApplicationNs + ".Common", StringComparison.Ordinal)
                    || t.Namespace == ApplicationNs)
                .Select(t => t.Name));

    [Fact]
    public void Handlers_ShouldEndWithHandlerAndBeSealed() =>
        AssertNoViolations(
            Handlers.Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal) || !t.IsSealed)
                .Select(t => t.Name));

    [Fact]
    public void EveryCommandAndQuery_ShouldHaveExactlyOneHandler()
    {
        var handlerTypes = Handlers.ToList();

        AssertNoViolations(
            Requests.Where(r => handlerTypes.Count(h => h.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericArguments()[0] == r)) != 1)
                .Select(r => r.Name));
    }

    [Fact]
    public void Handlers_ShouldNotDependOnOtherHandlers()
    {
        var handlerNames = Handlers.Select(t => t.FullName!).ToArray();

        var violations = new List<string>();
        foreach (var handler in Handlers)
        {
            var others = handlerNames.Where(n => n != handler.FullName).ToArray();
            if (others.Length == 0)
            {
                continue;
            }

            var result = Types.InAssembly(ApplicationAssembly)
                .That().HaveName(handler.Name)
                .ShouldNot().HaveDependencyOnAny(others)
                .GetResult();

            violations.AddRange(result.FailingTypeNames ?? []);
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void Validators_ShouldEndWithValidator() =>
        AssertNoViolations(
            ProjectTypes(ApplicationAssembly)
                .Where(t => t is { IsClass: true, BaseType: { IsGenericType: true } b }
                            && b.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                .Where(t => !t.Name.EndsWith("Validator", StringComparison.Ordinal))
                .Select(t => t.Name));

    [Fact]
    public void RepositoryAndServiceInterfaces_ShouldResideInApplicationCommonInterfaces()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().AreInterfaces()
            .And().HaveNameEndingWith("Repository").Or().HaveNameEndingWith("Service").Or().HaveNameEndingWith("UnitOfWork")
            .Should().ResideInNamespace(ApplicationNs + ".Common.Interfaces")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void CommandsAndQueries_ShouldBeImmutable() =>
        AssertNoViolations(
            Requests.SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(p => p.SetMethod is { IsPublic: true }
                    && !p.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
                .Select(p => $"{p.DeclaringType!.Name}.{p.Name}"));
}
