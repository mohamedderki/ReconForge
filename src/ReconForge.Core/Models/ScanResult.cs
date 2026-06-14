namespace ReconForge.Core.Models;

public sealed record ScanResult(
    string Target,
    string ScanType,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    ScanStatus Status,
    IReadOnlyList<ScanFinding> Findings);
