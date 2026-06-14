using Microsoft.Extensions.Logging;
using ReconForge.Cli.Presentation;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Exporting;
using ReconForge.Core.Models;
using Spectre.Console;

namespace ReconForge.Cli.Commands;

public sealed class CliScanExecutor
{
    private readonly IDomainScanService _domainScanService;
    private readonly IResultExporterFactory<DomainScanResult> _exporterFactory;
    private readonly IExportPathResolver _exportPathResolver;
    private readonly IScanResultConsoleRenderer _resultRenderer;
    private readonly ILogger<CliScanExecutor> _logger;

    public CliScanExecutor(
        IDomainScanService domainScanService,
        IResultExporterFactory<DomainScanResult> exporterFactory,
        IExportPathResolver exportPathResolver,
        IScanResultConsoleRenderer resultRenderer,
        ILogger<CliScanExecutor> logger)
    {
        _domainScanService = domainScanService;
        _exporterFactory = exporterFactory;
        _exportPathResolver = exportPathResolver;
        _resultRenderer = resultRenderer;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(RootCommand.Settings settings, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Application started.");

        if (settings.Verbose)
        {
            _logger.LogInformation("Verbose logging enabled.");
        }

        if (settings.ScanTarget is null)
        {
            _logger.LogWarning("Validation failed: scan target missing.");
            AnsiConsole.MarkupLine("[red]Error:[/] Provide a domain with [bold]--scan[/], for example [bold]--scan example.com[/].");
            return 1;
        }

        _logger.LogInformation("Scan option received.");
        _logger.LogInformation("Domain received: {Domain}", settings.ScanTarget);

        if (!settings.Export)
        {
            WarnAboutIgnoredExportOptions(settings);
        }

        var exportFormat = ExportFormat.Json;
        if (settings.Export
            && !ExportFormatParser.TryParse(settings.Format, out exportFormat, out var formatError))
        {
            _logger.LogWarning("Validation failed: unsupported export format {Format}", settings.Format);
            AnsiConsole.MarkupLine("[red]Error:[/] {0}", Markup.Escape(formatError));
            return 1;
        }

        try
        {
            _logger.LogInformation("Scan started for {Domain}", settings.ScanTarget);
            _logger.LogInformation("Scan service called for {Domain}", settings.ScanTarget);

            var result = await _domainScanService.ScanAsync(settings.ScanTarget, cancellationToken);

            _logger.LogInformation("Terminal rendering started.");
            _resultRenderer.Render(result, settings.Verbose);
            _logger.LogInformation("Terminal rendering completed.");

            if (result.Status != DomainScanStatus.Success)
            {
                return 1;
            }

            if (!settings.Export)
            {
                _logger.LogInformation("Export skipped because it was not requested.");
                _logger.LogInformation("Command completed.");
                return 0;
            }

            _logger.LogInformation("Export requested.");

            var pathResolution = _exportPathResolver.Resolve(
                result,
                exportFormat,
                settings.OutputPath);

            if (!pathResolution.Success || string.IsNullOrWhiteSpace(pathResolution.OutputPath))
            {
                var message = pathResolution.ErrorMessage ?? "Unable to resolve export output path.";
                _logger.LogWarning("Export path resolution failed: {Reason}", message);
                _logger.LogWarning("Export failed.");
                AnsiConsole.MarkupLine("[red]Error:[/] {0}", Markup.Escape(message));
                return 1;
            }

            _logger.LogInformation("Selected export format: {ExportFormat}", exportFormat);
            _logger.LogInformation("Output path selected: {OutputPath}", pathResolution.OutputPath);

            var exporter = _exporterFactory.Create(exportFormat);
            var exportResult = await exporter.ExportAsync(
                result,
                pathResolution.OutputPath,
                cancellationToken);

            if (!exportResult.Success)
            {
                var message = exportResult.ErrorMessage ?? exportResult.Message;
                _logger.LogWarning("Export failed: {Reason}", message);
                AnsiConsole.MarkupLine("[red]Export failed:[/] {0}", Markup.Escape(message));
                return 1;
            }

            AnsiConsole.MarkupLine("[green]Results exported successfully to:[/] {0}", Markup.Escape(exportResult.OutputPath));

            _logger.LogInformation("Export completed.");
            _logger.LogInformation("Command completed.");
            return 0;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected exception.");
            AnsiConsole.MarkupLine("[red]Unexpected error:[/] {0}", Markup.Escape(exception.Message));
            return 1;
        }
    }

    private void WarnAboutIgnoredExportOptions(RootCommand.Settings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Format))
        {
            _logger.LogWarning("The format option was ignored because export was not enabled.");
            AnsiConsole.MarkupLine("[yellow]Warning:[/] [bold]--format[/] is ignored unless [bold]--export[/] is provided.");
        }

        if (!string.IsNullOrWhiteSpace(settings.OutputPath))
        {
            _logger.LogWarning("The output option was ignored because export was not enabled.");
            AnsiConsole.MarkupLine("[yellow]Warning:[/] [bold]--output[/] requires [bold]--export[/] and will be ignored.");
        }
    }

}
