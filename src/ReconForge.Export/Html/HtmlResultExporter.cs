using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Models;
using ReconForge.Export.Shared;

namespace ReconForge.Export.Html;

public sealed class HtmlResultExporter : ResultExporterBase<DomainScanResult>
{
    public HtmlResultExporter(ILogger<HtmlResultExporter> logger)
        : base(logger)
    {
    }

    public override ExportFormat SupportedFormat => ExportFormat.Html;

    public override string FileExtension => ".html";

    protected override string CreateContent(DomainScanResult data)
    {
        var builder = new StringBuilder();

        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\">");
        builder.AppendLine("<title>ReconForge Scan Report</title>");
        builder.AppendLine("<style>body{font-family:Arial,sans-serif;line-height:1.5;margin:2rem;}table{border-collapse:collapse;width:100%;margin-bottom:1.5rem;}th,td{border:1px solid #ccc;padding:.4rem;text-align:left;}th{background:#f4f4f4;}</style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("<h1>ReconForge Scan Report</h1>");

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Target Domain</h2>");
        builder.AppendLine($"<p><strong>Original:</strong> {Encode(data.TargetDomain)}</p>");
        builder.AppendLine($"<p><strong>Normalized:</strong> {Encode(data.NormalizedDomain ?? string.Empty)}</p>");
        builder.AppendLine($"<p><strong>Status:</strong> {Encode(data.Status.ToString())}</p>");
        builder.AppendLine("</section>");

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Summary</h2>");
        builder.AppendLine("<ul>");
        builder.AppendLine($"<li>Subdomains: {data.DiscoveredSubdomainCount}</li>");
        builder.AppendLine($"<li>Resolved IP addresses: {data.ResolvedIpAddressCount}</li>");
        builder.AppendLine($"<li>Checked ports: {data.CheckedPortCount}</li>");
        builder.AppendLine($"<li>Open ports: {data.OpenPortCount}</li>");
        builder.AppendLine($"<li>Timed out ports: {data.TimedOutPortCount}</li>");
        builder.AppendLine("</ul>");
        builder.AppendLine("</section>");

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Subdomains</h2>");
        builder.AppendLine("<table><thead><tr><th>Name</th><th>Full Name</th><th>Source</th><th>Discovered At</th></tr></thead><tbody>");
        foreach (var subdomain in data.DiscoveredSubdomains)
        {
            builder.AppendLine($"<tr><td>{Encode(subdomain.Name)}</td><td>{Encode(subdomain.FullName)}</td><td>{Encode(subdomain.Source)}</td><td>{Encode(subdomain.DiscoveredAt.ToString("O"))}</td></tr>");
        }
        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>IP Addresses</h2>");
        builder.AppendLine("<table><thead><tr><th>Address</th><th>Family</th><th>Related Host</th><th>Private</th><th>Loopback</th></tr></thead><tbody>");
        foreach (var address in data.ResolvedIpAddresses)
        {
            builder.AppendLine($"<tr><td>{Encode(address.Address)}</td><td>{Encode(address.AddressFamily)}</td><td>{Encode(address.RelatedHost)}</td><td>{address.IsPrivate}</td><td>{address.IsLoopback}</td></tr>");
        }
        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Port Scan Results</h2>");
        builder.AppendLine("<table><thead><tr><th>IP Address</th><th>Port</th><th>Protocol</th><th>Service</th><th>State</th><th>Response Time</th></tr></thead><tbody>");
        foreach (var result in data.PortScanResults)
        {
            foreach (var port in result.CheckedPorts)
            {
                builder.AppendLine($"<tr><td>{Encode(result.TargetIpAddress)}</td><td>{port.Number}</td><td>{Encode(port.Protocol)}</td><td>{Encode(port.ServiceName)}</td><td>{Encode(port.State.ToString())}</td><td>{Encode(port.ResponseTimeMs?.ToString() ?? string.Empty)}</td></tr>");
            }
        }
        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");

        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}
