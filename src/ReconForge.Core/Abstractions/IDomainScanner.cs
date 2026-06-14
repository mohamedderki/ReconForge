using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IDomainScanner
{
    Task<ScanResult> ScanAsync(ScanRequest request, CancellationToken cancellationToken = default);
}
