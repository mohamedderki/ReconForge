using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Scanning.Domain;

public sealed class DomainScanService : IDomainScanService
{
    private readonly IDomainValidator _validator;
    private readonly ISubdomainDiscoveryService _subdomainDiscoveryService;
    private readonly IIpResolutionService _ipResolutionService;
    private readonly IPortScannerService _portScannerService;
    private readonly ILogger<DomainScanService> _logger;

    public DomainScanService(
        IDomainValidator validator,
        ISubdomainDiscoveryService subdomainDiscoveryService,
        IIpResolutionService ipResolutionService,
        IPortScannerService portScannerService,
        ILogger<DomainScanService> logger)
    {
        _validator = validator;
        _subdomainDiscoveryService = subdomainDiscoveryService;
        _ipResolutionService = ipResolutionService;
        _portScannerService = portScannerService;
        _logger = logger;
    }

    public async Task<DomainScanResult> ScanAsync(string? input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startedAt = DateTimeOffset.UtcNow;
        var targetDomain = input ?? string.Empty;

        _logger.LogInformation("Domain scan started.");
        _logger.LogInformation("Original domain input received: {DomainInput}", input);

        var validation = _validator.Validate(input);

        if (!validation.IsValid || validation.Domain is null)
        {
            _logger.LogWarning(
                "Validation failure for domain input {DomainInput}: {ValidationErrors}",
                input,
                string.Join("; ", validation.Errors));

            return new DomainScanResult(
                targetDomain,
                NormalizedDomain: null,
                startedAt,
                DateTimeOffset.UtcNow,
                DomainScanStatus.InvalidInput,
                "Domain scan input is invalid.",
                validation.Errors,
                Array.Empty<Subdomain>(),
                DiscoveredSubdomainCount: 0,
                SubdomainDiscoveryStatus.InvalidInput,
                Array.Empty<IpAddress>(),
                ResolvedIpAddressCount: 0,
                Array.Empty<string>(),
                IpResolutionStatus.NoResults,
                Array.Empty<PortScanResult>(),
                CheckedPortCount: 0,
                OpenPortCount: 0,
                TimedOutPortCount: 0,
                PortScanErrorCount: 0,
                PortScanStatus.NoOpenPorts);
        }

        _logger.LogInformation("Normalized host value: {NormalizedHost}", validation.Domain.NormalizedHost);
        _logger.LogInformation("Root domain value: {RootDomain}", validation.Domain.RootDomain);
        _logger.LogInformation("Validation success for {NormalizedHost}", validation.Domain.NormalizedHost);

        var discoveryResult = await _subdomainDiscoveryService.DiscoverAsync(
            validation.Domain.RootDomain,
            cancellationToken: cancellationToken);

        var ipResolutionResult = await _ipResolutionService.ResolveAsync(
            validation.Domain.NormalizedHost,
            discoveryResult.DiscoveredSubdomains,
            cancellationToken);

        var portScanResults = await _portScannerService.ScanAsync(
            ipResolutionResult.ResolvedAddresses,
            cancellationToken: cancellationToken);

        var result = new DomainScanResult(
            validation.Domain.OriginalInput,
            validation.Domain.NormalizedHost,
            startedAt,
            DateTimeOffset.UtcNow,
            DomainScanStatus.Success,
            "Domain scan, subdomain discovery, IP resolution, and safe port scan completed.",
            Array.Empty<string>(),
            discoveryResult.DiscoveredSubdomains,
            discoveryResult.DiscoveredSubdomainCount,
            discoveryResult.Status,
            ipResolutionResult.ResolvedAddresses,
            ipResolutionResult.ResolvedAddressCount,
            ipResolutionResult.FailedHosts,
            ipResolutionResult.Status,
            portScanResults,
            portScanResults.Sum(result => result.CheckedPorts.Count),
            portScanResults.Sum(result => result.OpenPorts.Count),
            portScanResults.Sum(result => result.TimedOutPorts.Count),
            portScanResults.Sum(result => result.CheckedPorts.Count(port => port.State == PortState.Error)),
            DeterminePortScanStatus(portScanResults))
        {
            RootDomain = validation.Domain.RootDomain
        };

        _logger.LogInformation("Domain scan completed for {NormalizedHost}", validation.Domain.NormalizedHost);

        return result;
    }

    private static PortScanStatus DeterminePortScanStatus(IReadOnlyList<PortScanResult> portScanResults)
    {
        if (portScanResults.Count == 0)
        {
            return PortScanStatus.NoOpenPorts;
        }

        if (portScanResults.Any(result => result.Status == PortScanStatus.Failed)
            || portScanResults.Any(result => result.Status == PortScanStatus.PartialSuccess))
        {
            return portScanResults.Any(result => result.OpenPorts.Count > 0 || result.ClosedPorts.Count > 0)
                ? PortScanStatus.PartialSuccess
                : PortScanStatus.Failed;
        }

        return portScanResults.Any(result => result.OpenPorts.Count > 0)
            ? PortScanStatus.Success
            : PortScanStatus.NoOpenPorts;
    }
}
