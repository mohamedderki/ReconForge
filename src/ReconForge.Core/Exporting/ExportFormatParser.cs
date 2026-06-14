using ReconForge.Core.Models;

namespace ReconForge.Core.Exporting;

public static class ExportFormatParser
{
    public static bool TryParse(
        string? value,
        out ExportFormat format,
        out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            format = ExportFormat.Json;
            errorMessage = string.Empty;
            return true;
        }

        if (Enum.TryParse(value.Trim(), ignoreCase: true, out format)
            && Enum.IsDefined(format))
        {
            errorMessage = string.Empty;
            return true;
        }

        format = ExportFormat.Json;
        errorMessage = $"Unsupported export format '{value}'. Supported formats are: json, csv, xml, html, yaml.";
        return false;
    }
}
