using System.Net;

namespace ReconForge.Core.Abstractions;

public interface IDnsLookupClient
{
    Task<IReadOnlyCollection<IPAddress>> GetHostAddressesAsync(
        string host,
        CancellationToken cancellationToken = default);
}
