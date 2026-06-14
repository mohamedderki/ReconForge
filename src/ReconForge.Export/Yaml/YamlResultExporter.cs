using System.Text;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Models;
using ReconForge.Export.Shared;

namespace ReconForge.Export.Yaml;

public sealed class YamlResultExporter : ResultExporterBase<DomainScanResult>
{
    public YamlResultExporter(ILogger<YamlResultExporter> logger)
        : base(logger)
    {
    }

    public override ExportFormat SupportedFormat => ExportFormat.Yaml;

    public override string FileExtension => ".yaml";

    protected override string CreateContent(DomainScanResult data)
    {
        var builder = new StringBuilder();

        Write(builder, "targetDomain", data.TargetDomain);
        Write(builder, "normalizedDomain", data.NormalizedDomain ?? string.Empty);
        Write(builder, "status", data.Status.ToString());
        Write(builder, "message", data.Message);
        Write(builder, "startedAt", data.StartedAt.ToString("O"));
        Write(builder, "finishedAt", data.FinishedAt.ToString("O"));

        builder.AppendLine("summary:");
        Write(builder, "  discoveredSubdomainCount", data.DiscoveredSubdomainCount.ToString());
        Write(builder, "  resolvedIpAddressCount", data.ResolvedIpAddressCount.ToString());
        Write(builder, "  checkedPortCount", data.CheckedPortCount.ToString());
        Write(builder, "  openPortCount", data.OpenPortCount.ToString());
        Write(builder, "  timedOutPortCount", data.TimedOutPortCount.ToString());
        Write(builder, "  portScanErrorCount", data.PortScanErrorCount.ToString());

        builder.AppendLine("subdomains:");
        foreach (var subdomain in data.DiscoveredSubdomains)
        {
            builder.AppendLine("  -");
            Write(builder, "    name", subdomain.Name);
            Write(builder, "    rootDomain", subdomain.RootDomain);
            Write(builder, "    fullName", subdomain.FullName);
            Write(builder, "    source", subdomain.Source);
            Write(builder, "    discoveredAt", subdomain.DiscoveredAt.ToString("O"));
        }

        builder.AppendLine("ipAddresses:");
        foreach (var address in data.ResolvedIpAddresses)
        {
            builder.AppendLine("  -");
            Write(builder, "    address", address.Address);
            Write(builder, "    addressFamily", address.AddressFamily);
            Write(builder, "    relatedHost", address.RelatedHost);
            Write(builder, "    resolvedAt", address.ResolvedAt.ToString("O"));
            Write(builder, "    isPrivate", address.IsPrivate.ToString().ToLowerInvariant());
            Write(builder, "    isLoopback", address.IsLoopback.ToString().ToLowerInvariant());
        }

        builder.AppendLine("portScanResults:");
        foreach (var result in data.PortScanResults)
        {
            builder.AppendLine("  -");
            Write(builder, "    targetIpAddress", result.TargetIpAddress);
            Write(builder, "    status", result.Status.ToString());
            builder.AppendLine("    checkedPorts:");

            foreach (var port in result.CheckedPorts)
            {
                builder.AppendLine("      -");
                Write(builder, "        number", port.Number.ToString());
                Write(builder, "        protocol", port.Protocol);
                Write(builder, "        serviceName", port.ServiceName);
                Write(builder, "        state", port.State.ToString());
                Write(builder, "        checkedAt", port.CheckedAt.ToString("O"));
                Write(builder, "        responseTimeMs", port.ResponseTimeMs?.ToString() ?? string.Empty);
            }
        }

        return builder.ToString();
    }

    private static void Write(StringBuilder builder, string key, string value)
    {
        builder.Append(key);
        builder.Append(": ");
        builder.AppendLine(Quote(value));
    }

    private static string Quote(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");

        return $"\"{escaped}\"";
    }
}
