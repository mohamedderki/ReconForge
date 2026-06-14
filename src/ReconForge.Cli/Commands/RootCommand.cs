using Spectre.Console.Cli;
using System.ComponentModel;

namespace ReconForge.Cli.Commands;

public sealed class RootCommand : AsyncCommand<RootCommand.Settings>
{
    private readonly CliScanExecutor _executor;

    public RootCommand(CliScanExecutor executor)
    {
        _executor = executor;
    }

    protected override Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        return _executor.ExecuteAsync(settings, cancellationToken);
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--scan <DOMAIN>")]
        [Description("Authorized domain to scan.")]
        public string? ScanTarget { get; init; }

        [CommandOption("--verbose")]
        [Description("Shows detailed workflow logs.")]
        public bool Verbose { get; init; }

        [CommandOption("--export")]
        [Description("Exports the scan result to a file.")]
        public bool Export { get; init; }

        [CommandOption("--output <PATH>")]
        [Description("Output file path used when --export is enabled.")]
        public string? OutputPath { get; init; }

        [CommandOption("--format <FORMAT>")]
        [Description("Export format used with --export. Supported: json, csv, xml, html, yaml. Default: json.")]
        public string? Format { get; init; }
    }
}
