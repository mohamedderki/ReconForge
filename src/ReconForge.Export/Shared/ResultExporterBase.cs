using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Export.Shared;

public abstract class ResultExporterBase<T> : IResultExporter<T>
{
    private readonly ILogger _logger;

    protected ResultExporterBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract ExportFormat SupportedFormat { get; }

    public abstract string FileExtension { get; }

    public async Task<ExportResult> ExportAsync(
        T data,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Export started.");
        _logger.LogInformation("Selected export format: {ExportFormat}", SupportedFormat);
        _logger.LogInformation("Output path selected: {OutputPath}", outputPath);

        try
        {
            _logger.LogInformation("Serialization or report generation started.");
            var content = CreateContent(data);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Output directory created: {OutputDirectory}", directory);
            }

            if (File.Exists(outputPath))
            {
                var message = $"The output file already exists: {outputPath}";
                _logger.LogWarning(message);

                return new ExportResult(
                    Success: false,
                    outputPath,
                    SupportedFormat,
                    startedAt,
                    DateTimeOffset.UtcNow,
                    "Export failed.",
                    message);
            }

            await File.WriteAllTextAsync(outputPath, content, cancellationToken);
            _logger.LogInformation("File written successfully: {OutputPath}", outputPath);
            _logger.LogInformation("Export completed.");

            return new ExportResult(
                Success: true,
                outputPath,
                SupportedFormat,
                startedAt,
                DateTimeOffset.UtcNow,
                $"Export completed successfully as {SupportedFormat}.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Export failed.");

            return new ExportResult(
                Success: false,
                outputPath,
                SupportedFormat,
                startedAt,
                DateTimeOffset.UtcNow,
                "Export failed.",
                exception.Message);
        }
    }

    protected abstract string CreateContent(T data);
}
