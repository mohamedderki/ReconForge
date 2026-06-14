using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IResultExporter<in T>
{
    ExportFormat SupportedFormat { get; }

    string FileExtension { get; }

    Task<ExportResult> ExportAsync(
        T data,
        string outputPath,
        CancellationToken cancellationToken = default);
}
