using Microsoft.Extensions.Logging.Abstractions;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Scanning.Ports;
using ReconForge.Tests.TestData;
using ReconForge.Tests.TestDoubles;
using ReconForgeIpAddress = ReconForge.Core.Models.IpAddress;

namespace ReconForge.Tests.PortScanning;

public sealed class PortScannerServiceTests
{
    [Fact]
    public async Task ScanAsync_UsesDefaultPortList_WhenNoCustomPortsAreProvided()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")]);

        Assert.Single(results);
        Assert.Equal(14, results[0].CheckedPorts.Count);
        Assert.Equal(new[]
        {
            21, 22, 25, 53, 80, 110, 143, 443, 587, 993, 995, 3306, 5432, 8080
        }, checker.CheckedPorts);
    }

    [Fact]
    public async Task ScanAsync_RemovesDuplicatePorts()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [80, 80, 443, 443]);

        Assert.Equal(new[] { 80, 443 }, checker.CheckedPorts);
        Assert.Equal(2, results[0].CheckedPorts.Count);
    }

    [Fact]
    public async Task ScanAsync_IgnoresInvalidPortNumbers()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [0, -1, 80, 70000]);

        Assert.Equal(new[] { 80 }, checker.CheckedPorts);
        Assert.Single(results[0].CheckedPorts);
        Assert.NotEmpty(results[0].Warnings);
    }

    [Fact]
    public async Task ScanAsync_HandlesEmptyIpAddressListGracefully()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync(Array.Empty<ReconForgeIpAddress>());

        Assert.Empty(results);
        Assert.Empty(checker.CheckedPorts);
    }

    [Fact]
    public async Task ScanAsync_ReturnsSuccess_WhenOpenPortIsFoundWithoutWarnings()
    {
        var checker = new FakeTcpConnectionChecker();
        checker.SetResult("192.0.2.10", 80, new TcpConnectionCheckResult(PortState.Open, 15));
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [80]);

        Assert.Equal(PortScanStatus.Success, results[0].Status);
        Assert.Single(results[0].OpenPorts);
    }

    [Fact]
    public async Task ScanAsync_ReturnsNoOpenPorts_WhenAllPortsAreClosed()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [80, 443]);

        Assert.Equal(PortScanStatus.NoOpenPorts, results[0].Status);
        Assert.Equal(2, results[0].CheckedPorts.Count);
        Assert.Empty(results[0].OpenPorts);
    }

    [Fact]
    public async Task ScanAsync_RecordsTimeouts()
    {
        var checker = new FakeTcpConnectionChecker();
        checker.SetResult("192.0.2.10", 443, new TcpConnectionCheckResult(PortState.Timeout, 1000));
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [80, 443]);

        Assert.Equal(PortScanStatus.NoOpenPorts, results[0].Status);
        Assert.Single(results[0].TimedOutPorts);
        Assert.Empty(results[0].Warnings);
    }

    [Fact]
    public async Task ScanAsync_ResultContainsCheckedPorts()
    {
        var checker = new FakeTcpConnectionChecker();
        var service = CreateService(checker);

        var results = await service.ScanAsync([CreateIpAddress("192.0.2.10")], [22, 80]);

        Assert.Equal(2, results[0].CheckedPorts.Count);
        Assert.Contains(results[0].CheckedPorts, port => port.Number == 22 && port.ServiceName == "SSH");
        Assert.Contains(results[0].CheckedPorts, port => port.Number == 80 && port.ServiceName == "HTTP");
    }

    [Fact]
    public void PortScannerService_DoesNotDependOnDnsResolution()
    {
        var constructorParameterTypes = typeof(PortScannerService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        Assert.DoesNotContain(typeof(IDnsLookupClient), constructorParameterTypes);
    }

    [Fact]
    public void PortScannerService_DoesNotDependOnExportLogic()
    {
        var constructorParameterTypes = typeof(PortScannerService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        Assert.DoesNotContain(typeof(IResultExporter<DomainScanResult>), constructorParameterTypes);
    }

    private static PortScannerService CreateService(FakeTcpConnectionChecker checker)
    {
        return new PortScannerService(
            checker,
            NullLogger<PortScannerService>.Instance);
    }

    private static ReconForgeIpAddress CreateIpAddress(string address)
    {
        return SampleScanResultFactory.CreateIpAddress(address, "example.com");
    }
}
