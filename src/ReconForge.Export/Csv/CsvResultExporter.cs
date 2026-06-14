using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Models;
using ReconForge.Export.Shared;

namespace ReconForge.Export.Csv;

public sealed class CsvResultExporter : ResultExporterBase<DomainScanResult>
{
    public CsvResultExporter(ILogger<CsvResultExporter> logger)
        : base(logger)
    {
    }

    public override ExportFormat SupportedFormat => ExportFormat.Csv;

    public override string FileExtension => ".csv";

    protected override string CreateContent(DomainScanResult data)
    {
        var builder = new StringBuilder();
        WriteRow(builder,
            "TargetDomain",
            "NormalizedDomain",
            "Subdomain",
            "IpAddress",
            "RelatedHost",
            "Port",
            "PortState",
            "ServiceName",
            "CheckedAt",
            "ScanStatus",
            "IpResolutionStatus",
            "PortScanStatus");

        foreach (var portScanResult in data.PortScanResults)
        {
            foreach (var port in portScanResult.CheckedPorts)
            {
                WriteRow(builder,
                    data.TargetDomain,
                    data.NormalizedDomain ?? string.Empty,
                    GetSubdomainForHost(data, portScanResult.TargetIpAddress),
                    portScanResult.TargetIpAddress,
                    GetRelatedHostForIp(data, portScanResult.TargetIpAddress),
                    port.Number.ToString(CultureInfo.InvariantCulture),
                    port.State.ToString(),
                    port.ServiceName,
                    port.CheckedAt.ToString("O", CultureInfo.InvariantCulture),
                    data.Status.ToString(),
                    data.IpResolutionStatus.ToString(),
                    data.PortScanStatus.ToString());
            }
        }

        if (data.PortScanResults.Count == 0)
        {
            foreach (var address in data.ResolvedIpAddresses)
            {
                WriteRow(builder,
                    data.TargetDomain,
                    data.NormalizedDomain ?? string.Empty,
                    string.Empty,
                    address.Address,
                    address.RelatedHost,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    address.ResolvedAt.ToString("O", CultureInfo.InvariantCulture),
                    data.Status.ToString(),
                    data.IpResolutionStatus.ToString(),
                    data.PortScanStatus.ToString());
            }
        }

        if (data.ResolvedIpAddresses.Count == 0 && data.DiscoveredSubdomains.Count > 0)
        {
            foreach (var subdomain in data.DiscoveredSubdomains)
            {
                WriteRow(builder,
                    data.TargetDomain,
                    data.NormalizedDomain ?? string.Empty,
                    subdomain.FullName,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    subdomain.DiscoveredAt.ToString("O", CultureInfo.InvariantCulture),
                    data.Status.ToString(),
                    data.IpResolutionStatus.ToString(),
                    data.PortScanStatus.ToString());
            }
        }

        if (data.DiscoveredSubdomains.Count == 0 && data.ResolvedIpAddresses.Count == 0 && data.PortScanResults.Count == 0)
        {
            WriteRow(builder,
                data.TargetDomain,
                data.NormalizedDomain ?? string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                data.FinishedAt.ToString("O", CultureInfo.InvariantCulture),
                data.Status.ToString(),
                data.IpResolutionStatus.ToString(),
                data.PortScanStatus.ToString());
        }

        return builder.ToString();
    }

    private static void WriteRow(StringBuilder builder, params string[] values)
    {
        builder.AppendLine(string.Join(",", values.Select(Escape)));
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r')
            ? $"\"{escaped}\""
            : escaped;
    }

    private static string GetRelatedHostForIp(DomainScanResult data, string ipAddress)
    {
        return data.ResolvedIpAddresses
            .FirstOrDefault(address => string.Equals(address.Address, ipAddress, StringComparison.OrdinalIgnoreCase))
            ?.RelatedHost ?? string.Empty;
    }

    private static string GetSubdomainForHost(DomainScanResult data, string ipAddress)
    {
        var relatedHost = GetRelatedHostForIp(data, ipAddress);
        return data.DiscoveredSubdomains.Any(subdomain => string.Equals(subdomain.FullName, relatedHost, StringComparison.OrdinalIgnoreCase))
            ? relatedHost
            : string.Empty;
    }
}
