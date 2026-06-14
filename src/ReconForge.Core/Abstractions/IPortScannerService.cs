using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IPortScannerService
{
    Task<IReadOnlyList<PortScanResult>> ScanAsync(
        IEnumerable<IpAddress> resolvedIpAddresses,
        IEnumerable<int>? ports = null,
        int timeoutMs = 1000,
        CancellationToken cancellationToken = default);
}
