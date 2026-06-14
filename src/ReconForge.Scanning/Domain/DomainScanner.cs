using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Scanning.Domain;

public sealed class DomainScanner : IDomainScanner
{
    private readonly ILogger<DomainScanner> _logger;

    public DomainScanner(ILogger<DomainScanner> logger)
    {
        _logger = logger;
    }

    public Task<ScanResult> ScanAsync(ScanRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Target))
        {
            throw new ArgumentException("A target domain is required.", nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var target = request.Target.Trim();
        var startedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation("Running placeholder domain scan for {Target}", target);

        var findings = new List<ScanFinding>
        {
            new(
                "Domain",
                "Placeholder scan completed. No network probing was performed.")
        };

        var result = new ScanResult(
            target,
            "DomainScan",
            startedAtUtc,
            DateTimeOffset.UtcNow,
            ScanStatus.Completed,
            findings);

        return Task.FromResult(result);
    }
}
