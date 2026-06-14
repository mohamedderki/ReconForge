namespace ReconForge.Core.Models;

public sealed record IpResolutionResult(
    string TargetDomain,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    IpResolutionStatus Status,
    string Message,
    IReadOnlyList<IpAddress> ResolvedAddresses,
    IReadOnlyList<string> FailedHosts,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public int ResolvedAddressCount => ResolvedAddresses.Count;
}
