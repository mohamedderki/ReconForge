namespace ReconForge.Core.Models;

public sealed record DomainScanResult(
    string TargetDomain,
    string? NormalizedDomain,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    DomainScanStatus Status,
    string Message,
    IReadOnlyList<string> ValidationErrors,
    IReadOnlyList<Subdomain> DiscoveredSubdomains,
    int DiscoveredSubdomainCount,
    SubdomainDiscoveryStatus SubdomainDiscoveryStatus,
    IReadOnlyList<IpAddress> ResolvedIpAddresses,
    int ResolvedIpAddressCount,
    IReadOnlyList<string> FailedHosts,
    IpResolutionStatus IpResolutionStatus,
    IReadOnlyList<PortScanResult> PortScanResults,
    int CheckedPortCount,
    int OpenPortCount,
    int TimedOutPortCount,
    int PortScanErrorCount,
    PortScanStatus PortScanStatus)
{
    public string? RootDomain { get; init; } = NormalizedDomain;
}
