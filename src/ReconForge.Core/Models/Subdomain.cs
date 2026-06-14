namespace ReconForge.Core.Models;

public sealed record Subdomain(
    string Name,
    string RootDomain,
    string FullName,
    string Source,
    DateTimeOffset DiscoveredAt);
