// ArchUnitNET layering enforcement (per Constitution Principle II).
// Domain has no infra refs; Application has no infra refs; Infrastructure
// depends only on Domain + Application.

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClauseLens.Architecture.Tests;

public class LayeringTests
{
    private static readonly Architecture Arch = new ArchLoader()
        .LoadAssemblies(
            typeof(ClauseLens.Domain.Common.Entity).Assembly,
            typeof(ClauseLens.Application.Abstractions.ICommand<>).Assembly,
            typeof(ClauseLens.Infrastructure.InfrastructureRegistration).Assembly
        ).Build();

    [Fact]
    public void Domain_should_not_depend_on_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(ClauseLens.Domain.Common.Entity).Assembly)
            .ShouldNot().DependOnAny(
                Types().That().ResideInAssembly(typeof(ClauseLens.Infrastructure.InfrastructureRegistration).Assembly))
            .Check(Arch);
    }

    [Fact]
    public void Application_should_not_depend_on_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(ClauseLens.Application.Abstractions.ICommand<>).Assembly)
            .ShouldNot().DependOnAny(
                Types().That().ResideInAssembly(typeof(ClauseLens.Infrastructure.InfrastructureRegistration).Assembly))
            .Check(Arch);
    }
}
