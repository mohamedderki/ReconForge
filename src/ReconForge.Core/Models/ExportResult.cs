namespace ReconForge.Core.Models;

public sealed record ExportResult(
    bool Success,
    string OutputPath,
    ExportFormat Format,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    string Message,
    string? ErrorMessage = null);
