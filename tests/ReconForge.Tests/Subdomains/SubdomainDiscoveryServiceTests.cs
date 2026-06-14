using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Scanning.Subdomains;

namespace ReconForge.Tests.Subdomains;

public sealed class SubdomainDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverAsync_GeneratesExpectedDefaultSubdomains()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com");

        Assert.Equal(SubdomainDiscoveryStatus.Success, result.Status);
        Assert.Equal(7, result.DiscoveredSubdomainCount);
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "www.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "mail.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "api.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "dev.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "test.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "staging.example.com");
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "admin.example.com");
    }

    [Fact]
    public async Task DiscoverAsync_GeneratedSubdomainsContainRootDomain()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com");

        Assert.All(result.DiscoveredSubdomains, subdomain =>
        {
            Assert.Equal("example.com", subdomain.RootDomain);
            Assert.EndsWith(".example.com", subdomain.FullName);
        });
    }

    [Fact]
    public async Task DiscoverAsync_NormalizesGeneratedSubdomains()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("Example.COM.");

        Assert.Equal("example.com", result.TargetDomain);
        Assert.All(result.DiscoveredSubdomains, subdomain =>
        {
            Assert.Equal(subdomain.FullName.ToLowerInvariant(), subdomain.FullName);
            Assert.Equal("example.com", subdomain.RootDomain);
        });
    }

    [Fact]
    public async Task DiscoverAsync_RemovesDuplicatePrefixes()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com", new[] { "WWW", "api", "portal", "portal" });

        Assert.Single(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "www.example.com");
        Assert.Single(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "api.example.com");
        Assert.Single(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "portal.example.com");
    }

    [Fact]
    public async Task DiscoverAsync_UsesRootDomainAndDoesNotGenerateNestedWwwCandidate()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("www.example.com");

        Assert.Equal("example.com", result.TargetDomain);
        Assert.Contains(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "www.example.com");
        Assert.DoesNotContain(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "www.www.example.com");
        Assert.All(result.DiscoveredSubdomains, subdomain =>
        {
            Assert.Equal("example.com", subdomain.RootDomain);
        });
    }

    [Fact]
    public async Task DiscoverAsync_NormalizesCustomPrefixes()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com", new[] { "  Portal. " });

        Assert.Contains(result.DiscoveredSubdomains, subdomain =>
            subdomain.Name == "portal" && subdomain.FullName == "portal.example.com");
    }

    [Fact]
    public async Task DiscoverAsync_IgnoresEmptyPrefixes()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com", new[] { "", "  ", ".", "portal" });

        Assert.Equal(SubdomainDiscoveryStatus.PartialSuccess, result.Status);
        Assert.Single(result.DiscoveredSubdomains, subdomain => subdomain.FullName == "portal.example.com");
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task DiscoverAsync_HandlesEmptyCustomPrefixListGracefully()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com", Array.Empty<string>());

        Assert.Equal(SubdomainDiscoveryStatus.Success, result.Status);
        Assert.Equal(7, result.DiscoveredSubdomainCount);
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsInvalidInputStatus_ForEmptyRootDomain()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("");

        Assert.Equal(SubdomainDiscoveryStatus.InvalidInput, result.Status);
        Assert.Empty(result.DiscoveredSubdomains);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task DiscoverAsync_ModelDoesNotContainIpOrPortInformation()
    {
        var service = CreateService();

        var result = await service.DiscoverAsync("example.com");
        var propertyNames = typeof(Subdomain).GetProperties().Select(property => property.Name).ToArray();

        Assert.NotEmpty(result.DiscoveredSubdomains);
        Assert.DoesNotContain("IpAddress", propertyNames);
        Assert.DoesNotContain("IPAddress", propertyNames);
        Assert.DoesNotContain("Port", propertyNames);
        Assert.DoesNotContain("Ports", propertyNames);
    }

    private static SubdomainDiscoveryService CreateService()
    {
        return new SubdomainDiscoveryService(NullLogger<SubdomainDiscoveryService>.Instance);
    }
}
