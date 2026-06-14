using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Cli.Commands;
using ReconForge.Cli.Presentation;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Tests.Cli;

public sealed class CliScanExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenScanTargetIsMissing()
    {
        var scanService = new RecordingDomainScanService();
        var executor = CreateExecutor(scanService);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings());

        Assert.Equal(1, exitCode);
        Assert.False(scanService.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenDomainIsEmpty()
    {
        var scanService = new RecordingDomainScanService(DomainScanStatus.InvalidInput);
        var executor = CreateExecutor(scanService);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
        {
            ScanTarget = "   "
        });

        Assert.Equal(1, exitCode);
        Assert.True(scanService.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZeroAndRendersTerminalResult_WhenDomainIsValid()
    {
        var scanService = new RecordingDomainScanService();
        var renderer = new RecordingScanResultConsoleRenderer();
        var exporterFactory = new StaticResultExporterFactory(new StaticResultExporter("{\"status\":\"ok\"}"));
        var executor = CreateExecutor(scanService, exporterFactory, resultRenderer: renderer);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
        {
            ScanTarget = "Example.COM"
        });

        Assert.Equal(0, exitCode);
        Assert.True(scanService.WasCalled);
        Assert.True(renderer.WasCalled);
        Assert.False(exporterFactory.WasCalled);
        Assert.Equal("Example.COM", scanService.LastInput);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExport_WhenExportIsNotRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"reconforge-{Guid.NewGuid():N}.json");
        var scanService = new RecordingDomainScanService();
        var exporter = new StaticResultExporter("{\"status\":\"ok\"}");
        var exporterFactory = new StaticResultExporterFactory(exporter);
        var executor = CreateExecutor(scanService, exporterFactory);

        try
        {
            var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
            {
                ScanTarget = "Example.COM",
                OutputPath = outputPath,
                Format = "json"
            });

            Assert.Equal(0, exitCode);
            Assert.True(scanService.WasCalled);
            Assert.False(exporterFactory.WasCalled);
            Assert.False(exporter.WasCalled);
            Assert.False(File.Exists(outputPath));
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenExportFormatIsUnsupportedAndExportIsRequested()
    {
        var scanService = new RecordingDomainScanService();
        var executor = CreateExecutor(scanService);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
        {
            ScanTarget = "example.com",
            Export = true,
            Format = "pdf"
        });

        Assert.Equal(1, exitCode);
        Assert.False(scanService.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WhenVerboseIsEnabled()
    {
        var scanService = new RecordingDomainScanService();
        var renderer = new RecordingScanResultConsoleRenderer();
        var executor = CreateExecutor(scanService, resultRenderer: renderer);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
        {
            ScanTarget = "example.com",
            Verbose = true
        });

        Assert.Equal(0, exitCode);
        Assert.True(scanService.WasCalled);
        Assert.True(renderer.LastVerbose);
    }

    [Fact]
    public async Task ExecuteAsync_ExportsAfterTerminalRendering_WhenExportIsRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"reconforge-{Guid.NewGuid():N}.json");
        var events = new List<string>();
        var scanService = new RecordingDomainScanService(events);
        var exporter = new StaticResultExporter("{}", events);
        var renderer = new RecordingScanResultConsoleRenderer(events);
        var executor = CreateExecutor(
            scanService,
            new StaticResultExporterFactory(exporter, events),
            resultRenderer: renderer);

        try
        {
            var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
            {
                ScanTarget = "example.com",
                Export = true,
                OutputPath = outputPath,
                Format = "json"
            });

            Assert.Equal(0, exitCode);
            Assert.True(exporter.WasCalled);
            Assert.Equal(new[] { "scan", "render", "factory", "export" }, events);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExport_WhenFormatIsProvidedWithoutExport()
    {
        var scanService = new RecordingDomainScanService();
        var exporterFactory = new StaticResultExporterFactory(new StaticResultExporter("{}"));
        var executor = CreateExecutor(scanService, exporterFactory);

        var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
        {
            ScanTarget = "example.com",
            Format = "pdf"
        });

        Assert.Equal(0, exitCode);
        Assert.True(scanService.WasCalled);
        Assert.False(exporterFactory.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_IgnoresOutputPath_WhenExportIsNotRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"reconforge-{Guid.NewGuid():N}.json");
        var scanService = new RecordingDomainScanService();
        var exporterFactory = new StaticResultExporterFactory(new StaticResultExporter("{}"));
        var executor = CreateExecutor(scanService, exporterFactory);

        try
        {
            var exitCode = await executor.ExecuteAsync(new RootCommand.Settings
            {
                ScanTarget = "example.com",
                OutputPath = outputPath
            });

            Assert.Equal(0, exitCode);
            Assert.True(scanService.WasCalled);
            Assert.False(exporterFactory.WasCalled);
            Assert.False(File.Exists(outputPath));
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    private static CliScanExecutor CreateExecutor(
        RecordingDomainScanService scanService,
        IResultExporterFactory<DomainScanResult>? exporterFactory = null,
        IExportPathResolver? exportPathResolver = null,
        IScanResultConsoleRenderer? resultRenderer = null)
    {
        return new CliScanExecutor(
            scanService,
            exporterFactory ?? new StaticResultExporterFactory(new StaticResultExporter("{}")),
            exportPathResolver ?? new PassthroughExportPathResolver(),
            resultRenderer ?? new RecordingScanResultConsoleRenderer(),
            NullLogger<CliScanExecutor>.Instance);
    }

    private sealed class RecordingDomainScanService : IDomainScanService
    {
        private readonly DomainScanStatus _status;
        private readonly List<string>? _events;

        public RecordingDomainScanService(
            DomainScanStatus status = DomainScanStatus.Success,
            List<string>? events = null)
        {
            _status = status;
            _events = events;
        }

        public RecordingDomainScanService(List<string> events)
            : this(DomainScanStatus.Success, events)
        {
        }

        public bool WasCalled { get; private set; }

        public string? LastInput { get; private set; }

        public Task<DomainScanResult> ScanAsync(string? input, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            LastInput = input;
            _events?.Add("scan");

            var now = DateTimeOffset.UtcNow;
            var result = new DomainScanResult(
                input ?? string.Empty,
                _status == DomainScanStatus.Success ? "example.com" : null,
                now,
                now,
                _status,
                _status == DomainScanStatus.Success ? "OK" : "Invalid input",
                _status == DomainScanStatus.Success
                    ? Array.Empty<string>()
                    : new[] { "Invalid domain." },
                Array.Empty<Subdomain>(),
                DiscoveredSubdomainCount: 0,
                _status == DomainScanStatus.Success
                    ? SubdomainDiscoveryStatus.Success
                    : SubdomainDiscoveryStatus.InvalidInput,
                Array.Empty<IpAddress>(),
                ResolvedIpAddressCount: 0,
                Array.Empty<string>(),
                _status == DomainScanStatus.Success
                    ? IpResolutionStatus.Success
                    : IpResolutionStatus.NoResults,
                Array.Empty<PortScanResult>(),
                CheckedPortCount: 0,
                OpenPortCount: 0,
                TimedOutPortCount: 0,
                PortScanErrorCount: 0,
                _status == DomainScanStatus.Success
                    ? PortScanStatus.NoOpenPorts
                    : PortScanStatus.NoOpenPorts);

            return Task.FromResult(result);
        }
    }

    private sealed class StaticResultExporterFactory : IResultExporterFactory<DomainScanResult>
    {
        private readonly IResultExporter<DomainScanResult> _exporter;
        private readonly List<string>? _events;

        public StaticResultExporterFactory(
            IResultExporter<DomainScanResult> exporter,
            List<string>? events = null)
        {
            _exporter = exporter;
            _events = events;
        }

        public bool WasCalled { get; private set; }

        public IResultExporter<DomainScanResult> Create(ExportFormat format)
        {
            WasCalled = true;
            _events?.Add("factory");
            return _exporter;
        }
    }

    private sealed class StaticResultExporter : IResultExporter<DomainScanResult>
    {
        private readonly string _payload;
        private readonly List<string>? _events;

        public StaticResultExporter(string payload, List<string>? events = null)
        {
            _payload = payload;
            _events = events;
        }

        public bool WasCalled { get; private set; }

        public ExportFormat SupportedFormat => ExportFormat.Json;

        public string FileExtension => ".json";

        public async Task<ExportResult> ExportAsync(
            DomainScanResult data,
            string outputPath,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            _events?.Add("export");
            await File.WriteAllTextAsync(outputPath, _payload, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            return new ExportResult(
                true,
                outputPath,
                SupportedFormat,
                now,
                now,
                "OK");
        }
    }

    private sealed class PassthroughExportPathResolver : IExportPathResolver
    {
        public ExportPathResolutionResult Resolve(
            DomainScanResult scanResult,
            ExportFormat format,
            string? requestedOutputPath)
        {
            return ExportPathResolutionResult.Succeeded(requestedOutputPath ?? Path.Combine(Path.GetTempPath(), $"reconforge-{Guid.NewGuid():N}.json"));
        }
    }

    private sealed class RecordingScanResultConsoleRenderer : IScanResultConsoleRenderer
    {
        private readonly List<string>? _events;

        public RecordingScanResultConsoleRenderer(List<string>? events = null)
        {
            _events = events;
        }

        public bool WasCalled { get; private set; }

        public bool LastVerbose { get; private set; }

        public DomainScanResult? LastResult { get; private set; }

        public void Render(DomainScanResult result, bool verbose = false)
        {
            WasCalled = true;
            LastVerbose = verbose;
            LastResult = result;
            _events?.Add("render");
        }
    }
}
