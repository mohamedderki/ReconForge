using ReconForge.Core.Models;
using ReconForgeIpAddress = ReconForge.Core.Models.IpAddress;

namespace ReconForge.Tests.TestData;

public static class SampleScanResultFactory
{
    public static readonly DateTimeOffset Timestamp = new(
        year: 2026,
        month: 6,
        day: 13,
        hour: 12,
        minute: 0,
        second: 0,
        offset: TimeSpan.Zero);

    public static DomainScanResult CreateDomainScanResult()
    {
        var subdomains = new[]
        {
            CreateSubdomain()
        };
        var ipAddresses = new[]
        {
            CreateIpAddress()
        };
        var portResults = new[]
        {
            CreatePortScanResult()
        };

        return new DomainScanResult(
            "example.com",
            "example.com",
            Timestamp,
            Timestamp,
            DomainScanStatus.Success,
            "OK",
            Array.Empty<string>(),
            subdomains,
            subdomains.Length,
            SubdomainDiscoveryStatus.Success,
            ipAddresses,
            ipAddresses.Length,
            Array.Empty<string>(),
            IpResolutionStatus.Success,
            portResults,
            CheckedPortCount: 2,
            OpenPortCount: 1,
            TimedOutPortCount: 0,
            PortScanErrorCount: 0,
            PortScanStatus.Success);
    }

    public static Subdomain CreateSubdomain(
        string name = "www",
        string rootDomain = "example.com")
    {
        return new Subdomain(
            name,
            rootDomain,
            $"{name}.{rootDomain}",
            "Test",
            Timestamp);
    }

    public static ReconForgeIpAddress CreateIpAddress(
        string address = "192.0.2.10",
        string relatedHost = "www.example.com")
    {
        return new ReconForgeIpAddress(
            address,
            "InterNetwork",
            relatedHost,
            Timestamp,
            IsPrivate: false,
            IsLoopback: false);
    }

    public static PortScanResult CreatePortScanResult(string targetIpAddress = "192.0.2.10")
    {
        return new PortScanResult(
            targetIpAddress,
            Timestamp,
            Timestamp,
            PortScanStatus.Success,
            "OK",
            [
                CreatePort(80, "HTTP", PortState.Open, 12),
                CreatePort(443, "HTTPS", PortState.Closed, 8)
            ],
            Array.Empty<string>(),
            Array.Empty<string>());
    }

    public static Port CreatePort(
        int number,
        string serviceName,
        PortState state,
        long? responseTimeMs)
    {
        return new Port(
            number,
            "TCP",
            serviceName,
            state,
            Timestamp,
            responseTimeMs);
    }
}
