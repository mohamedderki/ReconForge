using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Scanning.Domain;
using ReconForge.Tests.TestData;
using DomainModel = ReconForge.Core.Models.Domain;

namespace ReconForge.Tests.Workflow;

public sealed class ScanWorkflowOrderTests
{
    [Fact]
    public async Task ScanAsync_CallsScanStepsInExpectedOrder()
    {
        var events = new List<string>();
        var service = CreateService(events);

        var result = await service.ScanAsync("Example.COM");

        Assert.Equal(new[] { "domain-validation", "subdomain-discovery", "ip-resolution", "port-scan" }, events);
        Assert.Equal(DomainScanStatus.Success, result.Status);
        Assert.Equal("example.com", result.NormalizedDomain);
        Assert.NotEmpty(result.DiscoveredSubdomains);
        Assert.NotEmpty(result.ResolvedIpAddresses);
        Assert.NotEmpty(result.PortScanResults);
    }

    [Fact]
    public async Task ScanAsync_DoesNotContinue_WhenDomainValidationFails()
    {
        var events = new List<string>();
        var service = CreateService(events, validationSucceeds: false);

        var result = await service.ScanAsync("bad domain");

        Assert.Equal(new[] { "domain-validation" }, events);
        Assert.Equal(DomainScanStatus.InvalidInput, result.Status);
        Assert.Empty(result.DiscoveredSubdomains);
        Assert.Empty(result.ResolvedIpAddresses);
        Assert.Empty(result.PortScanResults);
    }

    [Fact]
    public async Task ScanAsync_ReturnsStructuredResult()
    {
        var service = CreateService([]);

        var result = await service.ScanAsync("example.com");

        Assert.Equal("example.com", result.TargetDomain);
        Assert.Equal("example.com", result.NormalizedDomain);
        Assert.Equal(SubdomainDiscoveryStatus.Success, result.SubdomainDiscoveryStatus);
        Assert.Equal(IpResolutionStatus.Success, result.IpResolutionStatus);
        Assert.Equal(PortScanStatus.Success, result.PortScanStatus);
        Assert.Equal(1, result.DiscoveredSubdomainCount);
        Assert.Equal(1, result.ResolvedIpAddressCount);
        Assert.Equal(2, result.CheckedPortCount);
        Assert.Equal(1, result.OpenPortCount);
    }

    private static DomainScanService CreateService(
        List<string> events,
        bool validationSucceeds = true)
    {
        return new DomainScanService(
            new RecordingDomainValidator(events, validationSucceeds),
            new RecordingSubdomainDiscoveryService(events),
            new RecordingIpResolutionService(events),
            new RecordingPortScannerService(events),
            NullLogger<DomainScanService>.Instance);
    }

    private sealed class RecordingDomainValidator : IDomainValidator
    {
        private readonly List<string> _events;
        private readonly bool _validationSucceeds;

        public RecordingDomainValidator(List<string> events, bool validationSucceeds)
        {
            _events = events;
            _validationSucceeds = validationSucceeds;
        }

        public DomainValidationResult Validate(string? input)
        {
            _events.Add("domain-validation");

            return _validationSucceeds
                ? DomainValidationResult.Valid(new DomainModel(input ?? string.Empty, "example.com", SampleScanResultFactory.Timestamp, IsValid: true))
                : DomainValidationResult.Invalid("Invalid domain.");
        }
    }

    private sealed class RecordingSubdomainDiscoveryService : ISubdomainDiscoveryService
    {
        private readonly List<string> _events;

        public RecordingSubdomainDiscoveryService(List<string> events)
        {
            _events = events;
        }

        public Task<SubdomainDiscoveryResult> DiscoverAsync(
            string rootDomain,
            IEnumerable<string>? customPrefixes = null,
            CancellationToken cancellationToken = default)
        {
            _events.Add("subdomain-discovery");
            var subdomains = new[] { SampleScanResultFactory.CreateSubdomain(rootDomain: rootDomain) };

            return Task.FromResult(new SubdomainDiscoveryResult(
                rootDomain,
                SampleScanResultFactory.Timestamp,
                SampleScanResultFactory.Timestamp,
                SubdomainDiscoveryStatus.Success,
                "OK",
                subdomains,
                Array.Empty<string>(),
                Array.Empty<string>()));
        }
    }

    private sealed class RecordingIpResolutionService : IIpResolutionService
    {
        private readonly List<string> _events;

        public RecordingIpResolutionService(List<string> events)
        {
            _events = events;
        }

        public Task<IpResolutionResult> ResolveAsync(
            string rootDomain,
            IEnumerable<Subdomain> subdomains,
            CancellationToken cancellationToken = default)
        {
            _events.Add("ip-resolution");
            var addresses = new[] { SampleScanResultFactory.CreateIpAddress(relatedHost: rootDomain) };

            return Task.FromResult(new IpResolutionResult(
                rootDomain,
                SampleScanResultFactory.Timestamp,
                SampleScanResultFactory.Timestamp,
                IpResolutionStatus.Success,
                "OK",
                addresses,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>()));
        }
    }

    private sealed class RecordingPortScannerService : IPortScannerService
    {
        private readonly List<string> _events;

        public RecordingPortScannerService(List<string> events)
        {
            _events = events;
        }

        public Task<IReadOnlyList<PortScanResult>> ScanAsync(
            IEnumerable<IpAddress> resolvedIpAddresses,
            IEnumerable<int>? ports = null,
            int timeoutMs = 1000,
            CancellationToken cancellationToken = default)
        {
            _events.Add("port-scan");
            IReadOnlyList<PortScanResult> results = [SampleScanResultFactory.CreatePortScanResult()];
            return Task.FromResult(results);
        }
    }
}
