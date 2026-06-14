using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForgeIpAddress = ReconForge.Core.Models.IpAddress;

namespace ReconForge.Scanning.Ip;

public sealed class IpResolutionService : IIpResolutionService
{
    private readonly IDnsLookupClient _dnsLookupClient;
    private readonly ILogger<IpResolutionService> _logger;

    public IpResolutionService(
        IDnsLookupClient dnsLookupClient,
        ILogger<IpResolutionService> logger)
    {
        _dnsLookupClient = dnsLookupClient;
        _logger = logger;
    }

    public async Task<IpResolutionResult> ResolveAsync(
        string primaryHost,
        IEnumerable<Subdomain> subdomains,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startedAt = DateTimeOffset.UtcNow;
        var normalizedPrimaryHost = NormalizeHost(primaryHost);
        var subdomainList = subdomains.ToList();

        _logger.LogInformation("IP Resolution started.");
        _logger.LogInformation("Primary host received: {PrimaryHost}", normalizedPrimaryHost);
        _logger.LogInformation("Subdomains received: {SubdomainCount}", subdomainList.Count);

        try
        {
            var hosts = BuildHostList(normalizedPrimaryHost, subdomainList);
            var resolvedAddresses = new List<ReconForgeIpAddress>();
            var failedHosts = new List<string>();
            var errors = new List<string>();
            var warnings = new List<string>();
            var seenAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (hosts.Count == 0)
            {
                _logger.LogWarning("IP Resolution completed with no hosts to resolve.");
                warnings.Add("No hosts were provided for IP resolution.");

                return new IpResolutionResult(
                    normalizedPrimaryHost,
                    startedAt,
                    DateTimeOffset.UtcNow,
                    IpResolutionStatus.NoResults,
                    "No hosts were available for IP resolution.",
                    resolvedAddresses,
                    failedHosts,
                    errors,
                    warnings);
            }

            foreach (var host in hosts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Host resolution started: {Host}", host);

                try
                {
                    var addresses = await _dnsLookupClient.GetHostAddressesAsync(host, cancellationToken);

                    if (addresses.Count == 0)
                    {
                        _logger.LogWarning("Host resolution failed for {Host}: no DNS addresses returned.", host);
                        failedHosts.Add(host);
                        warnings.Add($"No DNS addresses returned for {host}.");
                        continue;
                    }

                    foreach (var address in addresses)
                    {
                        var addressText = address.ToString();

                        if (!seenAddresses.Add(addressText))
                        {
                            _logger.LogInformation("Duplicate IP removed: {IpAddress}", addressText);
                            continue;
                        }

                        resolvedAddresses.Add(new ReconForgeIpAddress(
                            addressText,
                            address.AddressFamily.ToString(),
                            host,
                            DateTimeOffset.UtcNow,
                            IsPrivateAddress(address),
                            IPAddress.IsLoopback(address)));
                    }

                    _logger.LogInformation("Host resolved successfully: {Host}", host);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(
                        "Host resolution failed for {Host}: {Reason}",
                        host,
                        exception.Message);
                    failedHosts.Add(host);
                    errors.Add($"{host}: {exception.Message}");
                }
            }

            var status = DetermineStatus(resolvedAddresses.Count, failedHosts.Count);
            var message = status switch
            {
                IpResolutionStatus.Success => "IP resolution completed successfully.",
                IpResolutionStatus.PartialSuccess => "IP resolution completed with some failed hosts.",
                IpResolutionStatus.Failed => "IP resolution failed for all hosts.",
                _ => "IP resolution completed with no results."
            };

            if (status == IpResolutionStatus.NoResults)
            {
                _logger.LogWarning("IP Resolution completed with no resolved IP addresses.");
            }

            _logger.LogInformation("IP Resolution completed.");
            _logger.LogInformation("Resolved IP addresses: {ResolvedIpAddressCount}", resolvedAddresses.Count);

            return new IpResolutionResult(
                normalizedPrimaryHost,
                startedAt,
                DateTimeOffset.UtcNow,
                status,
                message,
                resolvedAddresses,
                failedHosts,
                errors,
                warnings);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Unexpected exception during IP Resolution.");

            return new IpResolutionResult(
                normalizedPrimaryHost,
                startedAt,
                DateTimeOffset.UtcNow,
                IpResolutionStatus.Failed,
                "IP resolution failed unexpectedly.",
                Array.Empty<ReconForgeIpAddress>(),
                Array.Empty<string>(),
                new[] { exception.Message },
                Array.Empty<string>());
        }
    }

    private static IReadOnlyList<string> BuildHostList(
        string normalizedPrimaryHost,
        IEnumerable<Subdomain> subdomains)
    {
        var hosts = new List<string>();
        var seenHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddHost(normalizedPrimaryHost, hosts, seenHosts);

        foreach (var subdomain in subdomains)
        {
            AddHost(subdomain.FullName, hosts, seenHosts);
        }

        return hosts;
    }

    private static void AddHost(
        string? host,
        List<string> hosts,
        HashSet<string> seenHosts)
    {
        var normalizedHost = NormalizeHost(host);

        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            return;
        }

        if (seenHosts.Add(normalizedHost))
        {
            hosts.Add(normalizedHost);
        }
    }

    private static string NormalizeHost(string? host)
    {
        return (host ?? string.Empty)
            .Trim()
            .TrimEnd('.')
            .ToLowerInvariant();
    }

    private static IpResolutionStatus DetermineStatus(int resolvedAddressCount, int failedHostCount)
    {
        if (resolvedAddressCount > 0 && failedHostCount == 0)
        {
            return IpResolutionStatus.Success;
        }

        if (resolvedAddressCount > 0)
        {
            return IpResolutionStatus.PartialSuccess;
        }

        return failedHostCount > 0
            ? IpResolutionStatus.Failed
            : IpResolutionStatus.NoResults;
    }

    private static bool IsPrivateAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();

            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return (bytes[0] & 0xfe) == 0xfc;
        }

        return false;
    }
}
