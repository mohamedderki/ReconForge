namespace ReconForge.Core.Models;

public sealed record IpAddress(
    string Address,
    string AddressFamily,
    string RelatedHost,
    DateTimeOffset ResolvedAt,
    bool IsPrivate,
    bool IsLoopback);
