using ReconForge.Core.Models;
using Spectre.Console;

namespace ReconForge.Cli.Presentation;

public sealed class ScanResultConsoleRenderer : IScanResultConsoleRenderer
{
    private readonly IAnsiConsole _console;
    private readonly CliTheme _theme;

    public ScanResultConsoleRenderer(IAnsiConsole console, CliTheme? theme = null)
    {
        _console = console;
        _theme = theme ?? CliTheme.Default;
    }

    public void Render(DomainScanResult result, bool verbose = false)
    {
        _console.Write(new Rule($"[{_theme.PrimaryColor}]ReconForge Scan Results[/]").RuleStyle(_theme.PrimaryColor));
        _console.WriteLine();

        RenderTargetInformation(result);
        RenderSubdomains(result);
        RenderIpAddresses(result);
        RenderPortResults(result, verbose);
        RenderSummary(result);
        RenderIssues(result, verbose);
    }

    private void RenderTargetInformation(DomainScanResult result)
    {
        var duration = result.FinishedAt - result.StartedAt;
        var table = CreateKeyValueTable("Target Information");

        table.AddRow("Original input", Escape(result.TargetDomain));
        table.AddRow("Normalized host", Escape(result.NormalizedDomain ?? "Not available"));
        table.AddRow("Root domain", Escape(result.RootDomain ?? "Not available"));
        table.AddRow("Scan start time", Escape(result.StartedAt.ToString("u")));
        table.AddRow("Scan end time", Escape(result.FinishedAt.ToString("u")));
        table.AddRow("Duration", Escape(FormatDuration(duration)));
        table.AddRow("Status", FormatStatus(result.Status));

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderSubdomains(DomainScanResult result)
    {
        WriteSection("Subdomains");

        if (result.DiscoveredSubdomains.Count == 0)
        {
            WriteMuted("No subdomains found.");
            return;
        }

        var table = new Table();
        table.AddColumn("Subdomain");
        table.AddColumn("Source");
        table.AddColumn("Discovered At");

        foreach (var subdomain in result.DiscoveredSubdomains)
        {
            table.AddRow(
                Escape(subdomain.FullName),
                Escape(subdomain.Source),
                Escape(subdomain.DiscoveredAt.ToString("u")));
        }

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderIpAddresses(DomainScanResult result)
    {
        WriteSection("Resolved IP Addresses");

        if (result.ResolvedIpAddresses.Count == 0)
        {
            WriteMuted("No IP addresses resolved.");
            return;
        }

        var table = new Table();
        table.AddColumn("Host");
        table.AddColumn("IP Address");
        table.AddColumn("Address Family");
        table.AddColumn("Is Private");
        table.AddColumn("Is Loopback");

        foreach (var ipAddress in result.ResolvedIpAddresses)
        {
            table.AddRow(
                Escape(ipAddress.RelatedHost),
                Escape(ipAddress.Address),
                Escape(ipAddress.AddressFamily),
                FormatBoolean(ipAddress.IsPrivate),
                FormatBoolean(ipAddress.IsLoopback));
        }

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderPortResults(DomainScanResult result, bool verbose)
    {
        WriteSection(verbose ? "Port Scan Results" : "Open Ports");

        var portEntries = result.PortScanResults
            .SelectMany(portResult => portResult.CheckedPorts.Select(port => new
            {
                portResult.TargetIpAddress,
                Port = port
            }))
            .Where(entry => verbose || entry.Port.State == PortState.Open)
            .ToArray();

        if (portEntries.Length == 0)
        {
            WriteMuted(verbose ? "No port results available." : "No open ports detected.");
            return;
        }

        var table = new Table();
        table.AddColumn("IP Address");
        table.AddColumn("Port");
        table.AddColumn("Protocol");
        table.AddColumn("State");
        table.AddColumn("Service Name");
        table.AddColumn("Response Time");

        foreach (var entry in portEntries)
        {
            table.AddRow(
                Escape(entry.TargetIpAddress),
                entry.Port.Number.ToString(),
                Escape(entry.Port.Protocol),
                FormatPortState(entry.Port.State),
                Escape(entry.Port.ServiceName),
                Escape(FormatResponseTime(entry.Port.ResponseTimeMs)));
        }

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderSummary(DomainScanResult result)
    {
        var table = CreateKeyValueTable("Summary");

        table.AddRow("Subdomains discovered", result.DiscoveredSubdomainCount.ToString());
        table.AddRow("Hosts resolved", CountResolvedHosts(result).ToString());
        table.AddRow("IP addresses resolved", result.ResolvedIpAddressCount.ToString());
        table.AddRow("Ports checked", result.CheckedPortCount.ToString());
        table.AddRow("Open ports", result.OpenPortCount.ToString());
        table.AddRow("Timed out ports", result.TimedOutPortCount.ToString());
        table.AddRow("Failed host resolutions", result.FailedHosts.Count.ToString());
        table.AddRow("Real warnings", CountWarnings(result).ToString());
        table.AddRow("Errors", CountErrors(result).ToString());

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderIssues(DomainScanResult result, bool verbose)
    {
        var issues = CollectIssues(result, verbose).ToArray();
        if (issues.Length == 0)
        {
            return;
        }

        WriteSection("Warnings and Errors");

        foreach (var issue in issues)
        {
            _console.MarkupLine("[{0}]![/] {1}", _theme.WarningColor, Escape(issue));
        }

        _console.WriteLine();
    }

    private static IEnumerable<string> CollectIssues(DomainScanResult result, bool verbose)
    {
        foreach (var error in result.ValidationErrors)
        {
            yield return error;
        }

        if (result.FailedHosts.Count > 0)
        {
            if (verbose)
            {
                foreach (var failedHost in result.FailedHosts)
                {
                    yield return $"Host resolution failed: {failedHost}";
                }
            }
            else
            {
                yield return $"{result.FailedHosts.Count} hosts could not be resolved. Use --verbose for details.";
            }
        }

        foreach (var portResult in result.PortScanResults)
        {
            foreach (var warning in portResult.Warnings)
            {
                yield return $"{portResult.TargetIpAddress}: {warning}";
            }

            foreach (var error in portResult.Errors)
            {
                yield return $"{portResult.TargetIpAddress}: {error}";
            }

            if (!verbose)
            {
                continue;
            }

            foreach (var port in portResult.TimedOutPorts)
            {
                yield return $"Port timeout: {portResult.TargetIpAddress}:{port.Number}";
            }
        }
    }

    private static int CountResolvedHosts(DomainScanResult result)
    {
        return result.ResolvedIpAddresses
            .Select(address => address.RelatedHost)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private static int CountWarnings(DomainScanResult result)
    {
        return result.PortScanResults.Sum(portResult => portResult.Warnings.Count);
    }

    private static int CountErrors(DomainScanResult result)
    {
        return result.ValidationErrors.Count
            + result.PortScanResults.Sum(portResult => portResult.Errors.Count);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalMilliseconds < 1000
            ? $"{duration.TotalMilliseconds:0} ms"
            : $"{duration.TotalSeconds:0.00} s";
    }

    private string FormatStatus(DomainScanStatus status)
    {
        var color = status == DomainScanStatus.Success
            ? _theme.SuccessColor
            : _theme.ErrorColor;

        return $"[{color}]{Escape(status.ToString())}[/]";
    }

    private string FormatPortState(PortState state)
    {
        var color = state switch
        {
            PortState.Open => _theme.SuccessColor,
            PortState.Timeout => _theme.WarningColor,
            PortState.Error => _theme.ErrorColor,
            _ => _theme.MutedColor
        };

        return $"[{color}]{Escape(state.ToString())}[/]";
    }

    private string FormatBoolean(bool value)
    {
        return value
            ? $"[{_theme.WarningColor}]Yes[/]"
            : $"[{_theme.MutedColor}]No[/]";
    }

    private static string FormatResponseTime(long? responseTimeMs)
    {
        return responseTimeMs.HasValue
            ? $"{responseTimeMs.Value} ms"
            : "Not available";
    }

    private static Table CreateKeyValueTable(string title)
    {
        var table = new Table().Title(title);
        table.AddColumn("Field");
        table.AddColumn("Value");
        return table;
    }

    private void WriteSection(string title)
    {
        _console.MarkupLine("[bold {0}]{1}[/]", _theme.PrimaryColor, Escape(title));
    }

    private void WriteMuted(string message)
    {
        _console.MarkupLine("[{0}]{1}[/]", _theme.MutedColor, Escape(message));
        _console.WriteLine();
    }

    private static string Escape(string value)
    {
        return Markup.Escape(value);
    }
}
