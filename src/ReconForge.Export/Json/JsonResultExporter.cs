using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Models;
using ReconForge.Export.Shared;

namespace ReconForge.Export.Json;

public sealed class JsonResultExporter : ResultExporterBase<DomainScanResult>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonResultExporter(ILogger<JsonResultExporter> logger)
        : base(logger)
    {
    }

    public override ExportFormat SupportedFormat => ExportFormat.Json;

    public override string FileExtension => ".json";

    protected override string CreateContent(DomainScanResult data)
    {
        return JsonSerializer.Serialize(data, SerializerOptions);
    }
}
