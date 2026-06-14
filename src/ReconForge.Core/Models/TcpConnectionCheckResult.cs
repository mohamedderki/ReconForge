namespace ReconForge.Core.Models;

public sealed record TcpConnectionCheckResult(
    PortState State,
    long? ResponseTimeMs = null,
    string? ErrorMessage = null);
