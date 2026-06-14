using System.Text;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Export.Paths;

public sealed class ExportPathResolver : IExportPathResolver
{
    private static readonly IReadOnlyDictionary<ExportFormat, string> Extensions = new Dictionary<ExportFormat, string>
    {
        [ExportFormat.Json] = ".json",
        [ExportFormat.Csv] = ".csv",
        [ExportFormat.Xml] = ".xml",
        [ExportFormat.Html] = ".html",
        [ExportFormat.Yaml] = ".yaml"
    };

    public ExportPathResolutionResult Resolve(
        DomainScanResult scanResult,
        ExportFormat format,
        string? requestedOutputPath)
    {
        try
        {
            var extension = GetExtension(format);

            if (string.IsNullOrWhiteSpace(requestedOutputPath))
            {
                return ExportPathResolutionResult.Succeeded(CreateDefaultPath(scanResult, extension));
            }

            var outputPath = requestedOutputPath.Trim();
            var currentExtension = Path.GetExtension(outputPath);

            if (string.IsNullOrWhiteSpace(currentExtension))
            {
                outputPath += extension;
            }
            else if (!string.Equals(currentExtension, extension, StringComparison.OrdinalIgnoreCase))
            {
                return ExportPathResolutionResult.Failed(
                    $"The output file extension '{currentExtension}' does not match the selected format '{format}'. Expected '{extension}'.");
            }

            var fullPath = Path.GetFullPath(outputPath);

            return File.Exists(fullPath)
                ? ExportPathResolutionResult.Failed($"The output file already exists: {fullPath}")
                : ExportPathResolutionResult.Succeeded(fullPath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return ExportPathResolutionResult.Failed($"The output path is invalid: {exception.Message}");
        }
    }

    private static string CreateDefaultPath(DomainScanResult scanResult, string extension)
    {
        var domain = string.IsNullOrWhiteSpace(scanResult.NormalizedDomain)
            ? "scan-result"
            : scanResult.NormalizedDomain;

        var safeDomain = ToSafeFileName(domain);
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd-HHmmss");
        var fileName = $"reconforge-{safeDomain}-{timestamp}{extension}";
        var fullPath = Path.GetFullPath(fileName);

        if (!File.Exists(fullPath))
        {
            return fullPath;
        }

        for (var counter = 1; counter < 1000; counter++)
        {
            var candidate = Path.GetFullPath($"reconforge-{safeDomain}-{timestamp}-{counter}{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Unable to create a unique default export file name.");
    }

    private static string GetExtension(ExportFormat format)
    {
        return Extensions.TryGetValue(format, out var extension)
            ? extension
            : throw new NotSupportedException($"Unsupported export format: {format}");
    }

    private static string ToSafeFileName(string value)
    {
        var builder = new StringBuilder();

        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (character is '.' or '-' or '_')
            {
                builder.Append('-');
            }
        }

        var result = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(result)
            ? "scan-result"
            : result;
    }
}
