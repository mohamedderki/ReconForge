namespace ReconForge.Core.Models;

public sealed record ExportPathResolutionResult(
    bool Success,
    string? OutputPath,
    string? ErrorMessage)
{
    public static ExportPathResolutionResult Succeeded(string outputPath)
    {
        return new ExportPathResolutionResult(true, outputPath, null);
    }

    public static ExportPathResolutionResult Failed(string errorMessage)
    {
        return new ExportPathResolutionResult(false, null, errorMessage);
    }
}
