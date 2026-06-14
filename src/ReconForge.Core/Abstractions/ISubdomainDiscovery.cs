namespace ReconForge.Core.Abstractions;

public interface ISubdomainDiscovery
{
    Task<IReadOnlyCollection<string>> DiscoverAsync(string domain, CancellationToken cancellationToken = default);
}
