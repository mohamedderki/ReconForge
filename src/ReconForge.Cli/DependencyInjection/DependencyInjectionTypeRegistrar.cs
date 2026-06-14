using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace ReconForge.Cli.DependencyInjection;

internal sealed class DependencyInjectionTypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public DependencyInjectionTypeRegistrar(IServiceCollection services)
    {
        _services = services;
    }

    public ITypeResolver Build()
    {
        return new DependencyInjectionTypeResolver(_services.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _services.AddSingleton(service, _ => factory());
    }
}
