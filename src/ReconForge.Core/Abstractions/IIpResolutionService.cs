using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IIpResolutionService
{
    Task<IpResolutionResult> ResolveAsync(
        string primaryHost,
        IEnumerable<Subdomain> subdomains,
        CancellationToken cancellationToken = default);
}
