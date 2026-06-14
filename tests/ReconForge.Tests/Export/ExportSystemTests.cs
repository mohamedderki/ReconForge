using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Exporting;
using ReconForge.Core.Models;
using ReconForge.Export.Csv;
using ReconForge.Export.Factories;
using ReconForge.Export.Html;
using ReconForge.Export.Json;
using ReconForge.Export.Paths;
using ReconForge.Export.Xml;
using ReconForge.Export.Yaml;
using ReconForge.Tests.TestData;

namespace ReconForge.Tests.Export;

public sealed class ExportSystemTests
{
    [Theory]
    [InlineData("json", ExportFormat.Json)]
    [InlineData("CSV", ExportFormat.Csv)]
    [InlineData("xml", ExportFormat.Xml)]
    [InlineData("Html", ExportFormat.Html)]
    [InlineData("yaml", ExportFormat.Yaml)]
    [InlineData(null, ExportFormat.Json)]
    public void ExportFormatParser_ParsesSupportedFormats_CaseInsensitively(
        string? value,
        ExportFormat expected)
    {
        var parsed = ExportFormatParser.TryParse(value, out var format, out var error);

        Assert.True(parsed);
        Assert.Equal(expected, format);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void ExportFormatParser_RejectsUnsupportedFormat()
    {
        var parsed = ExportFormatParser.TryParse("pdf", out _, out var error);

        Assert.False(parsed);
        Assert.Contains("Unsupported export format", error);
    }

    [Theory]
    [InlineData(ExportFormat.Json, typeof(JsonResultExporter))]
    [InlineData(ExportFormat.Csv, typeof(CsvResultExporter))]
    [InlineData(ExportFormat.Xml, typeof(XmlResultExporter))]
    [InlineData(ExportFormat.Html, typeof(HtmlResultExporter))]
    [InlineData(ExportFormat.Yaml, typeof(YamlResultExporter))]
    public void ResultExporterFactory_ReturnsExpectedExporter(
        ExportFormat format,
        Type expectedType)
    {
        var factory = CreateFactory();

        var exporter = factory.Create(format);

        Assert.IsType(expectedType, exporter);
    }

    [Fact]
    public void ResultExporterFactory_RejectsUnsupportedFormat()
    {
        var factory = CreateFactory();

        Assert.Throws<NotSupportedException>(() => factory.Create((ExportFormat)999));
    }

    [Theory]
    [InlineData(ExportFormat.Json, ".json")]
    [InlineData(ExportFormat.Csv, ".csv")]
    [InlineData(ExportFormat.Xml, ".xml")]
    [InlineData(ExportFormat.Html, ".html")]
    [InlineData(ExportFormat.Yaml, ".yaml")]
    public void ExportPathResolver_UsesMatchingExtension(
        ExportFormat format,
        string expectedExtension)
    {
        var resolver = new ExportPathResolver();

        var result = resolver.Resolve(SampleScanResultFactory.CreateDomainScanResult(), format, null);

        Assert.True(result.Success);
        Assert.EndsWith(expectedExtension, result.OutputPath);
    }

    [Fact]
    public void ExportPathResolver_RejectsMismatchedExtension()
    {
        var resolver = new ExportPathResolver();

        var result = resolver.Resolve(SampleScanResultFactory.CreateDomainScanResult(), ExportFormat.Json, "report.csv");

        Assert.False(result.Success);
        Assert.Contains("does not match", result.ErrorMessage);
    }

    [Theory]
    [InlineData(ExportFormat.Json)]
    [InlineData(ExportFormat.Csv)]
    [InlineData(ExportFormat.Xml)]
    [InlineData(ExportFormat.Html)]
    [InlineData(ExportFormat.Yaml)]
    public async Task Exporter_CreatesExpectedFile(ExportFormat format)
    {
        var directory = CreateTempDirectory();
        var outputPath = Path.Combine(directory, $"report.{GetExtension(format)}");
        var scanResult = SampleScanResultFactory.CreateDomainScanResult();
        var exporter = CreateFactory().Create(format);

        try
        {
            var result = await exporter.ExportAsync(scanResult, outputPath);

            Assert.True(result.Success);
            Assert.True(File.Exists(outputPath));
            Assert.EndsWith($".{GetExtension(format)}", result.OutputPath);
            Assert.NotEmpty(await File.ReadAllTextAsync(outputPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task JsonResultExporter_CreatesReadableJsonFile()
    {
        var outputPath = CreateTempFilePath(".json");
        var exporter = new JsonResultExporter(NullLogger<JsonResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            using var stream = File.OpenRead(outputPath);
            using var document = await JsonDocument.ParseAsync(stream);
            Assert.Equal("example.com", document.RootElement.GetProperty("TargetDomain").GetString());
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task CsvResultExporter_CreatesSpreadsheetFriendlyCsvFile()
    {
        var outputPath = CreateTempFilePath(".csv");
        var exporter = new CsvResultExporter(NullLogger<CsvResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            var csv = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("TargetDomain,NormalizedDomain,Subdomain,IpAddress", csv);
            Assert.Contains("example.com", csv);
            Assert.Contains("Open", csv);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task XmlResultExporter_CreatesValidXmlFile()
    {
        var outputPath = CreateTempFilePath(".xml");
        var exporter = new XmlResultExporter(NullLogger<XmlResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            var document = XDocument.Load(outputPath);
            Assert.Equal("ReconForgeScanResult", document.Root?.Name.LocalName);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task HtmlResultExporter_CreatesHtmlReport()
    {
        var outputPath = CreateTempFilePath(".html");
        var exporter = new HtmlResultExporter(NullLogger<HtmlResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            var html = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("<html", html);
            Assert.Contains("Target Domain", html);
            Assert.Contains("Port Scan Results", html);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task YamlResultExporter_CreatesYamlFile()
    {
        var outputPath = CreateTempFilePath(".yaml");
        var exporter = new YamlResultExporter(NullLogger<YamlResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            var yaml = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("targetDomain:", yaml);
            Assert.Contains("subdomains:", yaml);
            Assert.Contains("portScanResults:", yaml);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task Exporter_DoesNotModifyScanResult()
    {
        var outputPath = CreateTempFilePath(".json");
        var scanResult = SampleScanResultFactory.CreateDomainScanResult();
        var originalTarget = scanResult.TargetDomain;
        var originalOpenPortCount = scanResult.OpenPortCount;
        var exporter = new JsonResultExporter(NullLogger<JsonResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(scanResult, outputPath);

            Assert.Equal(originalTarget, scanResult.TargetDomain);
            Assert.Equal(originalOpenPortCount, scanResult.OpenPortCount);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task Exporter_DoesNotPerformScanning()
    {
        var outputPath = CreateTempFilePath(".json");
        var scanResult = SampleScanResultFactory.CreateDomainScanResult();
        var exporter = new JsonResultExporter(NullLogger<JsonResultExporter>.Instance);

        try
        {
            await exporter.ExportAsync(scanResult, outputPath);

            Assert.Single(scanResult.PortScanResults);
            Assert.Single(scanResult.ResolvedIpAddresses);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task Exporter_ReturnsFailure_WhenOutputFileAlreadyExists()
    {
        var outputPath = CreateTempFilePath(".json");
        await File.WriteAllTextAsync(outputPath, "existing");
        var exporter = new JsonResultExporter(NullLogger<JsonResultExporter>.Instance);

        try
        {
            var result = await exporter.ExportAsync(SampleScanResultFactory.CreateDomainScanResult(), outputPath);

            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    private static IResultExporterFactory<DomainScanResult> CreateFactory()
    {
        IResultExporter<DomainScanResult>[] exporters =
        [
            new JsonResultExporter(NullLogger<JsonResultExporter>.Instance),
            new CsvResultExporter(NullLogger<CsvResultExporter>.Instance),
            new XmlResultExporter(NullLogger<XmlResultExporter>.Instance),
            new HtmlResultExporter(NullLogger<HtmlResultExporter>.Instance),
            new YamlResultExporter(NullLogger<YamlResultExporter>.Instance)
        ];

        return new ResultExporterFactory<DomainScanResult>(
            exporters,
            NullLogger<ResultExporterFactory<DomainScanResult>>.Instance);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"reconforge-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string CreateTempFilePath(string extension)
    {
        return Path.Combine(Path.GetTempPath(), $"reconforge-tests-{Guid.NewGuid():N}{extension}");
    }

    private static string GetExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Json => "json",
            ExportFormat.Csv => "csv",
            ExportFormat.Xml => "xml",
            ExportFormat.Html => "html",
            ExportFormat.Yaml => "yaml",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
