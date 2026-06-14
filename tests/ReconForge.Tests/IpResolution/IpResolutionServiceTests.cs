using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Scanning.Ip;
using ReconForge.Tests.TestData;
using ReconForge.Tests.TestDoubles;
using ReconForgeIpAddress = ReconForge.Core.Models.IpAddress;

namespace ReconForge.Tests.IpResolution;

public sealed class IpResolutionServiceTests
{
    [Fact]
    public async Task ResolveAsync_ReturnsSuccess_ForSuccessfulDnsResolution()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"));
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", Array.Empty<Subdomain>());

        Assert.Equal(IpResolutionStatus.Success, result.Status);
        var address = Assert.Single(result.ResolvedAddresses);
        Assert.Equal("192.0.2.10", address.Address);
        Assert.Equal("example.com", address.RelatedHost);
    }

    [Fact]
    public async Task ResolveAsync_HandlesEmptyHostListGracefully()
    {
        var dnsClient = new FakeDnsLookupClient();
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync(" ", Array.Empty<Subdomain>());

        Assert.Equal(IpResolutionStatus.NoResults, result.Status);
        Assert.Empty(result.ResolvedAddresses);
        Assert.Empty(result.FailedHosts);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task ResolveAsync_RemovesDuplicateIpAddresses()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"));
        dnsClient.SetAddress("www.example.com", IPAddress.Parse("192.0.2.10"));
        dnsClient.SetAddress("api.example.com", IPAddress.Parse("192.0.2.11"));
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", new[]
        {
            CreateSubdomain("www", "example.com"),
            CreateSubdomain("api", "example.com")
        });

        Assert.Equal(IpResolutionStatus.Success, result.Status);
        Assert.Equal(2, result.ResolvedAddressCount);
        Assert.Single(result.ResolvedAddresses, address => address.Address == "192.0.2.10");
        Assert.Single(result.ResolvedAddresses, address => address.Address == "192.0.2.11");
    }

    [Fact]
    public async Task ResolveAsync_StoresFailedHostsSeparately()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"));
        dnsClient.SetFailure("missing.example.com");
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", new[]
        {
            CreateSubdomain("missing", "example.com")
        });

        Assert.Equal(IpResolutionStatus.PartialSuccess, result.Status);
        Assert.Contains("missing.example.com", result.FailedHosts);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsFailed_WhenAllHostsFail()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetFailure("example.com");
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", Array.Empty<Subdomain>());

        Assert.Equal(IpResolutionStatus.Failed, result.Status);
        Assert.Equal(0, result.ResolvedAddressCount);
        Assert.Contains("example.com", result.FailedHosts);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsCorrectResolvedIpCount()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"), IPAddress.Parse("2001:db8::10"));
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", Array.Empty<Subdomain>());

        Assert.Equal(IpResolutionStatus.Success, result.Status);
        Assert.Equal(2, result.ResolvedAddressCount);
    }

    [Fact]
    public async Task ResolveAsync_ContinuesIfOneHostFails()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetFailure("example.com");
        dnsClient.SetAddress("www.example.com", IPAddress.Parse("192.0.2.10"));
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", new[]
        {
            CreateSubdomain("www", "example.com")
        });

        Assert.Equal(IpResolutionStatus.PartialSuccess, result.Status);
        Assert.Contains("example.com", result.FailedHosts);
        Assert.Contains(result.ResolvedAddresses, address => address.RelatedHost == "www.example.com");
    }

    [Fact]
    public async Task ResolveAsync_ModelDoesNotContainPortInformation()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"));
        var service = CreateService(dnsClient);

        var result = await service.ResolveAsync("example.com", Array.Empty<Subdomain>());
        var propertyNames = typeof(ReconForgeIpAddress).GetProperties().Select(property => property.Name).ToArray();

        Assert.NotEmpty(result.ResolvedAddresses);
        Assert.DoesNotContain("Port", propertyNames);
        Assert.DoesNotContain("Ports", propertyNames);
    }

    [Fact]
    public async Task ResolveAsync_UsesDnsLookupOnlyAndDoesNotPerformPortScanning()
    {
        var dnsClient = new FakeDnsLookupClient();
        dnsClient.SetAddress("example.com", IPAddress.Parse("192.0.2.10"));
        var service = CreateService(dnsClient);

        await service.ResolveAsync("example.com", Array.Empty<Subdomain>());

        Assert.Equal(new[] { "example.com" }, dnsClient.RequestedHosts);
    }

    private static IpResolutionService CreateService(FakeDnsLookupClient dnsClient)
    {
        return new IpResolutionService(
            dnsClient,
            NullLogger<IpResolutionService>.Instance);
    }

    private static Subdomain CreateSubdomain(string prefix, string rootDomain)
    {
        return SampleScanResultFactory.CreateSubdomain(prefix, rootDomain);
    }
}
