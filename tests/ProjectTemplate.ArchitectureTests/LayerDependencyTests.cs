namespace ProjectTemplate.ArchitectureTests;

public class LayerDependencyTests
{
    private const string DomainNamespace = "ProjectTemplate.Domain";
    private const string ApplicationNamespace = "ProjectTemplate.Application";
    private const string InfrastructureNamespace = "ProjectTemplate.Infrastructure";
    private const string ApiNamespace = "ProjectTemplate.WebHost";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Application()
    {
        Types.InAssembly(typeof(ProjectTemplate.Domain.Abstractions.IRepository<>).Assembly)
             .Should().NotHaveDependencyOn(ApplicationNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Infrastructure()
    {
        Types.InAssembly(typeof(ProjectTemplate.Domain.Abstractions.IRepository<>).Assembly)
             .Should().NotHaveDependencyOn(InfrastructureNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Infrastructure()
    {
        Types.InAssembly(typeof(ProjectTemplate.Application.DependencyInjection).Assembly)
             .Should().NotHaveDependencyOn(InfrastructureNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Api()
    {
        Types.InAssembly(typeof(ProjectTemplate.Application.DependencyInjection).Assembly)
             .Should().NotHaveDependencyOn(ApiNamespace)
             .GetResult().IsSuccessful.Should().BeTrue();
    }
}
