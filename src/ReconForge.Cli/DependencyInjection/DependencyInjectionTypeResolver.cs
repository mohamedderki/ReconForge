using Spectre.Console.Cli;

namespace ReconForge.Cli.DependencyInjection;

internal sealed class DependencyInjectionTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInjectionTypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? Resolve(Type? type)
    {
        if (type is null)
        {
            return null;
        }

        return _serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
