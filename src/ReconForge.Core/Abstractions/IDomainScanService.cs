using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IDomainScanService
{
    Task<DomainScanResult> ScanAsync(string? input, CancellationToken cancellationToken = default);
}
