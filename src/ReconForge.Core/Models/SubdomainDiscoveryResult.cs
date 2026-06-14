namespace ReconForge.Core.Models;

public sealed record SubdomainDiscoveryResult(
    string TargetDomain,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    SubdomainDiscoveryStatus Status,
    string Message,
    IReadOnlyList<Subdomain> DiscoveredSubdomains,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public int DiscoveredSubdomainCount => DiscoveredSubdomains.Count;
}
