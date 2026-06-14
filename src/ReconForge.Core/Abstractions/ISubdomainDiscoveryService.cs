using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface ISubdomainDiscoveryService
{
    Task<SubdomainDiscoveryResult> DiscoverAsync(
        string rootDomain,
        IEnumerable<string>? customPrefixes = null,
        CancellationToken cancellationToken = default);
}
