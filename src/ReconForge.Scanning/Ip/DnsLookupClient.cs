using System.Net;
using ReconForge.Core.Abstractions;

namespace ReconForge.Scanning.Ip;

public sealed class DnsLookupClient : IDnsLookupClient
{
    public async Task<IReadOnlyCollection<IPAddress>> GetHostAddressesAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        return addresses;
    }
}
