using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Core.Validation;
using ReconForge.Scanning.Domain;

namespace ReconForge.Tests.Workflow;

public sealed class DomainScanServiceTests
{
    [Fact]
    public async Task ScanAsync_ReturnsSuccess_ForValidDomain()
    {
        var service = CreateService();

        var result = await service.ScanAsync("Example.COM");

        Assert.Equal(DomainScanStatus.Success, result.Status);
        Assert.Equal("Example.COM", result.TargetDomain);
        Assert.Equal("example.com", result.NormalizedDomain);
        Assert.Empty(result.ValidationErrors);
        Assert.Equal(SubdomainDiscoveryStatus.Success, result.SubdomainDiscoveryStatus);
        Assert.Equal(7, result.DiscoveredSubdomainCount);
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "www.example.com");
        Assert.Equal(IpResolutionStatus.Success, result.IpResolutionStatus);
        Assert.Equal(2, result.ResolvedIpAddressCount);
        Assert.Contains(result.ResolvedIpAddresses, address => address.Address == "192.0.2.10");
        Assert.Equal(PortScanStatus.Success, result.PortScanStatus);
        Assert.Equal(2, result.CheckedPortCount);
        Assert.Equal(1, result.OpenPortCount);
    }

    [Fact]
    public async Task ScanAsync_ReturnsInvalidInput_ForInvalidDomain()
    {
        var service = CreateService();

        var result = await service.ScanAsync("invalid domain");

        Assert.Equal(DomainScanStatus.InvalidInput, result.Status);
        Assert.Null(result.NormalizedDomain);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Equal(IpResolutionStatus.NoResults, result.IpResolutionStatus);
        Assert.Empty(result.ResolvedIpAddresses);
        Assert.Equal(PortScanStatus.NoOpenPorts, result.PortScanStatus);
        Assert.Empty(result.PortScanResults);
    }

    private static DomainScanService CreateService()
    {
        return new DomainScanService(
            new DomainNameValidator(),
            new StubSubdomainDiscoveryService(),
            new StubIpResolutionService(),
            new StubPortScannerService(),
            NullLogger<DomainScanService>.Instance);
    }

    private sealed class StubSubdomainDiscoveryService : ISubdomainDiscoveryService
    {
        public Task<SubdomainDiscoveryResult> DiscoverAsync(
            string rootDomain,
            IEnumerable<string>? customPrefixes = null,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var subdomains = new[]
            {
                new Subdomain("www", rootDomain, $"www.{rootDomain}", "Test", now),
                new Subdomain("mail", rootDomain, $"mail.{rootDomain}", "Test", now),
                new Subdomain("api", rootDomain, $"api.{rootDomain}", "Test", now),
                new Subdomain("dev", rootDomain, $"dev.{rootDomain}", "Test", now),
                new Subdomain("test", rootDomain, $"test.{rootDomain}", "Test", now),
                new Subdomain("staging", rootDomain, $"staging.{rootDomain}", "Test", now),
                new Subdomain("admin", rootDomain, $"admin.{rootDomain}", "Test", now)
            };

            return Task.FromResult(new SubdomainDiscoveryResult(
                rootDomain,
                now,
                now,
                SubdomainDiscoveryStatus.Success,
                "OK",
                subdomains,
                Array.Empty<string>(),
                Array.Empty<string>()));
        }
    }

    private sealed class StubIpResolutionService : IIpResolutionService
    {
        public Task<IpResolutionResult> ResolveAsync(
            string rootDomain,
            IEnumerable<Subdomain> subdomains,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var addresses = new[]
            {
                new IpAddress("192.0.2.10", "InterNetwork", rootDomain, now, IsPrivate: false, IsLoopback: false),
                new IpAddress("2001:db8::10", "InterNetworkV6", $"www.{rootDomain}", now, IsPrivate: false, IsLoopback: false)
            };

            return Task.FromResult(new IpResolutionResult(
                rootDomain,
                now,
                now,
                IpResolutionStatus.Success,
                "OK",
                addresses,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>()));
        }
    }

    private sealed class StubPortScannerService : IPortScannerService
    {
        public Task<IReadOnlyList<PortScanResult>> ScanAsync(
            IEnumerable<IpAddress> resolvedIpAddresses,
            IEnumerable<int>? ports = null,
            int timeoutMs = 1000,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            IReadOnlyList<PortScanResult> results =
            [
                new PortScanResult(
                    "192.0.2.10",
                    now,
                    now,
                    PortScanStatus.Success,
                    "OK",
                    [
                        new Port(80, "TCP", "HTTP", PortState.Open, now, 12),
                        new Port(443, "TCP", "HTTPS", PortState.Closed, now, 8)
                    ],
                    Array.Empty<string>(),
                    Array.Empty<string>())
            ];

            return Task.FromResult(results);
        }
    }
}
