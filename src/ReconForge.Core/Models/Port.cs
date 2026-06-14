namespace ReconForge.Core.Models;

public sealed record Port(
    int Number,
    string Protocol,
    string ServiceName,
    PortState State,
    DateTimeOffset CheckedAt,
    long? ResponseTimeMs);
