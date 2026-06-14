using Microsoft.Extensions.DependencyInjection;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Export.Csv;
using ReconForge.Export.Factories;
using ReconForge.Export.Html;
using ReconForge.Export.Json;
using ReconForge.Export.Paths;
using ReconForge.Export.Xml;
using ReconForge.Export.Yaml;
using ReconForge.Scanning.Domain;
using ReconForge.Scanning.Ip;
using ReconForge.Scanning.Ports;
using ReconForge.Scanning.Subdomains;
using ReconForge.Core.Validation;

namespace ReconForge.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReconForge(this IServiceCollection services)
    {
        services.AddSingleton<IDomainValidator, DomainNameValidator>();
        services.AddSingleton<IDomainScanService, DomainScanService>();
        services.AddSingleton<IDomainScanner, DomainScanner>();
        services.AddSingleton<ISubdomainDiscoveryService, SubdomainDiscoveryService>();
        services.AddSingleton<IDnsLookupClient, DnsLookupClient>();
        services.AddSingleton<IIpResolutionService, IpResolutionService>();
        services.AddSingleton<ITcpConnectionChecker, TcpConnectionChecker>();
        services.AddSingleton<IPortScannerService, PortScannerService>();
        services.AddSingleton<IResultExporter<DomainScanResult>, JsonResultExporter>();
        services.AddSingleton<IResultExporter<DomainScanResult>, CsvResultExporter>();
        services.AddSingleton<IResultExporter<DomainScanResult>, XmlResultExporter>();
        services.AddSingleton<IResultExporter<DomainScanResult>, HtmlResultExporter>();
        services.AddSingleton<IResultExporter<DomainScanResult>, YamlResultExporter>();
        services.AddSingleton<IResultExporterFactory<DomainScanResult>, ResultExporterFactory<DomainScanResult>>();
        services.AddSingleton<IExportPathResolver, ExportPathResolver>();

        return services;
    }
}
