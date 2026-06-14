using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IExportPathResolver
{
    ExportPathResolutionResult Resolve(
        DomainScanResult scanResult,
        ExportFormat format,
        string? requestedOutputPath);
}
