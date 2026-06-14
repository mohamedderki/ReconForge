using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Export.Factories;

public sealed class ResultExporterFactory<T> : IResultExporterFactory<T>
{
    private readonly IReadOnlyDictionary<ExportFormat, IResultExporter<T>> _exporters;
    private readonly ILogger<ResultExporterFactory<T>> _logger;

    public ResultExporterFactory(
        IEnumerable<IResultExporter<T>> exporters,
        ILogger<ResultExporterFactory<T>> logger)
    {
        _exporters = exporters.ToDictionary(exporter => exporter.SupportedFormat);
        _logger = logger;
    }

    public IResultExporter<T> Create(ExportFormat format)
    {
        if (_exporters.TryGetValue(format, out var exporter))
        {
            _logger.LogInformation(
                "Exporter selected by factory: {ExporterType} for {ExportFormat}",
                exporter.GetType().Name,
                format);

            return exporter;
        }

        _logger.LogWarning("Unsupported format requested: {ExportFormat}", format);
        throw new NotSupportedException($"Unsupported export format: {format}");
    }
}
