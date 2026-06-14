using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IResultExporterFactory<T>
{
    IResultExporter<T> Create(ExportFormat format);
}
