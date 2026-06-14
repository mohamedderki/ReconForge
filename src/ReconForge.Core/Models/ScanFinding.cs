namespace ReconForge.Core.Models;

public sealed record ScanFinding(
    string Category,
    string Description,
    string Severity = "Info");
