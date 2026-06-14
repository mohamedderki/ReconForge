using System.Net;
using ReconForge.Core.Abstractions;

namespace ReconForge.Tests.TestDoubles;

public sealed class FakeDnsLookupClient : IDnsLookupClient
{
    private readonly Dictionary<string, IReadOnlyCollection<IPAddress>> _addresses = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _failures = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _requestedHosts = [];

    public IReadOnlyList<string> RequestedHosts => _requestedHosts;

    public void SetAddress(string host, params IPAddress[] addresses)
    {
        _addresses[host] = addresses;
    }

    public void SetFailure(string host)
    {
        _failures.Add(host);
    }

    public Task<IReadOnlyCollection<IPAddress>> GetHostAddressesAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        _requestedHosts.Add(host);

        if (_failures.Contains(host))
        {
            throw new InvalidOperationException($"DNS lookup failed for {host}.");
        }

        return Task.FromResult(
            _addresses.TryGetValue(host, out var addresses)
                ? addresses
                : Array.Empty<IPAddress>());
    }
}
