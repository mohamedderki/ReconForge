using System.Xml;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Models;
using ReconForge.Export.Shared;

namespace ReconForge.Export.Xml;

public sealed class XmlResultExporter : ResultExporterBase<DomainScanResult>
{
    public XmlResultExporter(ILogger<XmlResultExporter> logger)
        : base(logger)
    {
    }

    public override ExportFormat SupportedFormat => ExportFormat.Xml;

    public override string FileExtension => ".xml";

    protected override string CreateContent(DomainScanResult data)
    {
        using var writer = new StringWriter();
        using var xml = XmlWriter.Create(writer, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        });

        xml.WriteStartElement("ReconForgeScanResult");

        WriteElement(xml, "TargetDomain", data.TargetDomain);
        WriteElement(xml, "NormalizedDomain", data.NormalizedDomain ?? string.Empty);
        WriteElement(xml, "Status", data.Status.ToString());
        WriteElement(xml, "Message", data.Message);
        WriteElement(xml, "StartedAt", data.StartedAt.ToString("O"));
        WriteElement(xml, "FinishedAt", data.FinishedAt.ToString("O"));

        xml.WriteStartElement("Summary");
        WriteElement(xml, "DiscoveredSubdomainCount", data.DiscoveredSubdomainCount.ToString());
        WriteElement(xml, "ResolvedIpAddressCount", data.ResolvedIpAddressCount.ToString());
        WriteElement(xml, "CheckedPortCount", data.CheckedPortCount.ToString());
        WriteElement(xml, "OpenPortCount", data.OpenPortCount.ToString());
        WriteElement(xml, "TimedOutPortCount", data.TimedOutPortCount.ToString());
        WriteElement(xml, "PortScanErrorCount", data.PortScanErrorCount.ToString());
        xml.WriteEndElement();

        xml.WriteStartElement("Subdomains");
        foreach (var subdomain in data.DiscoveredSubdomains)
        {
            xml.WriteStartElement("Subdomain");
            WriteElement(xml, "Name", subdomain.Name);
            WriteElement(xml, "RootDomain", subdomain.RootDomain);
            WriteElement(xml, "FullName", subdomain.FullName);
            WriteElement(xml, "Source", subdomain.Source);
            WriteElement(xml, "DiscoveredAt", subdomain.DiscoveredAt.ToString("O"));
            xml.WriteEndElement();
        }
        xml.WriteEndElement();

        xml.WriteStartElement("IpAddresses");
        foreach (var address in data.ResolvedIpAddresses)
        {
            xml.WriteStartElement("IpAddress");
            WriteElement(xml, "Address", address.Address);
            WriteElement(xml, "AddressFamily", address.AddressFamily);
            WriteElement(xml, "RelatedHost", address.RelatedHost);
            WriteElement(xml, "ResolvedAt", address.ResolvedAt.ToString("O"));
            WriteElement(xml, "IsPrivate", address.IsPrivate.ToString());
            WriteElement(xml, "IsLoopback", address.IsLoopback.ToString());
            xml.WriteEndElement();
        }
        xml.WriteEndElement();

        xml.WriteStartElement("PortScanResults");
        foreach (var result in data.PortScanResults)
        {
            xml.WriteStartElement("PortScanResult");
            WriteElement(xml, "TargetIpAddress", result.TargetIpAddress);
            WriteElement(xml, "Status", result.Status.ToString());
            WriteElement(xml, "Message", result.Message);

            xml.WriteStartElement("CheckedPorts");
            foreach (var port in result.CheckedPorts)
            {
                xml.WriteStartElement("Port");
                WriteElement(xml, "Number", port.Number.ToString());
                WriteElement(xml, "Protocol", port.Protocol);
                WriteElement(xml, "ServiceName", port.ServiceName);
                WriteElement(xml, "State", port.State.ToString());
                WriteElement(xml, "CheckedAt", port.CheckedAt.ToString("O"));
                WriteElement(xml, "ResponseTimeMs", port.ResponseTimeMs?.ToString() ?? string.Empty);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            xml.WriteEndElement();
        }
        xml.WriteEndElement();

        xml.WriteEndElement();
        xml.Flush();

        return writer.ToString();
    }

    private static void WriteElement(XmlWriter writer, string name, string value)
    {
        writer.WriteElementString(name, value);
    }
}
