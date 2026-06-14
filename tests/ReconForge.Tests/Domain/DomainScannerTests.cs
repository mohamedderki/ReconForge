using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Scanning.Domain;

namespace ReconForge.Tests.Domain;

public sealed class DomainScannerTests
{
    [Fact]
    public async Task ScanAsync_ReturnsCompletedPlaceholderResult()
    {
        var scanner = new DomainScanner(NullLogger<DomainScanner>.Instance);

        var result = await scanner.ScanAsync(new ScanRequest("example.com"));

        Assert.Equal("example.com", result.Target);
        Assert.Equal("DomainScan", result.ScanType);
        Assert.Equal(ScanStatus.Completed, result.Status);
        Assert.NotEmpty(result.Findings);
    }
}
