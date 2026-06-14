using ReconForge.Cli.Presentation;
using ReconForge.Core.Models;
using ReconForge.Tests.TestData;
using Spectre.Console;

namespace ReconForge.Tests.Cli.Presentation;

public sealed class ScanResultConsoleRendererTests
{
    [Fact]
    public void Render_DoesNotThrow_WhenResultHasData()
    {
        var result = SampleScanResultFactory.CreateDomainScanResult();

        var exception = Record.Exception(() => RenderToText(result));

        Assert.Null(exception);
    }

    [Fact]
    public void Render_DoesNotThrow_WhenSubdomainListIsEmpty()
    {
        var result = CreateResult(
            subdomains: Array.Empty<Subdomain>(),
            ipAddresses: [SampleScanResultFactory.CreateIpAddress()],
            portResults: [SampleScanResultFactory.CreatePortScanResult()]);

        var exception = Record.Exception(() => RenderToText(result));

        Assert.Null(exception);
    }

    [Fact]
    public void Render_DoesNotThrow_WhenIpListIsEmpty()
    {
        var result = CreateResult(
            subdomains: [SampleScanResultFactory.CreateSubdomain()],
            ipAddresses: Array.Empty<IpAddress>(),
            portResults: [SampleScanResultFactory.CreatePortScanResult()]);

        var exception = Record.Exception(() => RenderToText(result));

        Assert.Null(exception);
    }

    [Fact]
    public void Render_DoesNotThrow_WhenPortResultsAreEmpty()
    {
        var result = CreateResult(
            subdomains: [SampleScanResultFactory.CreateSubdomain()],
            ipAddresses: [SampleScanResultFactory.CreateIpAddress()],
            portResults: Array.Empty<PortScanResult>());

        var exception = Record.Exception(() => RenderToText(result));

        Assert.Null(exception);
    }

    [Fact]
    public void Render_DefaultMode_ShowsOnlyOpenPorts()
    {
        var output = RenderToText(CreateResultWithMixedPorts());

        Assert.Contains("80", output);
        Assert.DoesNotContain("443", output);
        Assert.DoesNotContain("22", output);
        Assert.Contains("Timed out ports", output);
    }

    [Fact]
    public void Render_VerboseMode_ShowsAllPortResults()
    {
        var output = RenderToText(CreateResultWithMixedPorts(), verbose: true);

        Assert.Contains("80", output);
        Assert.Contains("443", output);
        Assert.Contains("22", output);
        Assert.Contains("Timeout", output);
    }

    [Fact]
    public void Render_DefaultMode_DoesNotTreatPortTimeoutsAsWarnings()
    {
        var output = RenderToText(CreateResultWithMixedPorts());

        Assert.Contains("Real warnings", output);
        Assert.Contains("0", output);
        Assert.DoesNotContain("Port timeout:", output);
    }

    [Fact]
    public void Render_DefaultMode_GroupsFailedHostResolutions()
    {
        var result = CreateResult(
            subdomains: [SampleScanResultFactory.CreateSubdomain()],
            ipAddresses: [SampleScanResultFactory.CreateIpAddress()],
            portResults: Array.Empty<PortScanResult>(),
            failedHosts: ["missing.example.com", "offline.example.com"]);

        var output = RenderToText(result);

        Assert.Contains("2 hosts could not be resolved", output);
        Assert.DoesNotContain("missing.example.com", output);
        Assert.DoesNotContain("offline.example.com", output);
    }

    [Fact]
    public void Render_VerboseMode_ShowsFailedHostResolutionDetails()
    {
        var result = CreateResult(
            subdomains: [SampleScanResultFactory.CreateSubdomain()],
            ipAddresses: [SampleScanResultFactory.CreateIpAddress()],
            portResults: Array.Empty<PortScanResult>(),
            failedHosts: ["missing.example.com"]);

        var output = RenderToText(result, verbose: true);

        Assert.Contains("Host resolution failed: missing.example.com", output);
    }

    private static DomainScanResult CreateResult(
        IReadOnlyList<Subdomain> subdomains,
        IReadOnlyList<IpAddress> ipAddresses,
        IReadOnlyList<PortScanResult> portResults,
        IReadOnlyList<string>? failedHosts = null)
    {
        return new DomainScanResult(
            "example.com",
            "example.com",
            SampleScanResultFactory.Timestamp,
            SampleScanResultFactory.Timestamp,
            DomainScanStatus.Success,
            "OK",
            Array.Empty<string>(),
            subdomains,
            subdomains.Count,
            SubdomainDiscoveryStatus.Success,
            ipAddresses,
            ipAddresses.Count,
            failedHosts ?? Array.Empty<string>(),
            ipAddresses.Count == 0 ? IpResolutionStatus.NoResults : IpResolutionStatus.Success,
            portResults,
            portResults.Sum(result => result.CheckedPorts.Count),
            portResults.Sum(result => result.OpenPorts.Count),
            portResults.Sum(result => result.TimedOutPorts.Count),
            portResults.Sum(result => result.Errors.Count),
            portResults.Count == 0 ? PortScanStatus.NoOpenPorts : PortScanStatus.Success)
        {
            RootDomain = "example.com"
        };
    }

    private static DomainScanResult CreateResultWithMixedPorts()
    {
        var portResult = new PortScanResult(
            "192.0.2.10",
            SampleScanResultFactory.Timestamp,
            SampleScanResultFactory.Timestamp,
            PortScanStatus.Success,
            "OK",
            [
                SampleScanResultFactory.CreatePort(80, "HTTP", PortState.Open, 12),
                SampleScanResultFactory.CreatePort(443, "HTTPS", PortState.Closed, 4),
                SampleScanResultFactory.CreatePort(22, "SSH", PortState.Timeout, 1000)
            ],
            Array.Empty<string>(),
            Array.Empty<string>());

        return CreateResult(
            subdomains: [SampleScanResultFactory.CreateSubdomain()],
            ipAddresses: [SampleScanResultFactory.CreateIpAddress()],
            portResults: [portResult]);
    }

    private static string RenderToText(DomainScanResult result, bool verbose = false)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(writer)
        });

        var renderer = new ScanResultConsoleRenderer(console);
        renderer.Render(result, verbose);

        return writer.ToString();
    }
}
