### Prompet:
Ich möchte für meine Softwaretechnik-Abgabe ein Refactoring-Beispiel mit Generativer AI durchführen.

Bitte refaktoriere die folgende Methode aus meinem Projekt ReconForge. 
Die Methode heißt ExecuteAsync und befindet sich in der Klasse CliScanExecutor.

Ich möchte das Fowler-Refactoring Extract Method anwenden.

Bitte beachte:
- Die fachliche Logik darf nicht verändert werden.
- Es sollen keine neuen Features entstehen.
- Das Verhalten soll gleich bleiben.
- Die Methode soll besser lesbar werden.
- Große zusammenhängende Codebereiche sollen in private Hilfsmethoden ausgelagert werden.
- Die neuen Methodennamen sollen verständlich sein.
- async/await und die Rückgabewerte müssen korrekt bleiben.

Hier ist die Methode:

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

Bitte gib mir eine refaktorierte Version der Methode und der neu extrahierten Hilfsmethoden.

___________________________________________________________________________________________________________________________________________________________________________________________________________________________

### Vorher:
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

___________________________________________________________________________________________________________________________________________________________________________________________________________________________

### Nachher:
public async Task<int> ExecuteAsync(RootCommand.Settings settings, CancellationToken cancellationToken = default)
{
    LogApplicationStart(settings);

    if (!ValidateScanTarget(settings))
    {
        return 1;
    }

    LogScanTarget(settings);

    if (!TryResolveExportFormat(settings, out var exportFormat))
    {
        return 1;
    }

    try
    {
        var result = await RunScanAndRenderResultAsync(settings, cancellationToken);

        if (result.Status != DomainScanStatus.Success)
        {
            return 1;
        }

        if (!settings.Export)
        {
            LogExportSkipped();
            return 0;
        }

        return await ExportScanResultAsync(
            result,
            exportFormat,
            settings.OutputPath,
            cancellationToken);
    }
    catch (Exception exception)
    {
        return HandleUnexpectedException(exception);
    }
}

private void LogApplicationStart(RootCommand.Settings settings)
{
    _logger.LogInformation("Application started.");

    if (settings.Verbose)
    {
        _logger.LogInformation("Verbose logging enabled.");
    }
}

private bool ValidateScanTarget(RootCommand.Settings settings)
{
    if (settings.ScanTarget is not null)
    {
        return true;
    }

    _logger.LogWarning("Validation failed: scan target missing.");
    AnsiConsole.MarkupLine("[red]Error:[/] Provide a domain with [bold]--scan[/], for example [bold]--scan example.com[/].");
    return false;
}

private void LogScanTarget(RootCommand.Settings settings)
{
    _logger.LogInformation("Scan option received.");
    _logger.LogInformation("Domain received: {Domain}", settings.ScanTarget);
}

private bool TryResolveExportFormat(
    RootCommand.Settings settings,
    out ExportFormat exportFormat)
{
    exportFormat = ExportFormat.Json;

    if (!settings.Export)
    {
        WarnAboutIgnoredExportOptions(settings);
        return true;
    }

    if (ExportFormatParser.TryParse(settings.Format, out exportFormat, out var formatError))
    {
        return true;
    }

    _logger.LogWarning("Validation failed: unsupported export format {Format}", settings.Format);
    AnsiConsole.MarkupLine("[red]Error:[/] {0}", Markup.Escape(formatError));
    return false;
}

private async Task<DomainScanResult> RunScanAndRenderResultAsync(
    RootCommand.Settings settings,
    CancellationToken cancellationToken)
{
    _logger.LogInformation("Scan started for {Domain}", settings.ScanTarget);
    _logger.LogInformation("Scan service called for {Domain}", settings.ScanTarget);

    var result = await _domainScanService.ScanAsync(settings.ScanTarget, cancellationToken);

    _logger.LogInformation("Terminal rendering started.");
    _resultRenderer.Render(result, settings.Verbose);
    _logger.LogInformation("Terminal rendering completed.");

    return result;
}

private void LogExportSkipped()
{
    _logger.LogInformation("Export skipped because it was not requested.");
    _logger.LogInformation("Command completed.");
}

private async Task<int> ExportScanResultAsync(
    DomainScanResult result,
    ExportFormat exportFormat,
    string? outputPath,
    CancellationToken cancellationToken)
{
    _logger.LogInformation("Export requested.");

    var pathResolution = _exportPathResolver.Resolve(
        result,
        exportFormat,
        outputPath);

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

private int HandleUnexpectedException(Exception exception)
{
    _logger.LogError(exception, "Unexpected exception.");
    AnsiConsole.MarkupLine("[red]Unexpected error:[/] {0}", Markup.Escape(exception.Message));
    return 1;
}